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
    public struct SceneInfo
    {
        public ushort last_mapid;
        public byte last_warpid;
        public byte namelength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 55)]
        public ushort[] unk;
     }

    public static class SceneData
    {
        public static List<SceneInfo> sceneList = new List<SceneInfo>();
        public static int FIELD_LENGTH = 134;

        // Reads directly from stream to structure
        public static T ReadFromScenes<T>(Stream fs)
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
            val = Convert.ToUInt16((val ^ 0xEA6C) - 9);
        }

        public static void DecodeItem8(ref byte val)
        {
            if (val != 0xC8)
                val = Convert.ToByte((val ^ 0xC8) - 1);
            else
                val = 0xFF;
        }

        public static bool LoadScenes()
        {
            try
            {
                using (FileStream fs = new FileStream("Scene.Dat", FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Seek(FIELD_LENGTH, 0);

                    while (FileLen > FIELD_LENGTH)
                    {                        
                       SceneInfo NewScene = ReadFromScenes<SceneInfo>(fs);
                       DecodeItem16(ref NewScene.last_mapid);

                        for (int i = 0; i < 55; i++)
                            DecodeItem16(ref NewScene.unk[i]);

                        //for (int i = 0; i < 10; i++)
                        //{
                        //    byte tmp = NewScene.name[19 - i];
                        //    NewScene.name[19 - i] = NewScene.name[i];
                        //    NewScene.name[i] = tmp;
                        //}

                        sceneList.Add(NewScene);

                        FileLen -= Marshal.SizeOf(typeof(SceneInfo));
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

            s.Write("No \t");
            s.Write("last_mapid \t");
            s.Write("last_warpid \t");
            s.Write("namelength \t");
            s.Write("name \t");
            for (int i = 0; i < 55; i++)
                s.Write("unk" + (i + 1) + " \t");
            s.Write("\n");

            for (int i = 0; i < sceneList.Count; i++)
            {
                s.Write((i + 1) + "\t");
                s.Write(sceneList[i].last_mapid + "\t");
                s.Write(sceneList[i].last_warpid + "\t");
                s.Write(sceneList[i].namelength + "\t");
                s.Write(TextEncoder.convertToUniCode(sceneList[i].name, 0, sceneList[i].namelength) + "\t");
                for (int j = 0; j < 55; j++)
                    s.Write(sceneList[i].unk[j] + "\t");
                s.Write("\n");
            }

            s.Close();
            fs.Close();

        }

    }
}
