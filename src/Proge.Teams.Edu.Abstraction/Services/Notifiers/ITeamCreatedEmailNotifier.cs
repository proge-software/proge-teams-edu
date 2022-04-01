using System.Threading;
using System.Threading.Tasks;
using Proge.Teams.Edu.Abstraction.Events;

namespace Proge.Teams.Edu.Abstraction.Services.Notifiers
{
    public interface ITeamCreatedEmailNotifier
    {
        Task NotifyTeamCreatedAsync(TeamCreatedEvent @event, CancellationToken cancellationToken = default);
    }
}