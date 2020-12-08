using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Race.Util
{
    public static class ConcurrentQueueExtensions
    {
        public static async Task<T> DequeueAsync<T>(this ConcurrentQueue<T> queue, CancellationToken cancellationToken = default, TimeSpan timeout = default)
        {
            T result = default;
            DateTime requestTime = default == timeout ? default : DateTime.Now;
            while (!queue.TryDequeue(out result)
                && (default == cancellationToken || !cancellationToken.IsCancellationRequested)
                && (default == timeout || DateTime.Now > requestTime + timeout))
            {
                if (null != cancellationToken)
                {
                    await Task.Run(() => cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100)));
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
            return result;
        }
    }
}
