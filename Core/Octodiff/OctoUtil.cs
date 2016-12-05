using System;
using System.Text;

namespace Octodiff.Diagnostics
{
    public interface IProgressReporter
    {
        void ReportProgress(string operation, long currentPosition, long total);
    }

    public class NullProgressReporter : IProgressReporter
    {
        public void ReportProgress(string operation, long currentPosition, long total)
        {
        }
    }
}

namespace Octodiff.Core
{
    class BinaryFormat
    {
        public static readonly byte[] SignatureHeader = Encoding.ASCII.GetBytes("OCTOSIG");
        public static readonly byte[] DeltaHeader = Encoding.ASCII.GetBytes("OCTODELTA");
        public static readonly byte[] EndOfMetadata = Encoding.ASCII.GetBytes(">>>");
        public const byte CopyCommand = 0x60;
        public const byte DataCommand = 0x80;

        public const byte Version = 0x01;
    }

    public struct DataRange
    {
        public DataRange(long startOffset, long length)
        {
            StartOffset = startOffset;
            Length = length;
        }

        public long StartOffset;
        public long Length;
    }

    public class CompatibilityException : Exception
    {
        public CompatibilityException(string message) : base(message)
        {

        }
    }

    public class CorruptFileFormatException : Exception
    {
        public CorruptFileFormatException(string message) : base(message)
        {
        }
    }

    public class UsageException : Exception
    {
        public UsageException(string message) : base(message)
        {

        }
    }
}