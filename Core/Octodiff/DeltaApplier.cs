using System;
using System.Collections;
using System.IO;
using Octodiff.Diagnostics;
using System.Linq;

namespace Octodiff.Core
{
    public class DeltaApplier
    {
        public DeltaApplier()
        {
            SkipHashCheck = false;
        }

        public bool SkipHashCheck { get; set; }

        public void Apply(Stream basisFileStream, IDeltaReader delta, Stream outputStream, IProgressReporter progressReporter)
        {
            delta.Apply(
                writeData: (data, len) => outputStream.Write(data, 0, len),
                copy: (startPosition, length, buffer) =>
                {
                    basisFileStream.Seek(startPosition, SeekOrigin.Begin);

                    int read;
                    long soFar = 0;
                    while ((read = basisFileStream.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                    {
                        soFar += read;
                        outputStream.Write(buffer, 0, read);
                    }
                });

            if (delta.FileSize != outputStream.Length)
                throw new UsageException("Verification of the patched file failed. The hash of the patch result file, and the file that was used as input for the delta, do not match. This can happen if the basis file changed since the signatures were calculated.");

            if (!SkipHashCheck)
            {
                outputStream.Seek(0, SeekOrigin.Begin);

                var sourceFileHash = delta.ExpectedHash;
                var algorithm = delta.HashAlgorithm;
                var actualHash = algorithm.ComputeHash(outputStream, progressReporter);
//                var actualHash2 = algorithm.ComputeHash(outputStream);

                if (!Enumerable.SequenceEqual(sourceFileHash, actualHash))
                    throw new UsageException("Verification of the patched file failed. The hash of the patch result file, and the file that was used as input for the delta, do not match. This can happen if the basis file changed since the signatures were calculated.");
            }
        }
    }
}