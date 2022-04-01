using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.ItalianUniversity.ConsoleApp
{
    internal class ConsoleApplication : IConsoleApplication
    {
        private readonly IFacade _facadeLesson;
        private readonly ILogger _logger;

        public ConsoleApplication(IFacade lessonTeamsCreationFacade, ILogger<ConsoleApplication> logger)
        {
            _facadeLesson = lessonTeamsCreationFacade;
            _logger = logger;
        }

        public async Task Run(string[] args, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("ItalianUniversity application started at {dateTime}", DateTime.Now);

            await _facadeLesson.StartJob();

            _logger.LogInformation("ItalianUniversity application ended at {dateTime}", DateTime.Now);
        }

    }
}
