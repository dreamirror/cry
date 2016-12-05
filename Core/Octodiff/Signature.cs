using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public class ChunkSignature
    {
        public long StartOffset;            // 8 (but not included in the file on disk)
        public byte[] Hash;                 // 20
        public UInt32 RollingChecksum;      // 4
                                            // 24 bytes on disk
                                            // 32 bytes in memory

        public override string ToString()
        {
            return string.Format("{0,6} |{1,20}| {2}", StartOffset, RollingChecksum, BitConverter.ToString(Hash).ToLowerInvariant().Replace("-", ""));
        }
    }

    public class ChunkHash
    {
        public byte[] Hash { get; private set; }
        public List<long> Offsets = new List<long>();

        public ChunkHash(byte[] hash, long offset)
        {
            Hash = hash;
            Offsets.Add(offset);
        }

        public void AddOffset(long offset)
        {
            Offsets.Add(offset);
        }

        public long FindBestOffset(long offset)
        {
            long find_offset = Offsets.Find(o => o == offset);
            if (find_offset == 0)
                find_offset = Offsets[0];
            return find_offset;
        }
    }

    public class ChunkHashList
    {
        List<ChunkHash> hashes = new List<ChunkHash>();
        public ChunkHashList(byte[] hash, long offset)
        {
            Add(hash, offset);
        }

        public void Add(byte[] hash, long offset)
        {
            ChunkHash chunk = Find(hash);
            if (chunk == null)
                hashes.Add(new ChunkHash(hash, offset));
            else
                chunk.AddOffset(offset);
        }

        public ChunkHash Find(byte[] hash)
        {
            return hashes.Find(h => Enumerable.SequenceEqual(h.Hash, hash));
        }
    }

    public class Signature
    {
        public Signature(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm, short chunkSize)
        {
            HashAlgorithm = hashAlgorithm;
            RollingChecksumAlgorithm = rollingChecksumAlgorithm;
            Chunks = new List<ChunkSignature>();
            ChunkSize = chunkSize;
        }

        public IHashAlgorithm HashAlgorithm { get; private set; }
        public IRollingChecksum RollingChecksumAlgorithm { get; private set; }
        public List<ChunkSignature> Chunks { get; private set; } 
        public short ChunkSize { get; private set; }

        public Dictionary<uint, ChunkHashList> ChunkMap;

        public void AddChunkMap(long offset, uint checksum, byte[] hash)
        {
            if (ChunkMap == null)
                ChunkMap = new Dictionary<uint, ChunkHashList>();

            ChunkHashList list;
            if (ChunkMap.TryGetValue(checksum, out list) == false)
                ChunkMap.Add(checksum, new ChunkHashList(hash, offset));
            else
                list.Add(hash, offset);
        }

        public ChunkHashList FindChecksum(uint checksum)
        {
            ChunkHashList list = null;
            ChunkMap.TryGetValue(checksum, out list);
            return list;
        }
    }
}