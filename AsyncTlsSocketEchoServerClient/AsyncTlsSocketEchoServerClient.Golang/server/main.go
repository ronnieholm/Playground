package main

// https://github.com/denji/golang-tls

// [ ] Switch to struct for server
// [ ] Use waitgroup (semaphor) in C# implementation instead of wait
// [ ] Get debugger working for client and server
// [ ] Add localhost/127.0.0.1 to certificate using special config file like Ramon does in order to not fail test: https://www.digicert.com/subject-alternative-name.htm
// [ ] Go through Go tooling in action (https://www.youtube.com/watch?v=uBjoTxosSys)
// [ ] Use https://golang.org/pkg/context/
// [ ] Experiment with the value of runtime.GOMAXPROCS(100) (1 for concurrent, -1 to query). Default is one thread per core but oftentimes better perf can be achieved
// [ ] Check for race conditions at compile time through "go run -race src src/server"
// [ ] How to pass a root CA on the command-line for openssl? (https://github.com/ctz/rustls to send hello world by command-line)
// [ ] Run performance benchmark under Windows
// [ ] Run Go Critic (https://github.com/go-critic/go-critic)
// [ ] Benchmark with pprof
// [ ] Get debugger working for client and server
// [ ] Experiment with certs/certificat.conf
// [ ] Test out using OpenSSL library binding
// [ ] Execute go run -rave server/main.go
// [ ] Implement proper error handling (https://www.youtube.com/watch?v=lsBF58Q-DnY)
// [ ] Consider using io.Copy instead of read and write operations
// [ ] Look at https://www.youtube.com/watch?v=5buaPyJ0XeQ, 13:30 for how to avoid mutexes and use channels instead. Look very much like agent main loop in F#. Instead of discriminated using, we switch over channels instead
// [ ] https://www.yellowduck.be/posts/graceful-shutdown/ for use in client and/or server?

import (
	"crypto/tls"
	"crypto/x509"
	"flag"
	"io/ioutil"
	"log"
	"net"
	"os"
	"strings"
	"sync"
	"sync/atomic"
	"time"
)

type statistics struct {
	readOperations  uint64
	writeOperations uint64
	bytesReceived   uint64
	bytesSent       uint64
}

var (
	clients     = make(map[string]*tls.Conn)
	clientsLock = sync.Mutex{}
	listener    net.Listener
	stats       statistics
	wg          = sync.WaitGroup{}
)

func startServer(endpoint string, config *tls.Config) {
	defer func() {
		// Listener may have already been closed so we ignore any error.
		listener.Close()

		// Gracefully disconnect clients.
		clientsLock.Lock()
		defer clientsLock.Unlock()
		for _, v := range clients {
			err := v.Close()
			if err != nil {
				log.Panic(err)
			}
		}
	}()

	var err error
	listener, err = tls.Listen("tcp4", endpoint, config)
	if err != nil {
		log.Panicf("Unable to listen on endpoint: %s", err)
	}
	log.Printf("Listening on %s", listener.Addr())

	for {
		con, err := listener.Accept()
		if err != nil {
			if strings.Contains(err.Error(), "use of closed network connection") {
				break
			}
			log.Panicf("unable to accept connection: %s", err)
		}

		tlsCon, ok := con.(*tls.Conn)
		tlsCon.Handshake()

		// Under what condition and !ok happen?
		if ok {
			state := tlsCon.ConnectionState()
			for _, v := range state.PeerCertificates {
				log.Printf("PeerCertificate: %v", v.Subject)
			}
			log.Printf("HandshakeComplete: %v", state.HandshakeComplete)
			log.Printf("NegotiatedProtocolIsMutual %v: ", state.NegotiatedProtocolIsMutual)
		}

		go handleEcho(tlsCon)
	}
}

func handleEcho(con *tls.Conn) {
	wg.Add(1)
	s := statistics{}
	defer func() {
		wg.Done()

		// It may already have been closed so we ignore any error.
		con.Close()

		atomic.AddUint64(&stats.readOperations, s.readOperations)
		atomic.AddUint64(&stats.writeOperations, s.writeOperations)
		atomic.AddUint64(&stats.bytesReceived, s.bytesReceived)
		atomic.AddUint64(&stats.bytesSent, s.bytesSent)

		clientsLock.Lock()
		defer clientsLock.Unlock()
		addr := con.RemoteAddr().String()
		delete(clients, addr)
		log.Printf("Disconnected client %s (clients: %d)", addr, len(clients))
	}()

	resetTimeout := func() {
		timeoutDuration := 240 * time.Second
		err := con.SetDeadline(time.Now().Add(timeoutDuration))
		if err != nil {
			log.Panicf("Unable to set deadline: %s", err)
		}
	}

	clientsLock.Lock()
	clients[con.RemoteAddr().String()] = con
	log.Printf("Accepted client from %s (clients: %d)", con.RemoteAddr(), len(clients))
	clientsLock.Unlock()

	buf := make([]byte, 1024)
	resetTimeout()

	for {
		n, err := con.Read(buf)
		if n != 0 {
			s.readOperations++
			s.bytesReceived += uint64(n)
		}
		if err != nil {
			log.Printf("Error on read: %s", err)
			return
		}
		resetTimeout()

		m, err := con.Write(buf[:n])
		if m != 0 {
			s.writeOperations++
			s.bytesSent += uint64(m)
		}
		if err != nil {
			log.Printf("Error on write: %s", err)
			return
		}
		resetTimeout()
	}
}

func stopServer() {
	listener.Close()
	wg.Wait()
}

func main() {
	rootCAFile := flag.String("rootCA", "", "root certificate authority file")
	serverCertFile := flag.String("serverCertFile", "", "server certificate file")
	serverKeyFile := flag.String("serverKeyFile", "", "server key file")
	serverEndpoint := flag.String("server", "127.0.0.1:8080", "server IP and port number")
	flag.Parse()

	certPool := x509.NewCertPool()
	rootCA, err := ioutil.ReadFile(*rootCAFile)
	if err != nil {
		log.Fatalf("failed to load root certificate: %s", *rootCAFile)
	}

	ok := certPool.AppendCertsFromPEM(rootCA)
	if !ok {
		log.Fatalf("failed to add certificate to pool: %s", *rootCAFile)
	}

	cert, err := tls.LoadX509KeyPair(*serverCertFile, *serverKeyFile)
	if err != nil {
		log.Fatalf("unable to load server certificate: %s", err)
	}

	config := tls.Config{
		Certificates: []tls.Certificate{cert},
		MinVersion:   tls.VersionTLS12,
		ClientAuth:   tls.RequireAndVerifyClientCert,
		ClientCAs:    certPool}

	b := make([]byte, 1)
	go startServer(*serverEndpoint, &config)
	println("Press any key to stop server")
	os.Stdin.Read(b)

	stopServer()

	log.Printf("Read operations: %d", stats.readOperations)
	log.Printf("Write operations: %d", stats.writeOperations)
	log.Printf("Bytes receiver operations: %d", stats.bytesReceived)
	log.Printf("Bytes sent operations: %d", stats.bytesSent)

	println("Press any key to exit process")
	os.Stdin.Read(b)
}
