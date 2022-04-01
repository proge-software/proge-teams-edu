extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class CallRecord
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string JoinWebUrl { get; set; }
        public string CallDescription { get; set; }
        public DateTimeOffset? StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public string Modalities { get; set; }

        public ICollection<CallUser> CallUsers { get; set; }

        /// Sometimes we receive multiple callSession with same Id
        /// so we filter duplicates callSessions with a simple Distinct
        private ICollection<CallSession> _callSessions;
        public ICollection<CallSession> CallSessions
        {
            get => _callSessions?.Distinct(new CallSessionComparer()).ToArray();
            set => _callSessions = value;
        }
    }

    class CallSessionComparer : IEqualityComparer<CallSession>
    {
        public bool Equals(CallSession x, CallSession y) => x?.Id == y?.Id;
        public int GetHashCode(CallSession obj) => obj.Id.GetHashCode();
    }
}