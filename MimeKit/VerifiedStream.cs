using MimeKit.Cryptography;
using MimeKit.IO;

namespace MimeKit
{
    public class VerifiedStream 
    {
        public MemoryBlockStream Memory { get; set; }
        public DigitalSignatureCollection Signatures { get; set; }

        public VerifiedStream(MemoryBlockStream memory, DigitalSignatureCollection signatures)
        {
            Memory = memory;
            Signatures = signatures;
        }
    }
}