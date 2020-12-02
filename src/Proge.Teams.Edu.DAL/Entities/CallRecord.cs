extern alias BetaLib;
using System;
using System.Collections.Generic;
using Beta = BetaLib.Microsoft.Graph;

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
        public ICollection<CallSession> CallSessions { get; set; }
    }
}
