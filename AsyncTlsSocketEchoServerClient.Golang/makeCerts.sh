# Adapted from: https://gist.github.com/fntlnz/cf14feb5a46b2eda428e000157447309

rm *.key
rm *.csr
rm *.crt

# Generate private? key used to sign certificate requests. Anyone holding this
# key can sign certificates on your behalf.
openssl genrsa -out bugfree.rootCA.key 2048

# Generate and self-sign a root certificate using the previously generated key.
openssl req -subj "/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=ca.bugfree.dk" -x509 -new -nodes -days 3650 -key bugfree.rootCA.key -out bugfree.rootCA.crt

# Generate private key 
openssl genrsa -out server.bugfree.dk.key 2048
openssl genrsa -out client.bugfree.dk.key 2048

# Generate certificate signing request
openssl req -new -subj "/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=server.bugfree.dk" -key server.bugfree.dk.key -out server.bugfree.dk.csr
openssl req -new -subj "/C=DK/ST=Sjaelland/O=Bugfree Consulting/CN=client.bugfree.dk" -key client.bugfree.dk.key -out client.bugfree.dk.csr

# Generate certificate using the key, signing request, and the CA key and certificate
openssl x509 -req -in server.bugfree.dk.csr -CA bugfree.rootCA.crt -CAkey bugfree.rootCA.key -CAcreateserial -out server.bugfree.dk.crt -days 3620 -sha256
openssl x509 -req -in client.bugfree.dk.csr -CA bugfree.rootCA.crt -CAkey bugfree.rootCA.key -CAcreateserial -out client.bugfree.dk.crt -days 3620 -sha256

# View content of requests
openssl req -in server.bugfree.dk.csr -text -noout
openssl req -in client.bugfree.dk.csr -text -noout

# View content of certificates
openssl x509 -in bugfree.rootCA.crt -text -noout
openssl x509 -in server.bugfree.dk.crt -text -noout
openssl x509 -in client.bugfree.dk.crt -text -noout
