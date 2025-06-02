using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Server
{
    public class PacketCreator
    {
        private long pos;
        private byte[] data;
        private static int BUFFER_LENGTH = 512;

        public PacketCreator()
        {
            data = new byte[BUFFER_LENGTH];
            pos = 0;
        }

        public PacketCreator(byte opcode)
        {
            data = new byte[BUFFER_LENGTH];
            data[0] = opcode;
            pos = 1;
        }

        public PacketCreator(byte opcode, byte sub_opcode)
        {
            data = new byte[BUFFER_LENGTH];
            data[0] = opcode;
            data[1] = sub_opcode;
            pos = 2;
        }

        public PacketCreator(byte[] args)
        {
            data = args;
            pos = args.Length;
        }

        public byte[] send()
        {
            //Console.WriteLine("Pos: " + pos);
            UInt16 len = (UInt16)(pos);
            //Console.WriteLine("Len: " + pos);
            pos = 0;

            byte[] dat = new byte[len];
            for (int i = 0; i < len; i++)
                dat[i] = data[i];
 
            //Console.WriteLine("Send " + BitConverter.ToString(dat));

            byte[] ret = new byte[len + 4];
            ret[0] = 0x59;
            ret[1] = 0xE9;
            ret[2] = (byte)(len^0xAD);
            ret[3] = (byte)(len >> 8^0xAD); 
            
            for (int i = 0; i < len; i++)
               ret[i+4] = (byte)(data[i] ^ 0xAD);


            return ret;
        }

        public void addByte(byte b)
        {
            data[pos] = b;
            pos++;
        }

        public void add8(byte b)
        {
            data[pos] = b;
            pos++;
        }

        public void add16(UInt16 b)
        {
            data[pos] = (byte)(b);
            data[pos + 1] = (byte)(b >> 8);
            pos += 2;
        }

        public void add32(UInt32 b)
        {
            data[pos] = (byte)(b);
            data[pos + 1] = (byte)(b >> 8);
            data[pos + 2] = (byte)(b >> 16);
            data[pos + 3] = (byte)(b >> 24);
            pos += 4;
        }

        public void addZero(int nb_byte)
        {
            for (int i = 0; i < nb_byte; i++)
                data[i + pos] = 0;
            pos += nb_byte;
        }

        public void addString(String s)
        {
            byte[] str = System.Text.Encoding.Default.GetBytes(s);
            str.CopyTo(data, pos);
            pos += str.Length;
        }

        public void addBytes(byte[] a)
        {
            a.CopyTo(data, pos);
            pos += a.Length;
        }

        public void modifyBytes(byte[] a, int index) //dont use this unless really necessary, and be careful
        {
            for (int i = index; i < index + a.Length; i++)
                data[i] = a[i - index];
        }

        public byte[] getData()
        {
            byte[] ret = new byte[pos];
            for (int i = 0; i < pos; i++)
                ret[i] = data[i];
            return ret;
        }
        public int[] getDataInt()
        {
            int[] ret = new int[pos];

            for (int i = 0; i < pos; i++)
                ret[i] = BitConverter.ToInt32(data,i);
            return ret;
        }
    }
}
