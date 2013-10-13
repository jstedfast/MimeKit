#!/bin/sh

# Generate a certificate authority certificate
openssl genrsa -des3 -out certificate-authority.key 4096
openssl req -new -x509 -days 365 -key certificate-authority.key -out certificate-authority.cert

# Generate an S/MIME certificate for testing
openssl genrsa -des3 -out smime.key 4096
openssl req -new -key smime.key -out smime.csr
openssl x509 -req -days 365 -in smime.csr -CA certificate-authority.cert -CAkey certificate-authority.key -set_serial 1 -out smime.cert -setalias "mimekit@example.com" -addtrust emailProtection -addreject clientAuth -addreject serverAuth -trustout
openssl pkcs12 -export -in smime.cert -inkey smime.key -out smime.p12
