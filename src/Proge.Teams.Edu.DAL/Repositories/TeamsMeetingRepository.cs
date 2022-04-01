using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proge.Teams.Edu.DAL.Entities;

namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface ITeamsMeetingRepository : IRepository
    {
        Task<TeamsMeeting> GetByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task<bool> ExistByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task<int> Create(TeamsMeeting meeting, CancellationToken cancellationToken = default);
        Task<int> UpdateMeeting(TeamsMeeting meeting, CancellationToken cancellationToken = default);
        Task BulkCreateOrUpdateMeetings(IEnumerable<TeamsMeeting> meeting, CancellationToken cancellationToken = default);
        Task<bool> CallRecordExist(string oDataId, CancellationToken cancellationToken = default);
    }

    public class TeamsMeetingRepository : Repository<TeamsEduDbContext>, ITeamsMeetingRepository
    {
        public TeamsMeetingRepository(TeamsEduDbContext context) : base(context) { }

        public async Task<bool> CallRecordExist(string oDataId, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(oDataId, out Guid result))
                return false;

            return await _dbContext.CallRecord
                .AsNoTracking()
                .AnyAsync(a => a.Id == result, cancellationToken);
        }

        public async Task<int> Create(TeamsMeeting meeting, CancellationToken cancellationToken = default)
        {
            meeting.CreationDateTime = DateTime.Now;
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

        public async Task<bool> ExistByMeetingId(string meetingId, CancellationToken cancellationToken = default)
        {
            bool exist = await _dbContext.TeamsMeeting
                .AsNoTracking()
                .AnyAsync(t => t.MeetingId == meetingId, cancellationToken);
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
            if (!await UpdateMeetingNoSave(meeting, cancellationToken))
                return 0;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> UpdateMeetingNoSave(TeamsMeeting meeting, CancellationToken cancellationToken = default)
        {
            var stMeeting = await _dbContext.TeamsMeeting
                .SingleOrDefaultAsync(x => x.MeetingId == meeting.MeetingId, cancellationToken);
            if (meeting == null)
                return false;

            stMeeting.MeetingName = meeting.MeetingName;
            stMeeting.Attendees = meeting.Attendees;
            stMeeting.MeetingExtendedAttribute = meeting.MeetingExtendedAttribute;
            stMeeting.MeetingExtendedName = meeting.MeetingExtendedName;
            stMeeting.MeetingIdPrimary = meeting.MeetingIdPrimary;
            stMeeting.MeetingIdSecondary = meeting.MeetingIdSecondary;
            stMeeting.OwnerExtended = meeting.OwnerExtended;
            stMeeting.OwnerUpn = meeting.OwnerUpn;
            stMeeting.CustomAttribute = meeting.CustomAttribute;
            stMeeting.MeetingHierarchy = meeting.MeetingHierarchy;
            stMeeting.MeetingType = meeting.MeetingType;
            
            return true;
        }

        public async Task BulkCreateOrUpdateMeetings(IEnumerable<TeamsMeeting> meetings, CancellationToken cancellationToken = default)
        {
            foreach (var meeting in meetings)
            {
                if (await ExistByMeetingId(meeting.MeetingId, cancellationToken))
                {
                    await UpdateMeetingNoSave(meeting, cancellationToken);
                    continue;
                }

                meeting.CreationDateTime = DateTime.Now;
                await _dbContext.AddAsync(meeting, cancellationToken);
            }

            await SaveAsync(cancellationToken);
        }
    }
}
