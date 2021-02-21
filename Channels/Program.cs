using System.Threading.Tasks;

// Introduction, larger example with splitting and merging channels:
// https://deniskyashif.com/2019/12/08/csharp-channels-part-1/
// https://deniskyashif.com/2019/12/11/csharp-channels-part-2/
// https://deniskyashif.com/2020/01/07/csharp-channels-part-3/

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