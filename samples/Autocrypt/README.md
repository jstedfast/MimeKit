# Offline Autocrypt exchange

From the repository root, run:

```powershell
dotnet run --project samples\Autocrypt\Autocrypt.csproj
```

Alice sends a plaintext message advertising her key. Bob records the observation
in his own peer dictionary. Both accounts prefer encryption, so MimeKit recommends
`Encrypt` for Bob's reply. The sample chooses to encrypt; it does not fall back
to plaintext. Alice decrypts the reply and checks its signature using the key
advertised in that reply. A matching signature does not establish Bob's identity.

Each run generates two disposable accounts. Messages cross an in-memory stream
as MIME bytes; no mail is sent and no keyrings or peer records are saved. A real
application owns its key and peer storage, filters spam before applying
observations, and makes the final send decision.
