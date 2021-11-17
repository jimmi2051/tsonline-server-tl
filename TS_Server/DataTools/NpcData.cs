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
    public struct NpcInfo
    {
        public byte namelength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public byte[] name;
        public byte type;
        public UInt16 id;
        public ushort image1;
        public ushort image2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] color;
        public byte notPet;
        public byte unk5;
        public byte unk6;
        public byte level;
        public uint hpmax;
        public uint spmax;
        public ushort hpx;
        public ushort spx;
        public ushort mag;
        public ushort atk;
        public ushort def;
        public ushort agi;
        public byte doanhtrai; //1 nguy, 2 thuc, 3 ngo, 4 kvang, 5 du hiep, 0 none
        public ushort exp_bonus;
        public byte element;
        public ushort skill1;
        public ushort skill2;
        public ushort skill3;
        public ushort drop1;
        public ushort drop2;
        public ushort drop3;
        public ushort drop4;
        public ushort drop5;
        public ushort drop6;
        public byte unk1;
        public ushort skill4;
        public byte reborn;
        public ushort unk2;
        public ushort unk3;
    }

    public static class NpcData
    {
        public static Dictionary<ushort, NpcInfo> npcList = new Dictionary<ushort, NpcInfo>();
        public static Dictionary<ushort, List<ushort>> drop= new Dictionary<ushort, List<ushort>>();
        public static Dictionary<ushort, List<ushort>> rareDrop = new Dictionary<ushort, List<ushort>>();
        public static int FIELD_LENGTH = 92;        

        // Reads directly from stream to structure
        public static T ReadFromNpcs<T>(Stream fs)
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
            val = Convert.ToUInt16((val ^ 0x5209) - 1);
        }

        public static void DecodeItem8(ref byte val)
        {
            if (val != 0xC8)
                val = Convert.ToByte((val ^ 0xC8) - 1);
            else
                val = 0xFF;            
        }

        public static bool LoadNpcs()
        {
            try
            {
                using (FileStream fs = new FileStream("npc.dat", FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Seek(FIELD_LENGTH, 0);
 
                    while (FileLen > FIELD_LENGTH)
                    {
                        NpcInfo NewNpc = ReadFromNpcs<NpcInfo>(fs);
                        
                        DecodeItem8(ref NewNpc.type);
                        DecodeItem16(ref NewNpc.id);
                        DecodeItem16(ref NewNpc.image1);
                        DecodeItem16(ref NewNpc.image2);
                        DecodeItem32(ref NewNpc.color[0]);
                        DecodeItem32(ref NewNpc.color[1]);
                        DecodeItem32(ref NewNpc.color[2]);
                        DecodeItem32(ref NewNpc.color[3]);
                        DecodeItem8(ref NewNpc.notPet);
                        DecodeItem8(ref NewNpc.unk5);
                        DecodeItem8(ref NewNpc.unk6);
                        DecodeItem8(ref NewNpc.level);
                        DecodeItem32(ref NewNpc.hpmax);
                        DecodeItem32(ref NewNpc.spmax);
                        DecodeItem16(ref NewNpc.hpx);
                        DecodeItem16(ref NewNpc.spx);
                        DecodeItem16(ref NewNpc.mag);
                        DecodeItem16(ref NewNpc.atk);
                        DecodeItem16(ref NewNpc.def);
                        DecodeItem16(ref NewNpc.agi);
                        DecodeItem8(ref NewNpc.doanhtrai);
                        DecodeItem16(ref NewNpc.exp_bonus);
                        DecodeItem8(ref NewNpc.element);
                        DecodeItem16(ref NewNpc.skill1);
                        DecodeItem16(ref NewNpc.skill2);
                        DecodeItem16(ref NewNpc.skill3);
                        DecodeItem16(ref NewNpc.drop1);
                        DecodeItem16(ref NewNpc.drop2);
                        DecodeItem16(ref NewNpc.drop3);
                        DecodeItem16(ref NewNpc.drop4);
                        DecodeItem16(ref NewNpc.drop5);
                        DecodeItem16(ref NewNpc.drop6);
                        DecodeItem8(ref NewNpc.unk1);
                        DecodeItem16(ref NewNpc.skill4);
                        DecodeItem8(ref NewNpc.reborn);
                        DecodeItem16(ref NewNpc.unk2);
                        DecodeItem16(ref NewNpc.unk3);

                        for (int i = 0; i < 7; i++)
                        {
                            byte tmp = NewNpc.name[13 - i];
                            NewNpc.name[13 - i] = NewNpc.name[i];
                            NewNpc.name[i] = tmp;
                        }

                        npcList.Add(NewNpc.id,NewNpc);

                        drop.Add(NewNpc.id,new List<ushort>());
                        if (NewNpc.drop1 != 0) drop[NewNpc.id].Add(NewNpc.drop1);
                        if (NewNpc.drop2 != 0) drop[NewNpc.id].Add(NewNpc.drop2);
                        if (NewNpc.drop3 != 0) drop[NewNpc.id].Add(NewNpc.drop3);

                        rareDrop.Add(NewNpc.id, new List<ushort>());
                        if (NewNpc.drop4 != 0) rareDrop[NewNpc.id].Add(NewNpc.drop4);
                        if (NewNpc.drop5 != 0) rareDrop[NewNpc.id].Add(NewNpc.drop5);
                        if (NewNpc.drop6 != 0) rareDrop[NewNpc.id].Add(NewNpc.drop6);

                        FileLen -= Marshal.SizeOf(typeof(NpcInfo));
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

            s.Write("name \t");
            s.Write("type \t");
            s.Write("id \t");
            s.Write("image1 \t");
            s.Write("image2 \t");
            s.Write("color1 \t");
            s.Write("color2 \t");
            s.Write("color3 \t");
            s.Write("color4 \t");
            s.Write("notPet \t");
            s.Write("unk5 \t");
            s.Write("unk6 \t");
            s.Write("lvl \t");
            s.Write("hpmax \t");
            s.Write("spmax \t");
            s.Write("hpx \t");
            s.Write("spx \t");
            s.Write("int \t");
            s.Write("atk \t");
            s.Write("def \t");
            s.Write("agi \t");
            s.Write("doanhtrai \t");
            s.Write("exp_bonus \t");
            s.Write("element \t");
            s.Write("skill1 \t");
            s.Write("skill2 \t");
            s.Write("skill3 \t");
            s.Write("drop \t");
            s.Write("drop \t");
            s.Write("drop \t");
            s.Write("drop \t");
            s.Write("drop \t");
            s.Write("drop \t");
            s.Write("unk1 \t");
            s.Write("skill4 \t");
            s.Write("reborn \t");
            s.Write("unk2 \t");
            s.Write("unk3");
            s.Write("\n");            

            foreach (NpcInfo i in npcList.Values)
            {
                s.Write(TextEncoder.convertToUniCode(i.name, 0, i.namelength) + "\t");
                s.Write(i.type + "\t");
                s.Write(i.id + "\t");
                s.Write(i.image1 + "\t");
                s.Write(i.image2 + "\t");
                s.Write(i.color[0] + "\t");
                s.Write(i.color[1] + "\t");
                s.Write(i.color[2] + "\t");
                s.Write(i.color[3] + "\t");
                s.Write(i.notPet + "\t");
                s.Write(i.unk5 + "\t");
                s.Write(i.unk6 + "\t");
                s.Write(i.level + "\t");
                s.Write(i.hpmax + "\t");
                s.Write(i.spmax + "\t");
                s.Write(i.hpx + "\t");
                s.Write(i.spx + "\t");
                s.Write(i.mag + "\t");
                s.Write(i.atk + "\t");
                s.Write(i.def + "\t");
                s.Write(i.agi + "\t");
                s.Write(i.doanhtrai + "\t");
                s.Write(i.exp_bonus + "\t");
                s.Write(i.element + "\t");
                s.Write(i.skill1 + "\t");
                s.Write(i.skill2 + "\t");
                s.Write(i.skill3 + "\t");
                s.Write(i.drop1 + "\t");
                s.Write(i.drop2 + "\t");
                s.Write(i.drop3 + "\t");
                s.Write(i.drop4 + "\t");
                s.Write(i.drop5 + "\t");
                s.Write(i.drop6 + "\t");
                s.Write(i.unk1 + "\t");
                s.Write(i.skill4 + "\t");
                s.Write(i.reborn + "\t");
                s.Write(i.unk2 + "\t");
                s.Write(i.unk3);
                s.Write("\n");

            }

            s.Close();
            fs.Close();

        }

    }
}
