using System;

namespace RockSatC_2016.Utility
{
    internal static class Tools
    {
        public static long Bcd2Bin(byte[] bcd)
        {
            long result = 0;

            foreach (var b in bcd)
            {
                int digit1 = b >> 4;
                int digit2 = b & 0x0f;
                result = (result*100) + digit1*10 + digit2;
            }
            return result;
        }

        public static float map(float original, float fromLo, float fromHi, float toLow, float toHigh)
        {
            return (original - fromLo)*((toHigh - toLow)/(fromHi - fromLo) + fromLo);
        }
        public static byte[] Bin2Bcd(int value)
        {
            if (value < 0 || value > 99999999)
                throw new ArgumentOutOfRangeException(nameof(value));
            var ret = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }
            return ret;
        }
        
    }
}