using System.Threading.Tasks;

// https://deniskyashif.com/csharp-channels-part-1/
// https://deniskyashif.com/csharp-channels-part-2/

namespace Channels
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            await MyChannelRunner.RunAsync();
            await WaitToReadAsyncRunner.RunAsync();
            await ReadAllAsyncRunner.RunAsync();
            DependentClientsRunner.Run();
        }
    }
}