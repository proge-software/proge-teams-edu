using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.GraphApi
{
    public static class Retry
    {
        public static async Task<T> Do<T>(
          Func<Task<T>> action,
          TimeSpan retryInterval,
          int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();
            int retryCount = 1;
            for (int attempted = 1; attempted <= maxAttemptCount; attempted++)
            {
                retryCount = attempted;
                try
                {
                    if (attempted > 1)
                        await Task.Delay(retryInterval);
                    else
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                    return await action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (retryCount >= maxAttemptCount)
                throw new AggregateException(exceptions);
            else
                return default;
        }

        public static async Task Do<T>(
         Func<Task> action,
         TimeSpan retryInterval,
         int maxAttemptCount = 5)
        {
            var exceptions = new List<Exception>();
            int retryCount = 1;
            for (int attempted = 1; attempted <= maxAttemptCount; attempted++)
            {
                retryCount = attempted;
                try
                {
                    if (attempted > 1)                    
                        await Task.Delay(retryInterval);                    
                    else
                        await Task.Delay(TimeSpan.FromMilliseconds(300));
                    await action();
                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (retryCount >= maxAttemptCount)
                throw new AggregateException(exceptions);           
        }
    }
}
