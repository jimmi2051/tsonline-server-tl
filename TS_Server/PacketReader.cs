using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Server
{
    class PacketReader
    {
        public static ushort read16(byte[] data, int off)
        {
            return (ushort)(data[off] + (data[off + 1] << 8));
        }

        public static uint read32(byte[] data, int off)
        {
            return (uint)(data[off] + (data[off + 1] << 8) + (data[off + 2] << 16) + (data[off + 3] << 24));
        }
        public static string readString(byte[] data, int off, int len)
        {
            string ret = "";
            for (int i = 0; i < len; i++)
                ret += (char)(data[off + i]);
            return ret;
        }

        public static byte[] toByteArray(byte[] data, int pos, int length)
        {
            byte[] data2 = new byte[length];
            Array.Copy(data, pos, data2, 0, length);
            return data2;
        }

        //public static string toString(byte[] data, int pos, int length)
        //{
        //    var c = new TSMysqlConnection(); // just for get encoding charset maybe move to another configuration class
        //    byte[] data2 = new byte[length];
        //    Array.Copy(data, pos, data2, 0, length);
        //    //read String in client encoding
        //    return Encoding.GetEncoding(c.clientencode).GetString(data2);
        //}
    }
}
