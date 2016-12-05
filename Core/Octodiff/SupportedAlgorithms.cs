using System.IO;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public interface IHashAlgorithm
    {
        string Name { get; }
        int HashLength { get; }
        byte[] ComputeHash(Stream stream);
        byte[] ComputeHash(byte[] buffer, int offset, int length);
        byte[] ComputeHash(Stream stream, Diagnostics.IProgressReporter progressReporter);
    }

    public class HashAlgorithmWrapper : IHashAlgorithm
    {
        private readonly HashAlgorithm algorithm;

        public HashAlgorithmWrapper(string name, HashAlgorithm algorithm)
        {
            Name = name;
            this.algorithm = algorithm;
        }

        public string Name { get; private set; }
        public int HashLength { get { return algorithm.HashSize / 8; } }

        public byte[] ComputeHash(Stream stream)
        {
            return algorithm.ComputeHash(stream);
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int length)
        {
            return algorithm.ComputeHash(buffer, offset, length);
        }

        public byte[] ComputeHash(Stream stream, Diagnostics.IProgressReporter progressReporter)
        {
            byte[] input_buffer = new byte[1024 * 1024];
            byte[] output_buffer = new byte[1024 * 1024];

            progressReporter.ReportProgress("Compute Hash", stream.Position, stream.Length);

            while (true)
            {
                int read = stream.Read(input_buffer, 0, input_buffer.Length);
                progressReporter.ReportProgress("Compute Hash", stream.Position, stream.Length);

                if (read < input_buffer.Length)
                {
                    algorithm.TransformFinalBlock(input_buffer, 0, read);
                    break;
                }
                else
                    algorithm.TransformBlock(input_buffer, 0, input_buffer.Length, output_buffer, 0);
            }
            return algorithm.Hash;
        }

    }

    public static class SupportedAlgorithms
    {
        public static class Hashing
        {
            public static IHashAlgorithm Sha1()
            {
                return new HashAlgorithmWrapper("SHA1", SHA1CryptoServiceProvider.Create());
            }

            public static IHashAlgorithm MD5()
            {
                return new HashAlgorithmWrapper("MD5", MD5CryptoServiceProvider.Create());
            }

            public static IHashAlgorithm Default()
            {
                return Sha1();
            }

            public static IHashAlgorithm Create(string algorithm)
            {
                if (algorithm == "SHA1")
                    return Sha1();

                if (algorithm == "MD5")
                    return MD5();

                throw new CompatibilityException(string.Format("The hash algorithm '{0}' is not supported in this version of Octodiff", algorithm));
            }
        }

        public static class Checksum
        {
            public static IRollingChecksum Adler32Rolling() { return new Adler32RollingChecksum();  }

            public static IRollingChecksum Default()
            {
                return Adler32Rolling();
            }

            public static IRollingChecksum Create(string algorithm)
            {
                if (algorithm == "Adler32")
                    return Adler32Rolling();
                throw new CompatibilityException(string.Format("The rolling checksum algorithm '{0}' is not supported in this version of Octodiff", algorithm));
            }
        }
    }
}