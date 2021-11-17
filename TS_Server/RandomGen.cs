using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Server
{
    public static class RandomGen
    {
        public static Random rand = new Random();

        public static byte getByte(byte min, byte max)
        {
            return (byte)rand.Next(min, max);
        }
        public static ushort getUShort(ushort min, ushort max)
        {
            return (ushort)rand.Next(min, max);
        }
        public static int getInt(int min, int max)
        {
            return rand.Next(min, max);
        }
    }
}
