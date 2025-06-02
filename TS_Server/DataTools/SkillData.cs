using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace TS_Server.DataTools
{
    //public struct skillInfo
    //{
    //    //public ushort id;
    //    //public byte sp_used;
    //    //public byte nb_target;
    //    //public byte nb_hit;
    //    //public byte special_effect;
    //    //public byte delay;
    //}

    //public static class SkillData
    //{
    //    public static Dictionary<ushort, skillInfo> skillList = new Dictionary<ushort, skillInfo>();
    //}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SkillInfo
    {
        public byte namelength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        public byte type;
        public UInt16 id;
        public ushort sp_cost;
        public byte elem;
        public uint unk3;
        public byte unk4;
        public byte grade;
        public byte skill_type;
        public byte nb_target;
        public byte unk8;
        public byte delay;
        public byte state;
        public byte unk11;
        public byte unk12;
        public byte sk_point;
        public byte unk14;
        public byte max_lvl;
        public ushort require_sk;
        public ushort unk17;
        public byte unk18;
        public ushort unk19;
        public byte unk20;
        public ushort unk21;
        public ushort unk22;
        public byte des_length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public byte[] des;
    }

    public static class SkillData
    {
        public static Dictionary<ushort, SkillInfo> skillList = new Dictionary<ushort, SkillInfo>();
        public static List<SkillInfo> m_SkillList = new List<SkillInfo>();
        public static int FIELD_LENGTH = 86;

        // Reads directly from stream to structure
        public static T ReadFromSkills<T>(Stream fs)
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
            val = Convert.ToUInt32((val ^ 0x0BAEB716) - 2);
        }

        public static void DecodeItem32s(ref int val)
        {
            val = Convert.ToInt32((val ^ 0x0BAEB716) - 1);
        }

        public static void DecodeItem16(ref UInt16 val)
        {
            ushort op = 0x6ea0;
                val = Convert.ToUInt16((val ^ op) - 4);
        }

        public static void DecodeItem8(ref byte val)
        {
                val = Convert.ToByte((val ^ 0xfd) - 4);
        }

        public static bool GetSkillByID(UInt16 id, ref SkillInfo SkillPtr)
        {
            foreach (SkillInfo Inf in m_SkillList)
            {
                if (Inf.id == id)
                {
                    SkillPtr = Inf;
                    return true;
                }
            }

            return false;
        }

        public static bool LoadSkills()
        {
            //Console.WriteLine(Marshal.SizeOf(typeof(SkillInfo)));
            try
            {
                using (FileStream fs = new FileStream("skill.dat", FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Seek(FIELD_LENGTH, 0);

                    while (FileLen > FIELD_LENGTH)
                    {
                        SkillInfo NewSkill = ReadFromSkills<SkillInfo>(fs);

                        DecodeItem8(ref NewSkill.type);
                        DecodeItem16(ref NewSkill.id);                        
                        DecodeItem16(ref NewSkill.sp_cost);
                        DecodeItem8(ref NewSkill.elem);
                        DecodeItem32(ref NewSkill.unk3);
                        DecodeItem8(ref NewSkill.unk4);
                        DecodeItem8(ref NewSkill.grade);
                        DecodeItem8(ref NewSkill.skill_type);
                        DecodeItem8(ref NewSkill.nb_target);
                        DecodeItem8(ref NewSkill.unk8);
                        DecodeItem8(ref NewSkill.delay);
                        DecodeItem8(ref NewSkill.state);
                        DecodeItem8(ref NewSkill.unk11);
                        DecodeItem8(ref NewSkill.unk12);
                        DecodeItem8(ref NewSkill.sk_point);
                        DecodeItem8(ref NewSkill.unk14);
                        DecodeItem8(ref NewSkill.max_lvl);
                        DecodeItem16(ref NewSkill.require_sk);
                        DecodeItem16(ref NewSkill.unk17);
                        DecodeItem8(ref NewSkill.unk18);
                        DecodeItem16(ref NewSkill.unk19);
                        DecodeItem8(ref NewSkill.unk20);
                        DecodeItem16(ref NewSkill.unk21);
                        DecodeItem16(ref NewSkill.unk22);

                        for (int i = 0; i < 10; i++)
                        {
                            byte tmp = NewSkill.name[19 - i];
                            NewSkill.name[19 - i] = NewSkill.name[i];
                            NewSkill.name[i] = tmp;
                        }

                        for (int i = 0; i < 15; i++)
                        {
                            byte tmp = NewSkill.des[29 - i];
                            NewSkill.des[29 - i] = NewSkill.des[i];
                            NewSkill.des[i] = tmp;
                        }

                        skillList.Add(NewSkill.id, NewSkill);
                        m_SkillList.Add(NewSkill);

                        FileLen -= Marshal.SizeOf(typeof(SkillInfo));
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
            s.Write("sp_cost\t");
            s.Write("elem \t");
            s.Write("unk3 \t");
            s.Write("unk4 \t");
            s.Write("grade \t");
            s.Write("skill_type \t");
            s.Write("nb_target \t");
            s.Write("unk8 \t");
            s.Write("delay \t");
            s.Write("state \t");
            s.Write("unk11 \t");
            s.Write("unk12 \t");
            s.Write("sk_point \t");
            s.Write("unk14 \t");
            s.Write("max_lvl \t");
            s.Write("require_sk \t");
            s.Write("unk17 \t");
            s.Write("unk18 \t");
            s.Write("unk19 \t");
            s.Write("unk20 \t");
            s.Write("unk21 \t");
            s.Write("unk22 \t");
            s.Write("desc");
            s.Write("\n");

            for (int i = 0; i < m_SkillList.Count; i++)
            {
                s.Write(TextEncoder.convertToUniCode(m_SkillList[i].name, 0, m_SkillList[i].namelength) + "\t");
                s.Write(m_SkillList[i].type + "\t");
                s.Write(m_SkillList[i].id + "\t");
                s.Write(m_SkillList[i].sp_cost+ "\t");
                s.Write(m_SkillList[i].elem + "\t");
                s.Write(m_SkillList[i].unk3 + "\t");
                s.Write(m_SkillList[i].unk4 + "\t");
                s.Write(m_SkillList[i].grade + "\t");
                s.Write(m_SkillList[i].skill_type + "\t");
                s.Write(m_SkillList[i].nb_target + "\t");
                s.Write(m_SkillList[i].unk8 + "\t");
                s.Write(m_SkillList[i].delay + "\t");
                s.Write(m_SkillList[i].state + "\t");
                s.Write(m_SkillList[i].unk11 + "\t");
                s.Write(m_SkillList[i].unk12 + "\t");
                s.Write(m_SkillList[i].sk_point + "\t");
                s.Write(m_SkillList[i].unk14 + "\t");
                s.Write(m_SkillList[i].max_lvl + "\t");
                s.Write(m_SkillList[i].require_sk + "\t");
                s.Write(m_SkillList[i].unk17 + "\t");
                s.Write(m_SkillList[i].unk18 + "\t");
                s.Write(m_SkillList[i].unk19 + "\t");
                s.Write(m_SkillList[i].unk20 + "\t");
                s.Write(m_SkillList[i].unk21 + "\t");
                s.Write(m_SkillList[i].unk22 + "\t");
                s.Write(TextEncoder.convertToUniCode(m_SkillList[i].des, 0, m_SkillList[i].des_length));
                s.Write("\n");

            }

            s.Close();
            fs.Close();

        }

    }

}
