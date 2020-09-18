using Microsoft.EntityFrameworkCore;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface IMemberRepository
    {

    }

    public class MemberRepository : Repository<TeamsEduDbContext>, IMemberRepository
    {

        public MemberRepository(TeamsEduDbContext context) : base(context) { }

        public async Task<IEnumerable<Member>> GetMembers()
        {
            return await this._defaultCollection<Member>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Team)
                .ToListAsync();
        }

        public async Task<Team> GetMemeber(string teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId) || !Guid.TryParse(teamId, out Guid teamGuid))
                return null;

            return await this._defaultCollection<Team>()
                .Include(a => a.TeamsUsers)
                .ThenInclude(a => a.Team)
                .Where(a => a.TeamsId == teamGuid)
                .SingleOrDefaultAsync();
        }

    }
}
