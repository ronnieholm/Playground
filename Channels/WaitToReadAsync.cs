using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Channels
{
    public class WaitToReadAsync
    {
        public static async Task ProducerConsumer()
        {
            var ch = Channel.CreateUnbounded<string>();

            var consumer = Task.Run(async () =>
            {
                while (await ch.Reader.WaitToReadAsync())
                    Console.WriteLine($"Consuming message {await ch.Reader.ReadAsync()}");
                Console.WriteLine("Shutting down consumer");
            });

            var producer = Task.Run(async () =>
            {
                var rnd = new Random();
                for (var i = 0; i < 5; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                    await ch.Writer.WriteAsync($"{i}");
                }
                ch.Writer.Complete();
                Console.WriteLine("Shutting down producer");
            });

            await Task.WhenAll(producer, consumer);
        }        
    }

    public class WaitToReadAsyncRunner 
    {
        public static async Task RunAsync() => await WaitToReadAsync.ProducerConsumer();
    }
}