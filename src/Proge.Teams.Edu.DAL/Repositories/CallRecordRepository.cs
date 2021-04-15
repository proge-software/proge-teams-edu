using Proge.Teams.Edu.DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.DAL.Repositories
{

    public interface ICallRecordRepository : IRepository
    {
        Task InsertChangeNotification(Entities.ChangeNotification changeNotification);
        Task<TeamsMeeting> GetTeamsMeetingByJoinUrl(string joinUrl);
        Task BulkWriteCallRecordAsync(List<CallRecord> list);
        Task BulkDeleteChangeNotificationAsync(List<Entities.ChangeNotification> list);
        void BulkDeleteChangeNotification(List<Entities.ChangeNotification> changeNotificationList);
    }

    public class CallRecordRepository : Repository<TeamsEduDbContext>, ICallRecordRepository
    {
        public CallRecordRepository(TeamsEduDbContext context) : base(context) { }

        public async Task InsertChangeNotification(Entities.ChangeNotification changeNotification)
        {
             await this.CreateAsync(changeNotification);
        }

        public async Task<TeamsMeeting> GetTeamsMeetingByJoinUrl(string joinUrl)
        {
            if (await this.GetExistsAsync<TeamsMeeting>(a => a.JoinUrl == joinUrl))
                return await this.GetByIdAsync<TeamsMeeting, string>(joinUrl);
            
            return null;
        }

        public async Task BulkWriteCallRecordAsync(List<CallRecord> callRecordList)
        {
            var callUserList = callRecordList.SelectMany(c => c.CallUsers).ToList();
            var callSessionList = callRecordList.SelectMany(c => c.CallSessions).ToList();
            var callSegmentList = callSessionList.SelectMany(c => c.CallSegments).ToList();

            using (var transaction = this._dbContext.Database.BeginTransaction())
            {
                await this.BulkInsertOrUpdateAsync(callRecordList);
                await this.BulkInsertOrUpdateAsync(callUserList);
                await this.BulkInsertOrUpdateAsync(callSessionList);
                await this.BulkInsertOrUpdateAsync(callSegmentList);
                transaction.Commit();
            }
        }

        public async Task BulkDeleteChangeNotificationAsync(List<Entities.ChangeNotification> changeNotificationList)
        {
            await this.BulkDeleteAsync(changeNotificationList);
        }

        public void BulkDeleteChangeNotification(List<Entities.ChangeNotification> changeNotificationList)
        {
            this.BulkDelete(changeNotificationList);
        }
    }
}