using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;


namespace TS_Server.DataTools
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TalkInfo
    {
        public ushort id;
        public byte length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 254)]
        public byte[] dialog;
    }

    public static class TalkData
    {
        public static Dictionary<ushort,TalkInfo> talkList = new Dictionary<ushort,TalkInfo>();
        public static int FIELD_LENGTH = 257;

        // Reads directly from stream to structure
        public static T ReadFromTalks<T>(Stream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }

        public static void DecodeItem32(ref UInt32 val)
        {
            val = Convert.ToUInt32((val ^ 0x0BAEB716) - 1);
        }

        public static void DecodeItem32s(ref int val)
        {
            val = Convert.ToInt32((val ^ 0x0BAEB716) - 1);
        }

        public static void DecodeItem16(ref UInt16 val)
        {
            val = Convert.ToUInt16((val ^ 0xECEA) - 5);
        }

        public static void DecodeItem8(ref byte val)
        {
            if (val != 0xC8)
                val = Convert.ToByte((val ^ 0xC8) - 1);
            else
                val = 0xFF;
        }

        public static bool LoadTalks()
        {
            try
            {
                using (FileStream fs = new FileStream("Talk.Dat", FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Seek(FIELD_LENGTH, 0);

                    while (FileLen > FIELD_LENGTH)
                    {
                        TalkInfo NewTalk = ReadFromTalks<TalkInfo>(fs);
                        DecodeItem16(ref NewTalk.id);

                        for (int i = 0; i < 127; i++)
                        {
                            byte tmp = NewTalk.dialog[253 - i];
                            NewTalk.dialog[253 - i] = NewTalk.dialog[i];
                            NewTalk.dialog[i] = tmp;
                        }

                        talkList.Add(NewTalk.id,NewTalk);

                        FileLen -= Marshal.SizeOf(typeof(TalkInfo));
                    }

                    fs.Close();
                    fs.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static void writeToFile(string output)
        {
            FileStream fs = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter s = new StreamWriter(fs);

            s.Write("id \t");
            s.Write("dialog \n");
            s.Write("\n");

            foreach (TalkInfo i in talkList.Values)
            {
                s.Write(i.id + "\t");
                s.Write(TextEncoder.convertToUniCode(i.dialog, 0, i.length));
                s.Write("\n");
            }

            s.Close();
            fs.Close();

        }

    }
}
