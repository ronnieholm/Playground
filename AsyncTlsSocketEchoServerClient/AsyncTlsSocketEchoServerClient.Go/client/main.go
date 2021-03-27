package main

import (
	"crypto/tls"
	"crypto/x509"
	"flag"
	"io"
	"io/ioutil"
	"log"
	"math/rand"
	"os"
	"strings"
)

const maxRandomDataSize = 100

var randomData = make([][]byte, 100)

type client struct {
	con *tls.Conn
}

func (c *client) connect(endpoint string, config *tls.Config) {
	//defer conn.Close()
	var err error
	c.con, err = tls.Dial("tcp4", endpoint, config)
	if err != nil {
		log.Fatalf("Unable to connect to server: %s", err)
	}

	c.con.Handshake()
	log.Printf("Connected to server: %s", c.con.LocalAddr())

	state := c.con.ConnectionState()
	for _, v := range state.PeerCertificates {
		log.Printf("PeerCertificate: %v", v.Subject)
	}
	log.Println("HandshakeComplete: ", state.HandshakeComplete)
	log.Println("NegotiatedProtocolIsMutual: ", state.NegotiatedProtocolIsMutual)
}

func (c client) disconnect(endpoint string) {

}

func (c client) echo() {
	for {
		i := rand.Intn(100)
		n, err := c.con.Write(randomData[i])
		if err != nil {
			if strings.Contains(err.Error(), "write: broken pipe") {
				return
			}
			log.Panicf("%d %s", n, err)
		}

		m, err := c.con.Read(randomData[i])
		if err != nil {
			if err == io.EOF {
				return
			}
			log.Panicf("%d %s", m, err)
		}
	}
}

func main() {
	rootCAFile := flag.String("rootCA", "", "root certificate authority file")
	clientCertFile := flag.String("clientCertFile", "", "client certificate file")
	clientKeyFile := flag.String("clientKeyFile", "", "client key file")
	serverEndpoint := flag.String("server", "127.0.0.1:8080", "server IP and port number")
	numClients := flag.Int("clients", 64, "number of clients connecting to the server")
	flag.Parse()

	certPool := x509.NewCertPool()
	rootCA, err := ioutil.ReadFile(*rootCAFile)
	if err != nil {
		log.Fatalf("failed to load root certificate: %s", *rootCAFile)
	}
	ok := certPool.AppendCertsFromPEM(rootCA)
	if !ok {
		log.Fatalf("failed to add PEM certificates to cert pool: %s", *rootCAFile)
	}

	cert, err := tls.LoadX509KeyPair(*clientCertFile, *clientKeyFile)
	if err != nil {
		log.Fatalf("unable to load client certificate: %s", err)
	}
	config := tls.Config{
		Certificates:       []tls.Certificate{cert},
		InsecureSkipVerify: true, // set to true or you get "x509: cannot validate certificate for 127.0.0.1 because it doesn't contain any IP SANs"
		MinVersion:         tls.VersionTLS12,
		RootCAs:            certPool}

	for i := 0; i < maxRandomDataSize; i++ {
		randomData[i] = make([]byte, i)
		rand.Read(randomData[i])
	}

	clients := make([]client, *numClients)
	for i := 0; i < *numClients; i++ {
		clients[i].connect(*serverEndpoint, &config)
	}

	b := make([]byte, 1)
	println("Press any key to start sending and receiving")
	os.Stdin.Read(b)

	for i := 0; i < *numClients; i++ {
		go clients[i].echo()
	}

	println("Press any key to stop sending and receiving")
	os.Stdin.Read(b)
}
