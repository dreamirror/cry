using System;
namespace MNS
{
    public class Random
    {
        long value;
        public long Value { get { return value; } }
		public long Seed { set { this.value = value; } }

        public Random()
        {
            value = System.DateTime.Now.ToBinary();
        }

        public Random(long seed)
        {
            this.value = seed;
        }

        public static long NextValue(long value)
        {
            return Math.Abs(Math.Abs(1103515245110351524 * value) + 54321)/10;
        }

        public long _Next()
        {
            value = NextValue(value);
            return value;
        }

        public int Next()
        {
            long result = _Next() % ((long)int.MaxValue + 1);
            return (int)result;
        }

        public int NextRange(int min, int max)
        {
            return (int)_NextRange(min, max);
        }

        public long _NextRange(long min, long max)
        {
            if (min == max)
                return min;

            long result = _Next() % (max - min + 1);
            System.Diagnostics.Debug.Assert(result + min <= max);

            return result + min;
        }

        public double NextDouble()
        {
            return _NextRange(0, int.MaxValue) / (double)int.MaxValue;
        }

        public float NextFloat()
        {
            return _NextRange(0, int.MaxValue) / (float)int.MaxValue;
        }

        public float NextRange(float min, float max)
        {
            if (min == max)
                return min;

            return (_NextRange(0, int.MaxValue) / (float)int.MaxValue)*(max-min)+min;
        }

        static public Random Instance = new Random();
    }
}