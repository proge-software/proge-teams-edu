using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction.Services
{
    public interface IFunctionSerializer
    {
        Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default);
        Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    }

    public class FunctionSerializer : IFunctionSerializer
    {
        private readonly static SemaphoreSlim _dbSem = new SemaphoreSlim(1, 1);
        public async Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            await _dbSem.WaitAsync(cancellationToken);
            try
            {
                await action();
            }
            finally
            {
                _dbSem.Release();
            }
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            await _dbSem.WaitAsync(cancellationToken);
            try
            {
                return await action();
            }
            finally
            {
                _dbSem.Release();
            }
        }
    }
}
