#!/bin/sh

# Create the private key for the root (CA) certificate
if [ ! -e "certificate-authority.key" ]; then
    openssl genrsa -out certificate-authority.key 4096 > /dev/null
fi

# Create the private key for the primary intermediate certificate
if [ ! -e "intermediate1.key" ]; then
    openssl genrsa -out intermediate1.key 4096 > /dev/null
fi

# Create the private key for the secondary intermediate certificate
if [ ! -e "intermediate2.key" ]; then
    openssl genrsa -out intermediate2.key 4096 > /dev/null
fi

# Create the private key for the leaf-node S/MIME certificate
if [ ! -e "smime.key" ]; then
    openssl genrsa -out smime.key 4096 > /dev/null
fi

if [ ! -e "BouncyCastle.Crypto.dll" ]; then
    cp ../../bin/Debug/BouncyCastle.Crypto.dll .
fi

mcs mkcert.cs -r:BouncyCastle.Crypto.dll

# Create the root (CA) certificate
mono ./mkcert.exe certificate-authority.cfg > fingerprints.txt

# Create the primary intermediate certificate
mono ./mkcert.exe intermediate1.cfg >> fingerprints.txt

# Create the secondary intermediate certificate
mono ./mkcert.exe intermediate2.cfg >> fingerprints.txt

# Generate an S/MIME certificate for testing
mono ./mkcert.exe smime.cfg >> fingerprints.txt

cat fingerprints.txt
