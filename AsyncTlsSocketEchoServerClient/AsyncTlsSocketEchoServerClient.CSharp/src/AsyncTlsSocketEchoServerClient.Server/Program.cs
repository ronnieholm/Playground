﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTlsSocketEchoServerClient.Server
{
    public class Server
    {
        CancellationTokenSource _cts;
        Socket _listener;
        IPEndPoint _endPoint;

        // For the purpose of this server, we could do with a list of sockets.
        // But being able to quickly lookup a socket based on endpoint (or other
        // information) is generally useful. With a list of sockets, we could
        // use Socket.RemoteEndPoint but that would require traversing the list
        // with every lookup.
        ConcurrentDictionary<EndPoint, Socket> _clients;
        X509Certificate2 _serverCertificate;

        // Statistics displayed on shutdown.
        public int ReadOperations;
        public int WriteOperations;
        public int BytesReceived;
        public int BytesSent;

        public Server(IPEndPoint endpoint, X509Certificate2 serverCertificate)
        {
            _endPoint = endpoint;
            _serverCertificate = serverCertificate;
            _listener = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _cts = new CancellationTokenSource();
            _clients = new ConcurrentDictionary<EndPoint, Socket>();
        }

        async public void Start()
        {
            _listener.Bind(_endPoint);

            // backlog is the number of connections initialized but not yet
            // fully setup. Suppose a client opens a socket to the server, but
            // doesn't initiate the TLS handshake by creating a NetworkStream
            // wrapped in an SslStream. Once the backlog + 1 socket is opened,
            // the server will refuse the connection and Listen throws a
            // SocketException with "An existing connection was forcibly closed
            // by the remote host". On the client, a
            // SocketExceptionFactory+ExtendedSocketException is thrown with "No
            // connection could be made because the target machine actively
            // refused it".
            _listener.Listen(backlog: 10);
            await AcceptClientAsync(_cts.Token).ConfigureAwait(false);
        }

        public void Stop()
        {
            _cts.Cancel();
            foreach (var endPoint in _clients.Keys)
            {
                DisconnectClient(endPoint);
            }
            Debug.Assert(_clients.Count == 0);

            if (_listener.Connected)
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Close();
            }
            Debug.Assert(!_listener.Connected);
            Logger.Log("Server stopped");
        }

        async Task AcceptClientAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Logger.Log("Waiting for a connection");
                var client = await _listener.AcceptAsync().ConfigureAwait(false);
                var endPoint = client.RemoteEndPoint;
                _clients.TryAdd(endPoint, client);
                var task = ProcessEchoAsync(client, endPoint, ct);
            }
        }

        // Handles all processing for a single socket connection.
        async Task ProcessEchoAsync(Socket client, EndPoint endPoint, CancellationToken ct)
        {
            var readOperations = 0;
            var writeOperations = 0;
            var bytesSent = 0;
            var bytesReceived = 0;
            var timeoutCts = new CancellationTokenSource();

            try
            { 
                using (var networkStream = new NetworkStream(client, false))
                using (var sslStream = new SslStream(networkStream, false))
                {
                    // If we didn't include a certificate with the client's call
                    // to SslStream.AuthenticateAsClient, we must set the
                    // clientCertificateRequired to false. When set to true, the
                    // client and server does mutual authentication. In most
                    // cases, like with HTTPS, authenticating the server
                    // suffices. Certificate revocation checks for a self-signed
                    // certificate makes no sense and will throw an exception so
                    // we disable checkCertificateRevocation.
                    sslStream.AuthenticateAsServer(_serverCertificate, /* clientCertificateRequired */ true, SslProtocols.Tls12, /* checkCertificateRevocation */ false);
                    Logger.LogStream(sslStream);
                    Logger.LogCertificates(sslStream);

                    // Every connection is alotted a 1024 bytes buffer. A
                    // smaller buffer would mean less overall server memory use,
                    // but also additional calls to ReadAsync to process the
                    // same amount of data.
                    var buffer = new byte[1024];

                    Logger.Log($"Client connection {endPoint} accepted. {_clients.Count} clients connected to server");
                    while (!ct.IsCancellationRequested)
                    {
                        // When the connection has been idle for this amount of
                        // time, the connection is closed. Could equally well be
                        // used to trigger the sending of a keep-alive packet.
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(240), timeoutCts.Token);

                        // Task doesn't complete until data has been read. So
                        // either data is available (RanToCompletion) or client
                        // disconnected (Faulted) or task is cancelled
                        // (Canceled). 
                        var readTask = sslStream.ReadAsync(buffer, 0, buffer.Length, ct);

                        var completedTask = await Task.WhenAny(timeoutTask, readTask).ConfigureAwait(false);

                        // Checking on readTask state like so is only possible
                        // when ReadAsync was callled on a stream and not
                        // directly on the socket. Keep in mind that the Is*
                        // state values of a task aren't mutually exclusive
                        // whereas the Status values are.
                        if (completedTask == timeoutTask || (readTask.Status == TaskStatus.Canceled || readTask.Status == TaskStatus.Faulted))
                        {
                            break;
                        }

                        if (readTask.Status == TaskStatus.RanToCompletion)
                        {
                            var readBytes = readTask.Result;
                            readOperations++;
                            bytesReceived += readBytes;

                            // Zero bytes read marks end of stream reached. This
                            // happens upon client disconnect.
                            if (readBytes == 0)
                            {
                                break;
                            }
                            else
                            {
                                // await implies that while we're transmitting
                                // we cannot receive. Or rather the operating
                                // system's receive buffer may be filling up,
                                // but we aren't reading from it as we cannot
                                // send to the same client in parallel anyway.
                                // Also, we don't want to risk overriding buffer
                                // with new until we've send the old ones. That
                                // would garble up the transmission.
                                await sslStream.WriteAsync(buffer, 0, readBytes, ct).ConfigureAwait(false);
                                writeOperations++;
                                bytesSent += readBytes;
                                timeoutCts.Cancel();
                                timeoutCts = new CancellationTokenSource();
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                // During a server shutdown through the Stop method, it'll
                // iterate through all clients and close their sockets. This'll
                // usually cause some async method in the loop to throw an
                // exception indicating that the task was cancelled:
                // "System.IO.IOException: The write operation failed, see inner
                // exception. ---> System.Threading.Tasks.TaskCanceledException:
                // A task was canceled". Unless we explicitly catch such
                // exceptions, they'll go unnoticed.
                Logger.Log("--> " + e);
            }
            finally
            {
                timeoutCts.Cancel();
                Interlocked.Add(ref ReadOperations, readOperations);
                Interlocked.Add(ref WriteOperations, writeOperations);
                Interlocked.Add(ref BytesReceived, bytesReceived);
                Interlocked.Add(ref BytesSent, bytesSent);

                // Not called immidiately upon cancellation, but the next time
                // around the loop upon checking the IsCallationRequested
                // property. It's called immidiately when a client disconnects,
                // though.
                DisconnectClient(endPoint);
            }
        }

        void DisconnectClient(EndPoint endPoint)
        {
            // Because we may be called from either the ProcessEchoAsync or the
            // Stop method, and potentially in parallel, the socket may have
            // already been shutdown, closed, and removed from _clients.
            var ok = _clients.TryRemove(endPoint, out Socket client);
            if (ok)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Logger.Log($"Client {endPoint} disconnected. {_clients.Count} clients connected to server");
            }
        }
    }

    static class Logger
    {
        // Ideally, Console should be protected by a lock to prevent garbled
        // output.
        public static void Log(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
        }

        public static void LogStream(SslStream s)
        {
            Log($"CipherAlgorithm: {s.CipherAlgorithm}");
            Log($"CipherStrength: {s.CipherStrength}");
            Log($"HashAlgorithm: {s.HashAlgorithm}");
            Log($"IsAuthenticated: {s.IsAuthenticated}");
            Log($"IsEncrypted: {s.IsEncrypted}");
            Log($"IsMutuallyAuthenticated: {s.IsMutuallyAuthenticated}");
            Log($"IsServer: {s.IsServer}");
            Log($"IsSigned: {s.IsSigned}");
            Log($"SslProtocol: {s.SslProtocol}");
        }

        public static void LogCertificates(SslStream s)
        {
            Log($"RemoteCertificate.Issuer: {s.RemoteCertificate.Issuer}");
            Log($"RemoteCertificate.Subject: {s.RemoteCertificate.Subject}");
            Log($"RemoteCertificate.Thumbprint: {s.RemoteCertificate.GetCertHashString()}");
            Log($"LocalCertificate.Issuer: {s.LocalCertificate.Issuer}");
            Log($"LocalCertificate.Subject: { s.LocalCertificate.Subject}");
            Log($"LocalCertificate.Thumbprint: {s.LocalCertificate.GetCertHashString()}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
// Set to false in order to launch server from within Visual Studio (for
// debugging) and true to run from server.ps1.
#if false
            var ipAddress = IPAddress.Parse(args[0]);
            var remoteEndPoint = new IPEndPoint(ipAddress, int.Parse(args[1]));
            var serverCertificate = new X509Certificate2(args[2], args[3]);
#else
            var ipAddress = IPAddress.Parse("10.0.2.15");
            var remoteEndPoint = new IPEndPoint(ipAddress, 10000);
            var serverCertificate = new X509Certificate2("../../../../../certificates/server.bugfree.dk.pfx", "securepw");
#endif

            Logger.Log("Server started. Press any key to stop server.");
            Logger.Log(new string('-', 80));
            var server = new Server(remoteEndPoint, serverCertificate);
            server.Start();
            Console.ReadKey();
            server.Stop();

            // Just because we stopped the server's listener and closed all
            // client connections doesn't necessarily mean that every async
            // method processing each client has finished executing. We'll give
            // the async tasks a change to finish and update statistics before
            // displaying. Without the sleep, and if we only had a single
            // client, statistics would all be zero because the methods finally
            // block have yet executed. With more clients the results of one of
            // clients would likely be discarded.
            Thread.Sleep(1000);
            Logger.Log($"Read operations: {server.ReadOperations}");
            Logger.Log($"Write operations: {server.WriteOperations}");
            Logger.Log($"Bytes received: {server.BytesReceived}");
            Logger.Log($"Bytes sent: {server.BytesSent}");
            Logger.Log("Server stopped. Press any key to exit process.");
            Console.ReadKey();
        }
    }
}