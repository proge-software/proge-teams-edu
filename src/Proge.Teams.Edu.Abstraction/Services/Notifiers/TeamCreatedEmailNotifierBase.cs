using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Proge.Teams.Edu.Abstraction.Events;
using Proge.Teams.Edu.Abstraction.Helpers;

namespace Proge.Teams.Edu.Abstraction.Services.Notifiers
{
    public abstract class TeamCreatedEmailNotifierBase : ITeamCreatedEmailNotifier
    {
        protected abstract Task<string> ReadBodyTmpl();
        protected abstract Task<string> ReadSubjectTmpl();

        private Lazy<Task<string>> BodyTmpl;
        private Lazy<Task<string>> SubjectTmpl;

        public TeamCreatedEmailNotifierBase()
        {
            BodyTmpl = new Lazy<Task<string>>(async () => await ReadBodyTmpl());
            SubjectTmpl = new Lazy<Task<string>>(async () => await ReadSubjectTmpl());
        }

        public async Task NotifyTeamCreatedAsync(TeamCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            IEnumerable<ITeamMember> recipients = Recipients(@event);
            IEnumerable<Task> sendMailTasks = recipients.Select(r => SendMail(r, @event, cancellationToken));
            await Task.WhenAll(sendMailTasks);
        }

        protected abstract IEnumerable<ITeamMember> Recipients(TeamCreatedEvent @event);
        protected abstract Task SendMail(ITeamMember member, TeamCreatedEvent @event, CancellationToken cancellationToken = default);

        protected async Task<string> BuildContent<C>(ITeamMember member, C data, CancellationToken cancellationToken = default) where C : class
        {
            var bodyTmpl = await (BodyTmpl.Value);
            string body = TemplatesHelper.Execute(bodyTmpl, data);
            return body;
        }

        protected async Task<string> BuildSubject<C>(ITeamMember member, C data, CancellationToken cancellationToken = default) where C : class
        {
            var subjectTmpl = await (SubjectTmpl.Value);
            string subject = TemplatesHelper.Execute(subjectTmpl, data);
            return subject;
        }
    }
}