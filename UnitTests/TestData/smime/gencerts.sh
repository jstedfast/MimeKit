#!/bin/sh

# Create the private key for the root (CA) certificate
openssl genrsa -out certificate-authority.key 4096 > /dev/null

# Create the root (CA) certificate
echo "========= Certificate Authority ========="
echo "Country Name: US"
echo "State or Province Name: Massachusetts"
echo "Locality Name: Boston"
echo "Organization Name: Example Authority Inc."
echo "Organizational Unit Name: "
echo "Common Name: Bruce Wayne"
echo "Email Address: root@example.com"
echo "========================================="
openssl req -new -x509 -days 3650 -nodes -key certificate-authority.key -out certificate-authority.crt \
	-subj "/C=US/ST=Massachusetts/L=Boston/O=Example Authority Inc./CN=Bruce Wayne/emailAddress=root@example.com"

# Create the private key for the intermediate certificate
openssl genrsa -out intermediate.key 4096 > /dev/null

# Create the intermediate certificate signing request (CSR)
echo "========= Intermediate Certificate ========="
echo "Country Name: US"
echo "State or Province Name: Massachusetts"
echo "Locality Name: Boston"
echo "Organization Name: Example Authority Inc."
echo "Organizational Unit Name: "
echo "Common Name: Bruce Wayne"
echo "Email Address: intermediate@example.com"
echo "========================================="
openssl req -new -key intermediate.key -out intermediate.csr \
	-subj "/C=US/ST=Massachusetts/L=Boston/O=Example Authority Inc./CN=Bruce Wayne/emailAddress=intermediate@example.com"

# Sign the CSR using the root (CA) certificate
openssl x509 -req -in intermediate.csr -CA certificate-authority.crt -CAkey certificate-authority.key \
	-CAcreateserial -days 3650 -out intermediate.crt

# Create the certificate chain
cat intermediate.crt certificate-authority.crt > chain.crt

# Generate an S/MIME certificate for testing
echo "======== S/MIME Test Certificate ========"
echo "Country Name: US"
echo "State or Province Name: Massachusetts"
echo "Locality Name: Boston"
echo "Organization Name: "
echo "Common Name: MimeKit UnitTests"
echo "Email Address: mimekit@example.com"
echo "========================================="
openssl genrsa -des3 -out smime.key 4096 > /dev/null
openssl req -new -key smime.key -out smime.csr \
	-subj "/C=US/ST=Massachusetts/L=Boston/CN=MimeKit UnitTests/emailAddress=mimekit@example.com"
openssl x509 -req -days 3650 -in smime.csr -CA intermediate.crt -CAkey intermediate.key -set_serial 1 \
	-out smime.crt -setalias "mimekit@example.com" -addtrust emailProtection \
	-addreject clientAuth -addreject serverAuth -trustout
openssl pkcs12 -export -in smime.crt -inkey smime.key -out smime.p12 -chain -CAfile chain.crt
