using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL.Entities;

namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface ITeamRepository : IRepository
    {
        Task<IEnumerable<Team>> GetTeams();
        Task<Team> GetTeamById(string teamId);
        Task<Team> GetTeamByExternalId(string teamId, bool excludeDeleted = true);
        Task<Team> InsertOrUpdate(IEducationalClassTeam eduClass, CancellationToken cancellationToken = default);
        Task<Team> InsertOrUpdateWMembers(IEducationalClassTeam eduClass);
        Task<IEnumerable<Member>> GetTeamMember(string teamId);
        Task<Team> GetTeam(string adId, string departmentId, CancellationToken cancellationToken = default);
        Task<string> GetTeamJoinUrl(string adId, string departmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Team>> GetTeams(string annoOfferta, string departmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Team>> GetTeamWithMembers(string adId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Team>> GetTeamsWithMembers(string annoOfferta, CancellationToken cancellationToken = default);
    }

    public class TeamRepository : Repository<TeamsEduDbContext>, ITeamRepository
    {
        public TeamRepository(TeamsEduDbContext context) : base(context) { }

        public async Task<IEnumerable<Team>> GetTeams()
        {
            return await this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Member)
                .ToListAsync();
        }

        private IQueryable<Team> _defaultTeamCollection() => this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Member)
                .AsQueryable();

        private IQueryable<Team> _defaultTeamCollectionWithDeleted() => this._defaultCollectionWithDeleted<Team>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Member)
                .AsQueryable();

        public async Task<Team> GetTeamById(string teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId) || !Guid.TryParse(teamId, out Guid teamGuid))
                return null;

            return await this._defaultTeamCollection()
                .Where(a => a.TeamsId == teamGuid)
                .FirstOrDefaultAsync();
        }

        public async Task<Team> GetTeamByExternalId(string teamId, bool excludeDeleted = true)
        {
            if (string.IsNullOrWhiteSpace(teamId))
                return null;

            if (excludeDeleted)
            {
                return await this._defaultTeamCollection()
                    .Where(a => a.ExternalId == teamId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                return await this._defaultTeamCollectionWithDeleted()
                    .Where(a => a.ExternalId == teamId)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<Team> InsertOrUpdate(IEducationalClassTeam eduClass, CancellationToken cancellationToken = default)
        {
            var team = await this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .Where(a => a.TeamsId == eduClass.Id)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                team = new Team()
                {
                    CreatedOn = DateTime.UtcNow,
                    Description = eduClass.Description,
                    ExternalId = eduClass.Key,
                    Name = eduClass.Name,
                    TeamsId = eduClass.Id.Value,
                    InternalId = eduClass.InternalId,
                    TenantId = eduClass.TenantId,
                    JoinCode = eduClass.JoinCode,
                    //TeamsUsers = users,
                    JoinUrl = eduClass.JoinUrl,
                    TeamType = eduClass.TeamType,
                    IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value,
                    AdId = eduClass.AdId,
                    AdCod = eduClass.AdCod,
                    AdDesc = eduClass.AdDesc,
                    AnnoOrdinamento = eduClass.AnnoOrd,
                    CdsCod = eduClass.CdsCod,
                    DepartmentId = eduClass.DepartmentId,
                    AnnoOfferta = eduClass.AnnoOfferta,
                    AdditionalDataString = eduClass.AdditionalDataString
                };

                await this.CreateAsync(team);
            }
            else
            {
                team.Description = eduClass.Description;
                team.ExternalId = eduClass.Key;
                team.Name = eduClass.Name;
                team.InternalId = eduClass.InternalId;
                team.TenantId = eduClass.TenantId;
                team.JoinCode = eduClass.JoinCode;
                team.JoinUrl = eduClass.JoinUrl;
                team.TeamType = eduClass.TeamType;
                team.IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value;
                team.AdId = eduClass.AdId;
                team.AdCod = eduClass.AdCod;
                team.AdDesc = eduClass.AdDesc;
                team.AnnoOrdinamento = eduClass.AnnoOrd;
                team.CdsCod = eduClass.CdsCod;
                team.DepartmentId = eduClass.DepartmentId;
                team.AnnoOfferta = eduClass.AnnoOfferta;
                team.AdditionalDataString = eduClass.AdditionalDataString;

                this.Update(team);
            }
            return team;
        }

        public async Task<Team> InsertOrUpdateWMembers(IEducationalClassTeam eduClass)
        {
            Member map(ITeamMember m) => new Member
            {
                UserPrincipalName = m.UserPrincipalName,
                MemberId = Guid.Parse(m.AzureAdId),
                TenantId = eduClass.TenantId,
                DisplayName = (string.IsNullOrWhiteSpace(m.Name) || string.IsNullOrWhiteSpace(m.SecondName))
                    ? m.UserPrincipalName
                    : $"{m.Name} {m.SecondName}",
                Mail = m.Mail,
            };

            //var userInGroup = eduClass.Owners.Concat(eduClass.Members);
            var userInGroup = eduClass.Owners.Concat(eduClass.Members.Where(m => !eduClass.Owners.Select(o => o.AzureAdId).Contains(m.AzureAdId)));
            var userInGroupId = userInGroup.Select(a => new Guid(a.AzureAdId));
            var userInDb = await _dbContext.Members.Where(a => userInGroupId.Contains(a.MemberId)).ToListAsync();
            var userInDbIds = userInDb.Select(b => b.MemberId.ToString());
            userInGroup.Where(a => !userInDbIds.Contains(a.AzureAdId))
                .ToList()
                .ForEach(a => _dbContext.Add(map(a)));

            var toUpdate = userInGroup.
                Where(a => userInDbIds.Contains(a.AzureAdId))
                .Select(a => userInDb.FirstOrDefault(u => u.MemberId.ToString() == a.AzureAdId));
            foreach (var a in toUpdate)
            {
                var ug = userInGroup.FirstOrDefault(u => a.MemberId.ToString() == u.AzureAdId);
                a.DisplayName = (string.IsNullOrWhiteSpace(ug.Name) || string.IsNullOrWhiteSpace(ug.SecondName))
                    ? ug.UserPrincipalName
                    : $"{ug.Name} {ug.SecondName}";
                a.Mail = ug.Mail;

                _dbContext.Update(a);
            }

            var team = await this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .Where(a => a.TeamsId == eduClass.Id)
                .FirstOrDefaultAsync();

            var users = eduClass.Owners
                .Select(a => new TeamMember() { MemberId = Guid.Parse(a.AzureAdId), MemberType = MemberType.Owner })
                .Concat(eduClass.Members
                .Select(a => new TeamMember() { MemberId = Guid.Parse(a.AzureAdId), MemberType = MemberType.Member }))
                .ToList();

            if (team == null)
            {
                team = new Team()
                {
                    CreatedOn = DateTime.UtcNow,
                    Description = eduClass.Description,
                    ExternalId = eduClass.Key,
                    Name = eduClass.Name,
                    TeamsId = eduClass.Id.Value,
                    InternalId = eduClass.InternalId,
                    TenantId = eduClass.TenantId,
                    JoinCode = eduClass.JoinCode,
                    TeamsUsers = users,
                    JoinUrl = eduClass.JoinUrl,
                    TeamType = eduClass.TeamType,
                    IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value,
                    AdId = eduClass.AdId,
                    AdCod = eduClass.AdCod,
                    AdDesc = eduClass.AdDesc,
                    AnnoOrdinamento = eduClass.AnnoOrd,
                    AnnoOfferta = eduClass.AnnoOfferta,
                    CdsCod = eduClass.CdsCod,
                    DepartmentId = eduClass.DepartmentId,
                    AdditionalDataString = eduClass.AdditionalDataString
                };

                await this.CreateAsync(team);
            }
            else
            {
                team.Description = eduClass.Description;
                team.ExternalId = eduClass.Key;
                team.Name = eduClass.Name;
                team.InternalId = eduClass.InternalId;
                team.TenantId = eduClass.TenantId;
                team.JoinCode = eduClass.JoinCode;
                team.JoinUrl = eduClass.JoinUrl;
                team.TeamType = eduClass.TeamType;
                team.IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value;
                team.AdId = eduClass.AdId;
                team.AdCod = eduClass.AdCod;
                team.AdDesc = eduClass.AdDesc;
                team.AnnoOrdinamento = eduClass.AnnoOrd;
                team.AnnoOfferta = eduClass.AnnoOfferta;
                team.CdsCod = eduClass.CdsCod;
                team.DepartmentId = eduClass.DepartmentId;
                team.AdditionalDataString = eduClass.AdditionalDataString;

                var newIds = users == null || !users.Any() ? Enumerable.Empty<TeamMember>() : users;
                var existingIds = team.TeamsUsers == null || !team.TeamsUsers.Any() ? Enumerable.Empty<TeamMember>() : team.TeamsUsers;

                //var removedOwners = existingIds.Except(newIds).ToList();
                //var addedOwners = newIds.Except(existingIds).ToList();

                var removedOwners = existingIds.Where(x => !newIds.Any(y => y.MemberId == x.MemberId)).ToList();
                var addedOwners = newIds.Where(x => !existingIds.Any(y => y.MemberId == x.MemberId)).ToList();

                foreach (var id in removedOwners)
                {
                    team.TeamsUsers.Remove(id);
                }

                foreach (var id in addedOwners)
                {
                    team.TeamsUsers.Add(id);
                }

            }
            return team;
        }
        public async Task<IEnumerable<Member>> GetTeamMember(string teamId)
        {
            if (!Guid.TryParse(teamId, out Guid teamGuid))
                return Enumerable.Empty<Member>();

            return await _defaultReadOnlyCollection<Team>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Member)
                .Where(a => a.TeamsId == teamGuid)
                .SelectMany(a => a.TeamsUsers)
                .Select(a => a.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTeams(string annoOfferta, string departmentId, CancellationToken cancellationToken = default)
        {
            var teams = await _defaultReadOnlyCollection<Team>()
                .Where(x => x.AnnoOfferta == annoOfferta && x.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
            return teams;
        }

        public async Task<string> GetTeamJoinUrl(string adId, string departmentId, CancellationToken cancellationToken = default)
        {
            string joinUrl = await _defaultReadOnlyCollection<Team>()
                .Where(x => x.AdId == adId && x.DepartmentId == departmentId)
                .Select(x => x.JoinUrl)
                .SingleOrDefaultAsync(cancellationToken);
            return joinUrl;
        }

        public async Task<Team> GetTeam(string adId, string departmentId, CancellationToken cancellationToken = default)
        {
            Team team = await _defaultReadOnlyCollection<Team>()
                .SingleOrDefaultAsync(x => x.AdId == adId && x.DepartmentId == departmentId, cancellationToken);
            return team;
        }

        public async Task<IEnumerable<Team>> GetTeamsWithMembers(string annoOfferta, CancellationToken cancellationToken = default)
        {
            var teams = await _defaultReadOnlyCollection<Team>()
                .Include(x => x.TeamsUsers)
                    .ThenInclude(tu => tu.Member)
                .Where(x => x.DeletedOn == null && x.AnnoOfferta == annoOfferta)
                .ToListAsync(cancellationToken);

            return teams;
        }

        public async Task<IEnumerable<Team>> GetTeamWithMembers(string adId, CancellationToken cancellationToken = default)
        {
            IEnumerable<Team> teams = await _defaultReadOnlyCollection<Team>()
                .Include(x => x.TeamsUsers)
                    .ThenInclude(tu => tu.Member)
                .Where(x => x.DeletedOn == null && x.AdId == adId)
                .ToListAsync(cancellationToken);
            return teams;
        }
    }
}
