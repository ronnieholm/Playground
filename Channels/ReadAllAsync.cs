using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Channels
{
    public class ReadAllAsync
    {
        public static ChannelReader<string> Producer(string msg, int count)
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

        public static async Task ConsumerAsync()
        {
            var joe = Producer("Joe", 5);
            await foreach (var item in joe.ReadAllAsync())
                Console.WriteLine(item);
        }        
    }

    public class ReadAllAsyncRunner 
    {
        public static async Task RunAsync() => await ReadAllAsync.ConsumerAsync();
    }
}