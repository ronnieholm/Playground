using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Security.Authentication;

namespace AsyncTlsSocketEchoServerClient.Client
{
    public class Client
    {
        Socket _server;
        EndPoint _remoteEndPoint;
        SslStream _sslStream;
        X509Certificate2 _clientCertificate;
        CancellationToken _ct;
        Random _random;
        byte[][] _randomData;
       
        const int MaxRandomDataSize = 100;

        public Client(EndPoint remoteEndPoint, X509Certificate2 clientCertificate, CancellationToken ct)
        {
            _server = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _remoteEndPoint = remoteEndPoint;
            _clientCertificate = clientCertificate;
            _ct = ct;
            _random = new Random((int)DateTime.Now.Ticks);
            _randomData = new byte[MaxRandomDataSize][];

            for (var i = 0; i < MaxRandomDataSize; i++)
            {
                _randomData[i] = new byte[i + 1];
                _random.NextBytes(_randomData[i]);
            }
        }

        public void Connect()
        {
            _server.Connect(_remoteEndPoint);
            var networkStream = new NetworkStream(_server);
            _sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            // Client certificate is optional. If we leave it out, when the
            // server calls SslStream.AuthenticateAsServer, the
            // clientCertificateRequired argument must be false or mutual
            // authentication will fail. Regardless, we use this overload, to
            // specify TLS 1.2. Certificate revocation checks for a self-signed
            // certificate makes no sense and will throw an exception so we
            // disable checkCertificateRevocation.
            _sslStream.AuthenticateAsClient("server.bugfree.dk", /* clientCertificates */ new X509CertificateCollection { _clientCertificate }, SslProtocols.Tls12, /* checkCertificateRevocation */ false);
            Logger.Log($"Connected {_server.LocalEndPoint} to server");
            Logger.LogStream(_sslStream);
            Logger.LogCertificates(_sslStream);
        }

        public void Disconnect()
        {
            var endPoint = _server.LocalEndPoint;
            _sslStream.Close();
            _server.Shutdown(SocketShutdown.Both);

            // For some reason, a reuseSocket value of true causes Disconnect to not return.
            _server.Disconnect(reuseSocket: false);
            _server.Close();
            Logger.Log($"Disconnected {endPoint} from server");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        async public void EchoAsync()
        {
            try
            { 
                while (!_ct.IsCancellationRequested)
                {
                    var data = _randomData[_random.Next(MaxRandomDataSize)];
                    await _sslStream.WriteAsync(data, 0, data.Length, _ct);                
                    await _sslStream.ReadAsync(data, 0, data.Length, _ct);
                }
            }
            catch (Exception e)
            {
                // Exceptions are thrown for each socket when server shuts down
                Logger.Log("--> " + e);
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

    public class Program
    {
        public static int Main(String[] args)
        {
// Set to false in order to launch client from within Visual Studio (for
// debugging) and true to run from client.ps1.
#if false
            var ipAddress = IPAddress.Parse(args[0]);
            var endPoint = new IPEndPoint(ipAddress, int.Parse(args[1]));
            var clientCertificate = new X509Certificate2(args[2], args[3]);
            var connections = int.Parse(args[4]);
#else
            var ipAddress = IPAddress.Parse("10.0.2.15");
            var endPoint = new IPEndPoint(ipAddress, 10000);
            var clientCertificate = new X509Certificate2("../../../../../certificates/client.bugfree.dk.pfx", "securepw");
            var connections = 1;
#endif

            var clients = new Client[connections];
            var cts = new CancellationTokenSource();

            for (var i = 0; i < connections; i++)
            {
                clients[i] = new Client(endPoint, clientCertificate, cts.Token);
                clients[i].Connect();
            }

            Logger.Log("Clients connected. Press any key start sending and receiving.");
            Console.ReadKey();

            for (var i = 0; i < connections; i++)
            {
                clients[i].EchoAsync();
            }

            Logger.Log("Started sending/receiving. Press any key to disconnect clients.");
            Console.ReadKey();

            for (var i = 0; i < connections; i++)
            {
                clients[i].Disconnect();
            }

            Logger.Log("Clients disconnect. Press any key to exit process.");
            Console.ReadKey();
            return 0;
        }
    }
}