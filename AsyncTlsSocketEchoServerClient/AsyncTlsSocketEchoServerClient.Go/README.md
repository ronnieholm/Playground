# AsyncTlsSocketEchoServerClient (Go)

This repository contains the Golang implementation of a high
performance, highly scalable, asynchronous TLS socket echo server and
client. For performance comparison, it's an implementation of a
[similar server and client](https://github.com/ronnieholm/Playground/tree/master/AsyncTlsSocketEchoServerClient.CSharp)
written in C# and running on .NET Core.

## Generating certificates

The solution is primarily developed on Linux and as such makes use of
OpenSSL for certificate generation. The root certificate authority's
certificate and the self-signed server and client certificates are
generated by the following script:

    % ./makeCerts.sh
	
The script re-creates the certificates in the certs folder. For
convenience sake, certificates are part of the repository.

Because the Golang standard library comes with a self-contained
[Crypto/tls implementation](https://golang.org/pkg/crypto/tls),
OpenSSL on Linux and the Certificate Store on Windows aren't required
to run the Echo server and client. The Crypto/tls library isn't as
feature complete as OpenSSL, so in more advanced, or
[secure](https://groups.google.com/forum/#!topic/golang-nuts/0za-R3wVaeQ)),
cases, the [OpenSSL bindings for
Golang](https://github.com/spacemonkeygo/openssl] is an alternative.

## Running server and client

Running the server and client is done by issuing the commands below:

    % go run server/main.go -rootCA certs/bugfree.rootCA.crt -serverCertFile certs/server.bugfree.dk.crt -serverKeyFile certs/server.bugfree.dk.key -server 127.0.0.1:8080
	% go run client/main.go -rootCA certs/bugfree.rootCA.crt -clientCertFile certs/client.bugfree.dk.crt -clientKeyFile certs/client.bugfree.dk.key -server 127.0.0.1:8080 -clients 64
	
Once a client connects to the server, the server outputs the
certificate used and other information:

    2018/08/03 13:57:10 Server listening on 127.0.0.1:8080
    2018/08/03 13:57:14 PeerCertificates:  CN=client.bugfree.dk,O=Bugfree Consulting,ST=Sjaelland,C=DK
    2018/08/03 13:57:14 HandshakeComplete:  true
    2018/08/03 13:57:14 NegotiatedProtocolIsMutual:  true
    2018/08/03 13:57:14 Accepted connection from 127.0.0.1:51688 (total: 1)

Similarly, the client outputs the certificate used and other
information:

    2018/08/03 13:57:14 Connected to server: 127.0.0.1:51688
    2018/08/03 13:57:14 PeerCertificates:  CN=server.bugfree.dk,O=Bugfree Consulting,ST=Sjaelland,C=DK
    2018/08/03 13:57:14 HandshakeComplete:  true
    2018/08/03 13:57:14 NegotiatedProtocolIsMutual:  true
	
Note that mutual authentication is enabled and that the server is
using the client's certificate and the client is using the server's
certificate.

## Playing with certificates

Using the OpenSSL command-line, we can initiate a TLS connection with
the server. Certificate errors can safely be ignores as OpenSSL
doesn't know about the root CA certificate:

    % openssl s_client -connect 127.0.0.1:8080
    CONNECTED(00000003)
	depth=0 C = DK, ST = Sjaelland, O = Bugfree Consulting, CN = server.bugfree.dk
	verify error:num=20:unable to get local issuer certificate
	verify return:1
	depth=0 C = DK, ST = Sjaelland, O = Bugfree Consulting, CN = server.bugfree.dk
	verify error:num=21:unable to verify the first certificate
	verify return:1
	139849712599488:error:14094412:SSL routines:ssl3_read_bytes:sslv3 alert bad certificate:../ssl/record/rec_layer_s3.c:1399:SSL alert number 42
	---
	Certificate chain
	 0 s:/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=server.bugfree.dk
	   i:/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=ca.bugfree.dk
	---
	Server certificate
	-----BEGIN CERTIFICATE-----
	MIIDLDCCAhQCCQCDtgJ5BqYJ1jANBgkqhkiG9w0BAQsFADBWMQswCQYDVQQGEwJE
	SzESMBAGA1UECAwJU2phZWxsYW5kMRswGQYDVQQKDBJCdWdmcmVlIENvbnN1bHRp
	bmcxFjAUBgNVBAMMDWNhLmJ1Z2ZyZWUuZGswHhcNMTgwODAyMjEyMTI5WhcNMjgw
	NjMwMjEyMTI5WjBaMQswCQYDVQQGEwJESzESMBAGA1UECAwJU2phZWxsYW5kMRsw
	GQYDVQQKDBJCdWdmcmVlIENvbnN1bHRpbmcxGjAYBgNVBAMMEXNlcnZlci5idWdm
	cmVlLmRrMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0JUCyJPp5ppA
	y6F/xJmcPksyAZDvFRsSUUlcy8z+9R3/5gM7/XRCN6upJiG9pzjnsKhJ1bhmfsEv
	kuWr/CZzPkUfwXTRSQxoxYo0hLFKc8/dbZkG5NjNJmvaO2IRznqqy/OZ/EOSrYTx
	Me/aeir4zC2ddURYLfEPY9fgpUFJGYjsMIO8vtKhp91cRSxG/uarpnpqwSidjh/f
	obc2kZULGNCxiA+EBCOcl2Re5k8xgKxO35jFsbEZ+F6ckbALKG9x4Cs+QS/qhH1f
	7CBkjK2oEoowxTIwMZo1ZFG6Olz41fhqiLDJlXk4DnGbHBf681KtawOts/NS78NJ
	sSLkdoWQKwIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQCtCoHDrF5r8MHXgN7CqNPh
	FPiBcQWZCaCsH2gq2MC2zFDcKvYdYok/97YlBUC041jUVmpvtL4UxZkOUMuV0+9Q
	TLzbhGpV4xYu1j7PlR3H0YFNHkj7ULzfl1rP3taslvQLFMtnu/HrkTPjV3WyJCVU
	ge+zbzT9fxIKNu6utaQjIHGMV8sic42z+NBm1CJ5W7VvBoK8oaEyxmV7PmMc+s2c
	PJdOMiSWbfkZD3f5KayW+Un5BCd73wb2vUE/AsBrYVukhjo4oUsbXzO+B8eEKB1n
	Xv9FnifjKGRG/j1y6K9GN0ZcGgqLMxZUYXrOhacuX0xdhbH4QLbYbmbbBkllnHIb
	-----END CERTIFICATE-----
	subject=/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=server.bugfree.dk
	issuer=/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=ca.bugfree.dk
	---
	Acceptable client certificate CA names
	/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=ca.bugfree.dk
	Client Certificate Types: RSA sign, ECDSA sign
	Requested Signature Algorithms: RSA+SHA256:ECDSA+SHA256:RSA+SHA384:ECDSA+SHA384:RSA+SHA512:ECDSA+SHA512:RSA+SHA1:ECDSA+SHA1
	Shared Requested Signature Algorithms: RSA+SHA256:ECDSA+SHA256:RSA+SHA384:ECDSA+SHA384:RSA+SHA512:ECDSA+SHA512:RSA+SHA1:ECDSA+SHA1
	Peer signing digest: SHA512
	Server Temp Key: X25519, 253 bits
	---
	SSL handshake has read 1332 bytes and written 281 bytes
	Verification error: unable to verify the first certificate
	---
	New, TLSv1.2, Cipher is ECDHE-RSA-AES256-GCM-SHA384
	Server public key is 2048 bit
	Secure Renegotiation IS supported
	Compression: NONE
	Expansion: NONE
	No ALPN negotiated
	SSL-Session:
		Protocol  : TLSv1.2
		Cipher    : ECDHE-RSA-AES256-GCM-SHA384
		Session-ID: 
		Session-ID-ctx: 
		Master-Key: BE13F4786982DA04A1C1BD9F7D57DAD61883233D8754720C1941117AB5CDF39FF10F6D51B4C948D5C3503623A9FB72EA
		PSK identity: None
		PSK identity hint: None
		SRP username: None
		Start Time: 1533296716
		Timeout   : 7200 (sec)
		Verify return code: 21 (unable to verify the first certificate)
		Extended master secret: no

Note the certificates chain from server.bugfree.dk to ca.bugfree.dk
and that the handshake was made for TLSv1.2 which the server is setup
to require.

## References

- OpenSSL command-line options to [view and convert between common certificate formats](https://support.ssl.com/Knowledgebase/Article/View/19/0/der-vs-crt-vs-cer-vs-pem-certificates-and-how-to-convert-them).

- How to setup [mutual TLS authentication in Go](http://www.levigross.com/2015/11/21/mutual-tls-authentication-in-go). 
  Getting the tls.Config right for mutual authentication was especially challenging.

 - [The GO Programmer's Guide to Secure Connections](https://www.youtube.com/watch?v=kxKLYDLzuHA&feature=youtu.be&list=PL2ntRZ1ySWBdatAqf-2_125H4sGzaWngM).
