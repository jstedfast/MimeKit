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

# Create the RSA/DSA/EC private keys for the leaf-node S/MIME certificates
if [ ! -e "rsa/smime.key" ]; then
    openssl genrsa -out rsa/smime.key 4096 > /dev/null
fi

if [ ! -e "dsa/smime.key" ]; then
    openssl dsaparam -out dsa/dsaparam.pem 1024
    openssl gendsa -out dsa/smime.key dsa/dsaparam.pem > /dev/null
fi

if [ ! -e "ec/smime.key" ]; then
    openssl ecparam -name secp384r1 -genkey -noout -out ec/smime.key > /dev/null
fi
