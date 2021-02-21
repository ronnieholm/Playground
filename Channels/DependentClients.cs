using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// An alternative to ProducerClient placing events on a channel for
// ConsumerClient would be for ProducerClient to emit events by either calling
// Actions, Funcs, or delegates. One possible downside to this approach is that
// callee may block Actions, Funcs, or delegates calls.
//
// In some scenarios, ProducerClient and ConsumerClient can be one client,
// maintaining a synchronized domain model to the rest of the application.
// Some way to indicate updates to the model, and what those updates entail
// might still be required. Even if the UI is data-bound to the model some
// updates might still be considered out of band for the UI.
//
// For instance, how to show a "UserLeft" when the user is gone from the users
// collection. Perhaps by maintaining a list of events since that UI update,
// containing changes to the domain model.
//
// Updating a UI is typically a quick operation. So instead of updating the
// domain on one thread, with state shared/synchronized between that thread and
// UI thread, consider updating the domain and UI in serial on the same thread,
// or at least schedule the two to not overlap.
//
// Or have a Clone() on the domain and events to deep copy it. This would
// ensure predictable locking for the shortest possible time while cloning.

namespace Channels
{
    public abstract record Message(string Room, DateTime Time) 
    {
    }

    public record UserJoined(string Room, DateTime Time, string Username) : Message(Room, Time)
    {
    }

    // Client communicating with a server, updating state according to server
    // responses. Perhaps server is long-polled using HTTP GET requests.
    public class ProducerClient
    {
        private readonly Random _rng = new();
        private readonly Channel<Message> _channel = Channel.CreateUnbounded<Message>();

        public ChannelReader<Message> EventsChannel { get; }

        public ProducerClient()
        {
            EventsChannel = _channel.Reader;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Starting server");

            // Wait for random period, then emit a random event.
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await Task.Delay(_rng.Next(2, 10) * 1000, cancellationToken);
                var join = new UserJoined("Room1", DateTime.Now, "Joe");
                await _channel.Writer.WriteAsync(join, cancellationToken);
            }
        }
    }

    // Client considered part of UI, processing events from ProducerClient into
    // a data structure to be presented in a UI.
    public class ConsumerClient
    {
        // Application state exposed in UI.
        private readonly List<Message> _messages = new();
        
        // Updates application state in the background.
        public async Task RunAsync(ChannelReader<Message> events, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Starting client");
            await foreach (var e in events.ReadAllAsync(cancellationToken))
            {
                // Switch on typeof(Message) and update application state.
                _messages.Add(e);
                Console.WriteLine($"{_messages.Count}: {e}");
            }
        }
    }

    public static class DependentClientsRunner 
    {
        public static void Run()
        {
            var server = new ProducerClient();
            var client = new ConsumerClient();

            // Kick off server.
            var serverTask = Task.Run(async () =>
            {
                await server.RunAsync();
            });

            // Kick off client.
            var clientTask = Task.Run(async () =>
            {              
                // Similarly, client to server communication could happen on a
                // separate command channel passed into the server.
                await client.RunAsync(server.EventsChannel);
            });

            Task.WaitAll(serverTask, clientTask);
            Console.WriteLine("Shutting down runner");
        }
    }
}