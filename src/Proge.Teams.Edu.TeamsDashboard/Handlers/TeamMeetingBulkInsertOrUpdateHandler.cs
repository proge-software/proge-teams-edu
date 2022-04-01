extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.DAL.Entities;
using Proge.Teams.Edu.DAL.Repositories;

namespace Proge.Teams.Edu.TeamsDashboard
{
    public interface ITeamMeetingBulkInsertOrUpdateHandler
    {
        Task CreateOrUpdateTeamsMeeting(IEnumerable<TeamsMeeting> receivedMeetings, CancellationToken cancellationToken = default);
    }

    public class TeamMeetingBulkInsertOrUpdateHandler : ITeamMeetingBulkInsertOrUpdateHandler
    {
        protected readonly ILogger<TeamMeetingBulkInsertOrUpdateHandler> _logger;
        protected readonly ITeamsMeetingRepository _teamsMeetingRepository;

        public TeamMeetingBulkInsertOrUpdateHandler(
            ITeamsMeetingRepository teamsMeetingRepository,
            ILogger<TeamMeetingBulkInsertOrUpdateHandler> logger)
        {
            _logger = logger;
            _teamsMeetingRepository = teamsMeetingRepository;
        }

        public virtual async Task CreateOrUpdateTeamsMeeting(IEnumerable<TeamsMeeting> meetings, CancellationToken cancellationToken = default)
        {
            IEnumerable<TeamsMeeting> cleanedMeetings = ValidateAndCleanMeetings(meetings);
            await _teamsMeetingRepository.BulkCreateOrUpdateMeetings(cleanedMeetings, cancellationToken);
        }

        private static IEnumerable<TeamsMeeting> ValidateAndCleanMeetings(IEnumerable<TeamsMeeting> teamsMeetings)
        {
            List<TeamsMeeting> validatedMeetings = new();

            int counter = 0;
            foreach (var meeting in teamsMeetings)
            {
                try
                {
                    ValidateTeamMeeting(meeting);

                    meeting.MeetingId = CleanMeetingId(meeting.MeetingId);
                    validatedMeetings.Add(meeting);

                    counter++;
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Error processing team n. {counter}", e);
                }
            }
            return validatedMeetings;
        }

        private static void ValidateTeamMeeting(TeamsMeeting meeting)
        {
            if (string.IsNullOrWhiteSpace(meeting.JoinUrl))
                throw new ArgumentException("Missing JoinUrl");

            if (string.IsNullOrWhiteSpace(meeting.MeetingName))
                throw new ArgumentException("Missing MeetingName");
        }

        private static string CleanMeetingId(string meetingId)
        {
            string value = meetingId;

            string a = "meeting_";
            if (value.StartsWith(a))
                value = value[a.Length..];

            string b = "@thread";
            int pb = value.LastIndexOf(b);
            if (pb != -1)
                value = value[..pb];

            return value;
        }
    }
}