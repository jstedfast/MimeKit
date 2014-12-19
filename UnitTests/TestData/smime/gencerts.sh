#!/bin/sh

# Generate a certificate authority certificate
echo "Country Name: US"
echo "State or Province Name: Massachusetts"
echo "Locality Name: Boston"
echo "Organization Name: Example Authority Inc."
echo "Common Name: Bruce Wayne"
echo "Email Address: bruce.wayne@example.com"
openssl genrsa -des3 -out certificate-authority.key 4096 > /dev/null
openssl req -new -x509 -days 365 -key certificate-authority.key -out certificate-authority.crt

# Generate an S/MIME certificate for testing
echo "Country Name: US"
echo "State or Province Name: Massachusetts"
echo "Locality Name: Boston"
echo "Organization Name: "
echo "Common Name: MimeKit UnitTests"
echo "Email Address: mimekit@example.com"
openssl genrsa -des3 -out smime.key 4096 > /dev/null
openssl req -new -key smime.key -out smime.csr
openssl x509 -req -days 365 -in smime.csr -CA certificate-authority.crt -CAkey certificate-authority.key -set_serial 1 -out smime.crt -setalias "mimekit@example.com" -addtrust emailProtection -addreject clientAuth -addreject serverAuth -trustout
openssl pkcs12 -export -in smime.crt -inkey smime.key -out smime.p12
