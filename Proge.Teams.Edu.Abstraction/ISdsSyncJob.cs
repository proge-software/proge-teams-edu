using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.Abstraction
{
    public interface ISdsSyncJob
    {
        Task<ISdsSyncParameters> Read();
        Task Validate(ISdsSyncParameters syncParameters);
        Task<string> Write(ISdsSyncParameters syncParameters);
        Task GetFeedback();
    }
}
