using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IJobChannel : IJobChannelReader, IJobChannelWriter { }

    public interface IJobChannelReader<T>
    {
        Task Read(ChannelWriter<T> chan, CancellationToken cancellationToken = default);
    }

    public interface IJobChannelReader : IJobChannelReader<IEducationalClassTeam>
    {
    }

    public interface IJobChannelWriter<T>
    {
        Task Write(ChannelReader<T> insegnamenti, CancellationToken cancellationToken = default);
    }

    public interface IJobChannelWriter : IJobChannelWriter<IEducationalClassTeam>
    {
    }
}
