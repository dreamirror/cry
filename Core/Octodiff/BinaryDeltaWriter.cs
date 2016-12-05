using System;
using System.IO;

namespace Octodiff.Core
{
    public interface IDeltaWriter
    {
        long LastOffset { get; }
        void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash, long filesize, short chunkSize);
        void WriteCopyCommand(DataRange segment);
        void WriteDataCommand(Stream source, long offset, long length);
        void Finish();
    }

    // This decorator turns any sequential copy operations into a single operation, reducing 
    // the size of the delta file.
    // For example:
    //   Copy: 0x0000 - 0x0400
    //   Copy: 0x0401 - 0x0800
    //   Copy: 0x0801 - 0x0C00
    // Gets turned into:
    //   Copy: 0x0000 - 0x0C00
    public class AggregateCopyOperationsDecorator : IDeltaWriter
    {
        private readonly IDeltaWriter decorated;
        private DataRange bufferedCopy;

        public long LastOffset { get { return bufferedCopy.StartOffset + bufferedCopy.Length; } }

        public AggregateCopyOperationsDecorator(IDeltaWriter decorated)
        {
            this.decorated = decorated;
        }

        public void WriteDataCommand(Stream source, long offset, long length)
        {
            FlushCurrentCopyCommand();
            decorated.WriteDataCommand(source, offset, length);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash, long filesize, short chunkSize)
        {
            decorated.WriteMetadata(hashAlgorithm, expectedNewFileHash, filesize, chunkSize);
        }

        public void WriteCopyCommand(DataRange chunk)
        {
            if (bufferedCopy.Length > 0 && bufferedCopy.StartOffset + bufferedCopy.Length == chunk.StartOffset)
            {
                bufferedCopy.Length += chunk.Length;
            }
            else
            {
                FlushCurrentCopyCommand();
                bufferedCopy = chunk;
            }
        }

        void FlushCurrentCopyCommand()
        {
            if (bufferedCopy.Length <= 0) return;

            decorated.WriteCopyCommand(bufferedCopy);
            bufferedCopy = new DataRange();
        }

        public void Finish()
        {
            FlushCurrentCopyCommand();
            decorated.Finish();
        }
    }

    public class BinaryDeltaWriter : IDeltaWriter
    {
        public long LastOffset { get { return 0; } }

        private readonly BinaryWriter writer;
        byte[] buffer = new byte[1024 * 1024];

        public BinaryDeltaWriter(Stream stream)
        {
            writer = new BinaryWriter(stream);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash, long filesize, short chunkSize)
        {
            writer.Write(BinaryFormat.DeltaHeader);
            writer.Write(BinaryFormat.Version);
            writer.Write(chunkSize);
            writer.Write(hashAlgorithm.Name);
            writer.Write(expectedNewFileHash.Length);
            writer.Write(expectedNewFileHash);
            writer.Write(filesize);
            writer.Write(BinaryFormat.EndOfMetadata);
        }

        public void WriteCopyCommand(DataRange segment)
        {
            writer.Write(BinaryFormat.CopyCommand);
            writer.Write(segment.StartOffset);
            writer.Write(segment.Length);
        }

        public void WriteDataCommand(Stream source, long offset, long length)
        {
            writer.Write(BinaryFormat.DataCommand);
            writer.Write(length);


            var originalPosition = source.Position;
            try
            {
                source.Seek(offset, SeekOrigin.Begin);

                int read;
                long soFar = 0;
                while ((read = source.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                {
                    soFar += read;

                    writer.Write(buffer, 0, read);
                }
            }
            finally
            {
                source.Seek(originalPosition, SeekOrigin.Begin);
            }
        }

        public void Finish()
        {
        }
    }
}