using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public class RingBuffer
    {
        Stream stream;
        int position = 0;
        byte[] buffer, return_buffer;

        public bool IsEnd { get; private set; }
        public byte Peek() { return buffer[position]; }

        public RingBuffer(Stream stream, int buffer_size)
        {
            this.stream = stream;
            buffer = new byte[buffer_size];
            return_buffer = new byte[buffer_size];
            ReadNew();
        }

        public void ReadNew()
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read < buffer.Length)
            {
                IsEnd = true;
                return;
            }
            position = 0;
        }

        public byte ReadContinue()
        {
            int read = stream.ReadByte();
            if (read == -1)
            {
                IsEnd = true;
                return 0;
            }
            buffer[position] = (byte)read;
            position = (position + 1) % buffer.Length;
            return (byte)read;
        }

        public byte[] GetBuffer()
        {
            Array.Copy(buffer, position, return_buffer, 0, buffer.Length - position);
            Array.Copy(buffer, 0, return_buffer, buffer.Length - position, position);

            return return_buffer;
        }
    }

    public class DeltaBuilder
    {
        public DeltaBuilder()
        {
            ProgressReporter = new NullProgressReporter();
        }

        public IProgressReporter ProgressReporter { get; set; }

        public void BuildDelta(IHashAlgorithm hashAlgorithm, Stream newFileStream, ISignatureReader signatureReader, IDeltaWriter deltaWriter)
        {
            var signature = signatureReader.ReadSignature();

            var hash = hashAlgorithm.ComputeHash(newFileStream);
            newFileStream.Seek(0, SeekOrigin.Begin);

            deltaWriter.WriteMetadata(hashAlgorithm, hash, newFileStream.Length, signature.ChunkSize);

            var fileSize = newFileStream.Length;
            ProgressReporter.ReportProgress("Building delta", 0, fileSize);

            var chunkSize = signature.ChunkSize;

            var missingPosition = newFileStream.Position;
            RingBuffer buffer = new RingBuffer(newFileStream, chunkSize);
            var checksumAlgorithm = signature.RollingChecksumAlgorithm;
            uint checksum = checksumAlgorithm.Calculate(buffer.GetBuffer(), 0, chunkSize);

            while (buffer.IsEnd == false)
            {
                ProgressReporter.ReportProgress("Building delta", newFileStream.Position, fileSize);

                var list = signature.FindChecksum(checksum);

                if (list != null)
                {
                    var sha = signature.HashAlgorithm.ComputeHash(buffer.GetBuffer(), 0, chunkSize);
                    var chunkHash = list.Find(sha);
                    if (chunkHash != null)
                    {
                        long offset = chunkHash.FindBestOffset(deltaWriter.LastOffset);

                        if (missingPosition < newFileStream.Position-chunkSize)
                            deltaWriter.WriteDataCommand(newFileStream, missingPosition, newFileStream.Position - chunkSize - missingPosition);

                        deltaWriter.WriteCopyCommand(new DataRange(offset, chunkSize));
                        missingPosition = newFileStream.Position;

                        buffer.ReadNew();
                        checksum = checksumAlgorithm.Calculate(buffer.GetBuffer(), 0, chunkSize);

                        continue;
                    }
                }

                var remove = buffer.Peek();
                var add = buffer.ReadContinue();
                checksum = checksumAlgorithm.Rotate(checksum, remove, add, chunkSize);
            }

            if (newFileStream.Length != missingPosition)
            {
                deltaWriter.WriteDataCommand(newFileStream, missingPosition, newFileStream.Length - missingPosition);
            }
            ProgressReporter.ReportProgress("Building delta", newFileStream.Position, fileSize);

            deltaWriter.Finish();
        }
    }
}