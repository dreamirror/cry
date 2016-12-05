using System;
using System.Collections;
using System.IO;
using System.Security.Policy;
using Octodiff.Diagnostics;
using System.Linq;

namespace Octodiff.Core
{
    public interface IDeltaReader
    {
        byte[] ExpectedHash { get; }
        IHashAlgorithm HashAlgorithm { get; }
        long FileSize { get; }
        void Apply(
            Action<byte[], int> writeData,
            Action<long, long, byte[]> copy
            );
    }

    public class BinaryDeltaReader : IDeltaReader
    {
        private const int BufferSize = 1 * 1024 * 1024;

        private readonly BinaryReader reader;
        private readonly IProgressReporter progressReporter;
        private byte[] expectedHash;
        private IHashAlgorithm hashAlgorithm;
        private bool hasReadMetadata;
        public short chunkSize;
        public long fileSize;

        public BinaryDeltaReader(Stream stream, IProgressReporter progressReporter)
        {
            this.reader = new BinaryReader(stream);
            this.progressReporter = progressReporter ?? new NullProgressReporter();
        }

        public byte[] ExpectedHash
        {
            get
            {
                EnsureMetadata();
                return expectedHash;
            }
        }

        public IHashAlgorithm HashAlgorithm
        {
            get
            {
                EnsureMetadata();
                return hashAlgorithm;
            }
        }

        public long FileSize
        {
            get
            {
                EnsureMetadata();
                return fileSize;
            }
        }

        void EnsureMetadata()
        {
            if (hasReadMetadata)
                return;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var first = reader.ReadBytes(BinaryFormat.DeltaHeader.Length);
            if (!Enumerable.SequenceEqual(first, BinaryFormat.DeltaHeader))
                throw new CorruptFileFormatException("The delta file appears to be corrupt.");

            var version = reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException("The delta file uses a newer file format than this program can handle.");

            chunkSize = reader.ReadInt16();
            var hashAlgorithmName = reader.ReadString();
            hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            var hashLength = reader.ReadInt32();
            expectedHash = reader.ReadBytes(hashLength);

            fileSize = reader.ReadInt64();

            var endOfMeta = reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!Enumerable.SequenceEqual(BinaryFormat.EndOfMetadata, endOfMeta))
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            hasReadMetadata = true;
        }

        public void Apply(
            Action<byte[], int> writeData,
            Action<long, long, byte[]> copy)
        {
            var fileLength = reader.BaseStream.Length;

            EnsureMetadata();

            byte[] buffer = new byte[BufferSize];

            while (reader.BaseStream.Position != fileLength)
            {
                var b = reader.ReadByte();

                progressReporter.ReportProgress("Applying delta", reader.BaseStream.Position, fileLength);

                if (b == BinaryFormat.CopyCommand)
                {
                    var start = reader.ReadInt64();
                    var length = reader.ReadInt64();
                    copy(start, length, buffer);
                }
                else if (b == BinaryFormat.DataCommand)
                {
                    var length = reader.ReadInt64();
                    long soFar = 0;
                    while (soFar < length)
                    {
                        int len = (int)Math.Min(length - soFar, buffer.Length);
                        reader.BaseStream.Read(buffer, 0, len);
                        soFar += len;
                        writeData(buffer, len);
                    }
                }
            }
            progressReporter.ReportProgress("Applying delta", reader.BaseStream.Position, fileLength);
        }
    }
}