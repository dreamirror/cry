using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Octodiff.Diagnostics;
using System.Linq;

namespace Octodiff.Core
{
    public interface ISignatureReader
    {
        Signature ReadSignature();
    }

    public class SignatureReader : ISignatureReader
    {
        private readonly BinaryReader reader;

        public SignatureReader(Stream stream)
        {
            this.reader = new BinaryReader(stream);
        }

        public Signature ReadSignature()
        {
            var header = reader.ReadBytes(BinaryFormat.SignatureHeader.Length);
            if (!Enumerable.SequenceEqual(BinaryFormat.SignatureHeader, header)) 
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            var version = reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException("The signature file uses a newer file format than this program can handle.");

            var chunkSize = reader.ReadInt16();

            var hashAlgorithm = reader.ReadString();
            var rollingChecksumAlgorithm = reader.ReadString();

            var endOfMeta = reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!Enumerable.SequenceEqual(BinaryFormat.EndOfMetadata, endOfMeta)) 
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            var hashAlgo = SupportedAlgorithms.Hashing.Create(hashAlgorithm);
            var signature = new Signature(
                hashAlgo,
                SupportedAlgorithms.Checksum.Create(rollingChecksumAlgorithm), chunkSize);

            var expectedHashLength = hashAlgo.HashLength;
            long start = 0;

            var fileLength = reader.BaseStream.Length;
            var remainingBytes = fileLength - reader.BaseStream.Position;
            var signatureSize = sizeof (uint) + expectedHashLength;
            if (remainingBytes % signatureSize != 0)
                throw new CorruptFileFormatException("The signature file appears to be corrupt; at least one chunk has data missing.");

            while (reader.BaseStream.Position < fileLength - 1)
            {
                var checksum = reader.ReadUInt32();
                var chunkHash = reader.ReadBytes(expectedHashLength);

                signature.AddChunkMap(start, checksum, chunkHash);

                start += chunkSize;
            }

            return signature;
        }
    }
}