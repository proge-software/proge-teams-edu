using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IJob : IJobReader, IJobWriter { }

    public interface IJobReader
    {
        Task<IEnumerable<IEducationalClassTeam>> Read();
    }

    public interface IJobWriter
    {
        Task Write(IEnumerable<IEducationalClassTeam> insegnamenti);
    }
}
