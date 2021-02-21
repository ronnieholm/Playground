using System.Threading.Tasks;

// https://deniskyashif.com/csharp-channels-part-1/
// https://deniskyashif.com/csharp-channels-part-2/

namespace Channels
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await MyChannelRunner.Run();
            await WaitToReadAsyncRunner.Run();
            await ReadAllAsyncRunner.Run();
        }
    }
}