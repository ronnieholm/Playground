using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    // https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels
    public class MyChannel<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        // Produces data into channel. Since our channel is unbounded, producing
        // data into it will always complete successfully and synchronously.
        public void Write(T value)
        {
            // Store the data
            _queue.Enqueue(value);
            // Notify any consumers that more data is available
            _semaphore.Release();
        }

        // Consume data from channel. ReadAsync is asynchronous because the data
        // we want to consume may not be available yet, and thus we’ll need to
        // wait for it to arrive if nothing is available to consume at the time
        // we try. our ReadAsync method returns a ValueTask<T> rather than a
        // Task<T>, so that we can make it allocation-free when it completes
        // synchronously.
        public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            // Wait
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            // Retrieve the data
            bool gotOne = _queue.TryDequeue(out T item);
            Debug.Assert(gotOne);
            return item;   
        }        
    }

    public class MyChannelRunner 
    {
        public static async Task Run()
        {
            var c = new MyChannel<int>();
            c.Write(42);
            var i = await c.ReadAsync();
            Console.WriteLine(i);
        }
    }
}