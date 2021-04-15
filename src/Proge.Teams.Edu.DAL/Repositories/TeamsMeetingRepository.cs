using System.Threading;
using Proge.Teams.Edu.DAL.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface ITeamsMeetingRepository : IRepository
    {
        Task<TeamsMeeting> GetByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task<bool> ExistByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task<int> Create(TeamsMeeting meeting, CancellationToken cancellationToken = default);
        Task<int> UpdateMeeting(TeamsMeeting meeting, CancellationToken cancellationToken = default);
    }

    public class TeamsMeetingRepository : Repository<TeamsEduDbContext>, ITeamsMeetingRepository
    {
        public TeamsMeetingRepository(TeamsEduDbContext context) : base(context) { }

        public async Task<int> Create(TeamsMeeting meeting, CancellationToken cancellationToken = default)
        {
            await _dbContext.AddAsync(meeting, cancellationToken);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistByJoinUrl(string joinUrl, CancellationToken cancellationToken = default)
        {
            bool exist = await _dbContext.TeamsMeeting
                .AsNoTracking()
                .AnyAsync(t => t.JoinUrl == joinUrl, cancellationToken);
            return exist;
        }

        public async Task<TeamsMeeting> GetByJoinUrl(string joinUrl, CancellationToken cancellationToken = default)
        {
            var meeting = await _dbContext.TeamsMeeting
                .SingleOrDefaultAsync(x => x.JoinUrl == joinUrl, cancellationToken);
            return meeting;
        }

        public async Task<int> UpdateMeeting(TeamsMeeting meeting, CancellationToken cancellationToken = default)
        {
            var stMeeting = await _dbContext.TeamsMeeting
                .SingleOrDefaultAsync(x => x.MeetingId == meeting.MeetingId, cancellationToken);
            if (meeting == null)
                return 0;

            stMeeting.MeetingName = meeting.MeetingName;
            stMeeting.Attendees = meeting.Attendees;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
