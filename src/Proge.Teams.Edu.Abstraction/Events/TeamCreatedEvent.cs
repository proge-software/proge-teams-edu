using System;

namespace Proge.Teams.Edu.Abstraction.Events
{
    public class TeamCreatedEvent
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public IEducationalClassTeam Team { get; set; }
    }
}