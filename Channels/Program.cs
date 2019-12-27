using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels
// https://deniskyashif.com/csharp-channels-part-1/
// https://deniskyashif.com/csharp-channels-part-2/

namespace Channels
{
    public sealed class MyChannel<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        // Produces data into channel. Since our channel is unbounded, producing
        // data into it will always complete successfully and synchronously.
        public void Write(T value)
        {
            _queue.Enqueue(value); // store the data
            _semaphore.Release(); // notify any consumers that more data is available
        }

        // Consume data from channel. ReadAsync is asynchronous because the data
        // we want to consume may not be available yet, and thus we’ll need to
        // wait for it to arrive if nothing is available to consume at the time
        // we try. our ReadAsync method returns a ValueTask<T> rather than a
        // Task<T>, so that we can make it allocation-free when it completes
        // synchronously.
        public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false); // wait
            bool gotOne = _queue.TryDequeue(out T item); // retrieve the data
            Debug.Assert(gotOne);
            return item;   
        }
    }

    class Program
    {
        static async Task MyImplementation()
        {
            // Custom implementation
            var c = new MyChannel<int>();
            c.Write(42);
            var i = await c.ReadAsync();
            Console.WriteLine(i);
        }

        static async Task BasicOutOfTheBox()
        {
            var ch = Channel.CreateUnbounded<string>();

            var consumer = Task.Run(async () =>
            {
                while (await ch.Reader.WaitToReadAsync())
                    Console.WriteLine(await ch.Reader.ReadAsync());
            });

            var producer = Task.Run(async () =>
            {
                var rnd = new Random();
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                    await ch.Writer.WriteAsync($"Message {i}");
                }
                ch.Writer.Complete();
            });

            await Task.WhenAll(producer, consumer);
        }

        static ChannelReader<string> CreateMessenger(string msg, int count)
        {
            var ch = Channel.CreateUnbounded<string>();
            var rnd = new Random();

            Task.Run(async () =>
            {
                for (int i = 0; i < count; i++)
                {
                    await ch.Writer.WriteAsync($"{msg} {i}");
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                }
                ch.Writer.Complete();
            });

            return ch.Reader;
        }

        static async Task Test()
        {
            var joe = CreateMessenger("Joe", 5);
            await foreach (var item in joe.ReadAllAsync())
                Console.WriteLine(item);
        }

        static async Task Main(string[] args)
        {
            await MyImplementation();
            await BasicOutOfTheBox();
            await Test();
        }
    }
}