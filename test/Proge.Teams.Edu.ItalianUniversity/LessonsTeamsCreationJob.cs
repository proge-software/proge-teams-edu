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

        public Task<IEnumerable<IEducationalClassTeam>> Read()
        {
            return Task.FromException<IEnumerable<IEducationalClassTeam>>(new NotImplementedException("Read() method not implemented yet"));            
        }

        public Task Write(IEnumerable<IEducationalClassTeam> insegnamenti)
        {
            _logger.LogInformation("LessonsTeamsCreationJob.Write() started at {dateTime}", DateTime.Now);
            return Task.FromException<IEnumerable<IEducationalClassTeam>>(new NotImplementedException("Write() method not implemented yet"));            
        }      
    }
}
