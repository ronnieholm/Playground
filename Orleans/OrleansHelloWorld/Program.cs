using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace OrleansHelloWorld;

public interface IUrlShortenerGrain : IGrainWithStringKey
{
    [Alias("SetUrl")]
    Task SetUrl(string fullUrl);

    [Alias("GetUrl")]
    Task<string> GetUrl();

    Task<string> Ping(string name);
}

[GenerateSerializer, Alias(nameof(UrlShortenerGrainState))]
public sealed record UrlShortenerGrainState
{
    [Id(0)] public string FullUrl { get; set; } = "";
    [Id(1)] public string ShortenedRouteSegment { get; set; } = "";
}

public sealed class UrlShortenerGrain(
    [PersistentState(
        stateName: "url",
        storageName: "urls")]
    IPersistentState<UrlShortenerGrainState> state)
    : Grain, IUrlShortenerGrain
{
    public async Task SetUrl(string fullUrl)
    {
        state.State = new UrlShortenerGrainState
        {
            ShortenedRouteSegment = this.GetPrimaryKeyString(),
            FullUrl = fullUrl
        };

        await state.WriteStateAsync();
    }

    public Task<string> GetUrl() => Task.FromResult(state.State.FullUrl);

    public Task<string> Ping(string name) => Task.FromResult(name);
}

class Application(IGrainFactory grains)
{
    public async Task RunUrlShortenerAsync()
    {
        var shortened = Guid.NewGuid().GetHashCode().ToString("X");
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortened);
        await shortenerGrain.SetUrl("https://bugfree.dk");
        var originalUrl = await shortenerGrain.GetUrl();
        Console.WriteLine($"Shortened: {shortened}, Original: {originalUrl}");
    }

    public async Task RunPingPongBenchmark()
    {
        const int max = 1_000_000;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < max; i++)
        {
            var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>("ABC");
            var name = await shortenerGrain.Ping("Ronnie");
            Debug.Assert(name == "Ronnie");
        }
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms. Messages/second: {max / (sw.ElapsedMilliseconds / 1000.0)}.");
    }
}

static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = new HostBuilder()
            .UseOrleans(static siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.AddMemoryGrainStorage("urls");
            });

        builder.ConfigureServices(s => s.AddTransient<Application, Application>());
        var host = await builder.StartAsync();
        var application = host.Services.GetService<Application>();
        await application!.RunUrlShortenerAsync();
        await application!.RunPingPongBenchmark();
        
        Console.WriteLine("Waiting for key press");
        Console.ReadKey();
    }
}