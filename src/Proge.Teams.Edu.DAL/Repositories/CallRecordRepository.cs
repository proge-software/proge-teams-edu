using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.DAL.Entities;

namespace Proge.Teams.Edu.DAL.Repositories
{

    public interface ICallRecordRepository : IRepository
    {
        Task CreateOrUpdate(CallRecord callRecord, ChangeNotification changeNotification, CancellationToken cancellationToken = default);
        Task<TeamsMeeting> GetTeamsMeetingByChatThreadId(string chatThreadId, CancellationToken cancellationToken = default);
        Task InsertChangeNotification(ChangeNotification changeNotification, CancellationToken cancellationToken = default);
        Task<TeamsMeeting> GetTeamsMeetingByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task<string> GetTeamsMeetingNameByJoinUrl(string joinUrl, CancellationToken cancellationToken = default);
        Task BulkWriteCallRecordsAsync(List<CallRecord> list, CancellationToken cancellationToken = default);
        Task BulkDeleteChangeNotificationAsync(List<ChangeNotification> list, CancellationToken cancellationToken = default);
        void BulkDeleteChangeNotification(List<ChangeNotification> changeNotificationList);
    }

    public class CallRecordRepository : Repository<TeamsEduDbContext>, ICallRecordRepository
    {
        protected readonly ILogger<CallRecordRepository> _logger;
        public CallRecordRepository(TeamsEduDbContext context,
            ILogger<CallRecordRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task InsertChangeNotification(ChangeNotification changeNotification, CancellationToken cancellationToken = default)
        {
            await CreateAsync(changeNotification, cancellationToken);
        }

        public async Task<TeamsMeeting> GetTeamsMeetingByJoinUrl(string joinUrl, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TeamsMeeting.AsNoTracking()
                .FirstOrDefaultAsync(a => a.JoinUrl == joinUrl, cancellationToken);
        }

        public async Task<string> GetTeamsMeetingNameByJoinUrl(string joinUrl, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TeamsMeeting.AsNoTracking()
                .Where(a => a.JoinUrl == joinUrl)
                .Select(a => a.MeetingName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task BulkWriteCallRecordsAsync(List<CallRecord> callRecordList, CancellationToken cancellationToken = default)
        {
            var callUserList = callRecordList.SelectMany(c => c.CallUsers).ToList();
            var callSessionList = callRecordList.SelectMany(c => c.CallSessions).ToList();
            var callSegmentList = callSessionList.SelectMany(c => c.CallSegments).ToList();

            using var transaction = _dbContext.Database.BeginTransaction();
            await BulkInsertOrUpdateAsync(callRecordList, cancellationToken);
            await BulkInsertOrUpdateAsync(callUserList, cancellationToken);
            await BulkInsertOrUpdateAsync(callSessionList, cancellationToken);
            await BulkInsertOrUpdateAsync(callSegmentList, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task<string> CreateSavepointAsync(IDbContextTransaction transaction, string createSavepoint,
            string releaseSavepoint = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await transaction.CreateSavepointAsync(createSavepoint, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error creating transaction savepoint {create}, won't release {release}", createSavepoint, releaseSavepoint);
                return releaseSavepoint;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(releaseSavepoint))
                    await transaction.ReleaseSavepointAsync(releaseSavepoint, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error releasing transaction savepoint {savepoint}", releaseSavepoint);
            }

            return createSavepoint;
        }

        public async Task<TeamsMeeting> GetTeamsMeetingByChatThreadId(string chatThreadId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TeamsMeeting
                .SingleOrDefaultAsync(t => t.ChatThreadId == chatThreadId, cancellationToken);
        }

        public async Task BulkDeleteChangeNotificationAsync(List<ChangeNotification> changeNotificationList, CancellationToken cancellationToken = default)
        {
            await BulkDeleteAsync(changeNotificationList, cancellationToken);
        }

        public void BulkDeleteChangeNotification(List<ChangeNotification> changeNotificationList)
        {
            BulkDelete(changeNotificationList);
        }

        public async Task CreateOrUpdate(CallRecord callRecord, ChangeNotification changeNotification, CancellationToken cancellationToken = default)
        {
            try
            {
                await BulkWriteCallRecordAsync(callRecord, cancellationToken);
                await InsertChangeNotification(changeNotification, cancellationToken);
                await SaveAsync(cancellationToken);
                _logger.LogInformation("Call record {0} persisted successfully", callRecord.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "DbUpdateConcurrencyException on Call record with Id {0}! Data may be not updated!", callRecord.Id);
                throw;
            }
            catch (SqlException sex)
            {
                _logger.LogError(sex, "SqlException on Call record with Id {0}! Data not saved on DB!", callRecord.Id);
                throw;
            }
            catch (DbUpdateException dex)
            {
                _logger.LogError(dex, "DbUpdateException on Call record with Id {0}! Data not saved on DB!", callRecord.Id);
                throw;
            }
            catch (Exception dex)
            {
                _logger.LogError(dex, "Exception on Call record with Id {0}! Data not saved on DB!", callRecord.Id);
                throw;
            }
        }

        private async Task BulkWriteCallRecordAsync(CallRecord callRecord, CancellationToken cancellationToken)
        {
            using IDbContextTransaction transaction = _dbContext.Database.BeginTransaction();

            try
            {
                await BulkWriteCallRecordAsync(transaction, callRecord, cancellationToken);
            }
            catch (TransactionException te)
            {
                if (!te.HasSavepoint)
                {
                    _logger.LogError(te, "Error inserting/updating call record, no savepoint to rollback to found");
                    return;
                }

                await TryRollback(transaction, te, cancellationToken);
            }
        }

        private async Task<bool> TryRollback(IDbContextTransaction transaction, TransactionException te, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Rolling back to savepoint {savepoint}", te.Savepoint);
                await transaction.RollbackToSavepointAsync(te.Savepoint, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(new AggregateException(te, e),
                    "Error rolling back to savepoint '{savepoint}'", te.Savepoint);
                return false;
            }
        }

        private async Task BulkWriteCallRecordAsync(IDbContextTransaction transaction, CallRecord callRecord, CancellationToken cancellationToken)
        {
            // update call record
            List<CallRecord> callRecords = new() { callRecord };
            string savepoint = await BulkInsertAsync(transaction, callRecords, "call-record", null, cancellationToken);

            // update call sessions
            var callSessions = callRecord.CallSessions.ToList();
            savepoint = await BulkInsertAsync(transaction, callSessions, "call-sessions", savepoint, cancellationToken);

            // update call segments
            var callSegments = callRecord.CallSessions.SelectMany(c => c.CallSegments).ToList();
            savepoint = await BulkInsertAsync(transaction, callSegments, "call-segments", savepoint, cancellationToken);

            // update call users, disctincy by key table properties
            var callUsers = callRecord.CallUsers
                .GroupBy(cu => new { cu.Id, cu.UserRole, cu.CallRecord })
                .Select(gcu => gcu.FirstOrDefault())
                .Where(cu => cu != null)
                .ToList();

            savepoint = await BulkInsertAsync(transaction, callUsers, "call-users", savepoint, cancellationToken);

            // release savepoint
            try
            {
                if (!string.IsNullOrWhiteSpace(savepoint))
                    await transaction.ReleaseSavepointAsync(savepoint, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error releasing transaction savepoint {savepoint}", savepoint);
            }

            // committing transaction
            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                throw new TransactionException("Error committing transaction", e);
            }
        }

        private async Task<string> BulkInsertAsync<T>(
            IDbContextTransaction transaction,
            IList<T> tt,
            string entity,
            string releaseSavepoint,
            CancellationToken cancellationToken) where T : class
        {
            try
            {
                await BulkInsertOrUpdateAsync(tt, cancellationToken);
            }
            catch (Exception e)
            {
                throw new TransactionException($"Error updating {entity}", e, releaseSavepoint);
            }

            string savepoint = await CreateSavepointAsync(transaction,
                createSavepoint: $"{entity}",
                releaseSavepoint: releaseSavepoint,
                cancellationToken: cancellationToken);
            return savepoint;
        }

        class TransactionException : Exception
        {
            internal string Savepoint { get; private set; }
            internal bool HasSavepoint => !string.IsNullOrWhiteSpace(Savepoint);

            internal TransactionException(string message, Exception e, string savepoint = null) : base(message, e)
            {
                Savepoint = savepoint;
            }
        }
    }
}