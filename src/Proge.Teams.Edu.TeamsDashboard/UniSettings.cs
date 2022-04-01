namespace Proge.Teams.Edu.TeamsDashboard
{
    public class UniSettings
    {
        public string ClientStateSecret { get; set; }
        public string NotificationUrl { get; set; }
        public string ChangeType { get; set; }
        public string Resource { get; set; }
        public string SenderKey { get; set; }
        public string AppClientId { get; set; }
        public string AppAudience { get; set; }
        public string AppTenant { get; set; }
        public string StorageEventContainer { get; set; }
        public string EventHubName { get; set; }
        public bool SaveOnlineMeetingDetails { get; set; } = false;
    }

    public class CallFilters
    {
        public string CallType { get; set; }
        public int? MinDuration { get; set; }
        public bool NameNeededInTeamsMeetingTable { get; set; }
    }
}
