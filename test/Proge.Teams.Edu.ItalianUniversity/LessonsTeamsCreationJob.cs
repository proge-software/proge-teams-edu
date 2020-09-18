using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.ItalianUniversity
{
    public class LessonsTeamsCreationJob : IJob
    {
        private readonly ILogger<LessonsTeamsCreationJob> _logger;

        public LessonsTeamsCreationJob(ILogger<LessonsTeamsCreationJob> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<IEducationalClassTeam>> Read()
        {
            throw new NotImplementedException();
        }

        public async Task Write(IEnumerable<IEducationalClassTeam> insegnamenti)
        {
            _logger.LogInformation("LessonsTeamsCreationJob.Write() started at {dateTime}", DateTime.Now);

            throw new NotImplementedException();

            _logger.LogInformation("LessonsTeamsCreationJob.Write() ended at {dateTime}", DateTime.Now);
        }

        public Task Validate(IEnumerable<IEducationalClassTeam> insegnamenti)
        {
            throw new NotImplementedException();
        }

        public Task GetFeedback()
        {
            throw new NotImplementedException();
        }
    }
}
