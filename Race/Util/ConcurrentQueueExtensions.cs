using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Race.Util
{
    public static class ConcurrentQueueExtensions
    {
        public static async Task<T> DequeueAsync<T>(this ConcurrentQueue<T> queue, CancellationToken cancellationToken = default)
        {
            T result = default;
            while (!queue.TryDequeue(out result) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Run(() => cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100)));
            }
            return result;
        }
    }
}
