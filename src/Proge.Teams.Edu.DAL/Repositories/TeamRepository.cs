using Microsoft.EntityFrameworkCore;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface ITeamRepository : IRepository
    {
        Task<IEnumerable<Team>> GetTeams();
        Task<Team> GetTeamById(string teamId);
        Task<Team> GetTeamByExternalId(string teamId, bool excludeDeleted = true);
        Task<Team> InsertOrUpdate(IEducationalClassTeam eduClass);
        Task<Team> InsertOrUpdateWMembers(IEducationalClassTeam eduClass);
        Task<IEnumerable<Member>> GetTeamMember(string teamId);
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

        public async Task<Team> InsertOrUpdate(IEducationalClassTeam eduClass)
        {
            //var userInGroup = eduClass.Owners.Concat(eduClass.Members);
            //var userInGroupId = userInGroup.Select(a => new Guid(a.AzureAdId));
            //var userInDb = await _dbContext.Members.Where(a => userInGroupId.Contains(a.MemberId)).ToListAsync();
            //userInGroup.Where(a => !userInDb.Select(b => b.MemberId).Contains(Guid.Parse(a.AzureAdId)))
            //    .ToList()
            //    .ForEach(a => _dbContext.Add(new Member() { UserPrincipalName = a.UserPrincipalName, MemberId = Guid.Parse(a.AzureAdId), TenantId = eduClass.TenantId }));

            var team = await this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .Where(a => a.TeamsId == eduClass.Id)
                .FirstOrDefaultAsync();

            //var users = eduClass.Owners
            //    .Select(a => new TeamMember() { MemberId = Guid.Parse(a.AzureAdId), MemberType = MemberType.Owner })
            //    .Concat(eduClass.Members
            //    .Select(a => new TeamMember() { MemberId = Guid.Parse(a.AzureAdId), MemberType = MemberType.Member }))
            //    .ToList();

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
                    IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value
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

                //var newIds = users == null || !users.Any() ? Enumerable.Empty<TeamMember>() : users;
                //var existingIds = team.TeamsUsers == null || !team.TeamsUsers.Any() ? Enumerable.Empty<TeamMember>() : team.TeamsUsers;

                ////var removedOwners = existingIds.Except(newIds).ToList();
                ////var addedOwners = newIds.Except(existingIds).ToList();

                //var removedOwners = existingIds.Where(x => !newIds.Any(y => y.MemberId == x.MemberId)).ToList();
                //var addedOwners = newIds.Where(x => !existingIds.Any(y => y.MemberId == x.MemberId)).ToList();

                //foreach (var id in removedOwners)
                //{
                //    team.TeamsUsers.Remove(id);
                //}

                //foreach (var id in addedOwners)
                //{
                //    team.TeamsUsers.Add(id);
                //}

            }
            return team;
        }

        public async Task<Team> InsertOrUpdateWMembers(IEducationalClassTeam eduClass)
        {
            //var userInGroup = eduClass.Owners.Concat(eduClass.Members);
            var userInGroup = eduClass.Owners.Concat(eduClass.Members.Where(m => !eduClass.Owners.Select(o => o.AzureAdId).Contains(m.AzureAdId)));
            var userInGroupId = userInGroup.Select(a => new Guid(a.AzureAdId));
            var userInDb = await _dbContext.Members.Where(a => userInGroupId.Contains(a.MemberId)).ToListAsync();
            userInGroup.Where(a => !userInDb.Select(b => b.MemberId).Contains(Guid.Parse(a.AzureAdId)))
                .ToList()
                .ForEach(a => _dbContext.Add(new Member() { UserPrincipalName = a.UserPrincipalName, MemberId = Guid.Parse(a.AzureAdId), TenantId = eduClass.TenantId }));

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
                    IsMembershipLimitedToOwners = eduClass.IsMembershipLimitedToOwners.Value
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
    }
}
