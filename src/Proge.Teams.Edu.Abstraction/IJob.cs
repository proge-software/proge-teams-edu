
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IJob
    {
        Task<IEnumerable<IEducationalClassTeam>> Read();       
        Task Write(IEnumerable<IEducationalClassTeam> insegnamenti);       
    }
}
