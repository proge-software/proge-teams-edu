using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.ItalianUniversity
{
    public class LessonsTeamsCreationFacade : IFacade
    {
        private readonly IJob _job;

        public LessonsTeamsCreationFacade(IJob lessonTeamsCreationJob)
        {
            _job = lessonTeamsCreationJob;
        }

        public Task Configure()
        {
            throw new NotImplementedException();
        }

        public async Task StartJob()
        {
            var readParameters = await _job.Read();

            //await _job.Validate();
            await _job.Write(readParameters);
            //await _job.GetFeedback();
        }
    }
}
