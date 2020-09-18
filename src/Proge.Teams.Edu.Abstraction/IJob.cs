
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IJob
    {
        Task<IEnumerable<IEducationalClassTeam>> Read();
        Task Validate(IEnumerable<IEducationalClassTeam> insegnamenti);
        Task Write(IEnumerable<IEducationalClassTeam> insegnamenti);
        Task GetFeedback();
    }
}
