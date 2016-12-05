using System.IO;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public interface ISignatureWriter
    {
        void WriteMetadata(short chunkSize, IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm);
        void WriteChunk(ChunkSignature signature);
    }

    public class SignatureWriter : ISignatureWriter
    {
        private readonly BinaryWriter signatureStream;

        public SignatureWriter(Stream signatureStream)
        {
            this.signatureStream = new BinaryWriter(signatureStream);
        }

        public void WriteMetadata(short chunkSize, IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm)
        {
            signatureStream.Write(BinaryFormat.SignatureHeader);
            signatureStream.Write(BinaryFormat.Version);
            signatureStream.Write(chunkSize);
            signatureStream.Write(hashAlgorithm.Name);
            signatureStream.Write(rollingChecksumAlgorithm.Name);
            signatureStream.Write(BinaryFormat.EndOfMetadata);
        }

        public void WriteChunk(ChunkSignature signature)
        {
            signatureStream.Write(signature.RollingChecksum);
            signatureStream.Write(signature.Hash);
        }
    }

    public class SignatureBuilder
    {
        public static readonly short MinimumChunkSize = 128;
        public static readonly short DefaultChunkSize = 2048;
        public static readonly short MaximumChunkSize = 31 * 1024;

        private short chunkSize;

        public SignatureBuilder()
        {
            ChunkSize = DefaultChunkSize;
            HashAlgorithm = SupportedAlgorithms.Hashing.Sha1();
            RollingChecksumAlgorithm = SupportedAlgorithms.Checksum.Default();
            ProgressReporter = new NullProgressReporter();
        }

        public IProgressReporter ProgressReporter { get; set; }

        public IHashAlgorithm HashAlgorithm { get; set; }

        public IRollingChecksum RollingChecksumAlgorithm { get; set; }

        public short ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value < MinimumChunkSize)
                    throw new UsageException(string.Format("Chunk size cannot be less than {0}", MinimumChunkSize));
                if (value > MaximumChunkSize)
                    throw new UsageException(string.Format("Chunk size cannot be exceed {0}", MaximumChunkSize));
                chunkSize = value;
            }
        }

        public void Build(Stream stream, ISignatureWriter signatureWriter)
        {
            WriteMetadata(stream, signatureWriter);
            WriteChunkSignatures(stream, signatureWriter);
        }

        void WriteMetadata(Stream stream, ISignatureWriter signatureWriter)
        {
            stream.Seek(0, SeekOrigin.Begin);

            signatureWriter.WriteMetadata(ChunkSize, HashAlgorithm, RollingChecksumAlgorithm);
        }

        void WriteChunkSignatures(Stream stream, ISignatureWriter signatureWriter)
        {
            var checksumAlgorithm = RollingChecksumAlgorithm;
            var hashAlgorithm = HashAlgorithm;

            ProgressReporter.ReportProgress("Building signatures", 0, stream.Length);
            stream.Seek(0, SeekOrigin.Begin);

            int start = 0;
            int read;
            var block = new byte[ChunkSize];
            while ((read = stream.Read(block, 0, block.Length)) > 0)
            {
                if (read < chunkSize)
                    continue;

                signatureWriter.WriteChunk(new ChunkSignature
                {
                    StartOffset = start,
                    Hash = hashAlgorithm.ComputeHash(block, 0, read),
                    RollingChecksum = checksumAlgorithm.Calculate(block, 0, read)
                });

                start += read;
                ProgressReporter.ReportProgress("Building signatures", start, stream.Length);
            }
        }
    }
}