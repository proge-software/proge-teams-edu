using System.Threading.Channels;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IJobChannel : IJobChannelReader, IJobChannelWriter { }

    public interface IJobChannelReader
    {
        Task Read(ChannelWriter<IEducationalClassTeam> chan);
    }

    public interface IJobChannelWriter
    {
        Task Write(ChannelReader<IEducationalClassTeam> insegnamenti);
    }
}
