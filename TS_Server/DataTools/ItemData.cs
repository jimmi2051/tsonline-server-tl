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
    public struct ItemInfo
    {
        public byte namelength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        public byte type;
        public UInt16 id;
        public UInt16 IconNum;
        public UInt16 LargeIconNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public UInt16[] EquipImageNum;
        public UInt16 prop1;
        public UInt16 prop2;
        public byte unk1;
        public byte unk2;
        public int prop1_val;
        public int prop2_val;
        public byte contribute;
        public byte sellprice;
        public byte equippos;
        public byte unk3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public UInt32[] ColorDef;
        public byte unk4;
        public byte level;
        public UInt32 BuyingPrice;
        public UInt32 SellingPrice;
        public byte EquipLimit;
        public byte unk5;
        public UInt32 unk6;
        public byte elem_type;
        public int elem_val;
        public UInt16 unk9;
        public byte unk10;
        public UInt16 unk11;
        public byte unk12;
        public UInt16 unk13;
        public byte unk14;
        public UInt16 unk15;
        public byte unk16;
        public byte ItemDescriptionLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 254)]
        public byte[] ItemDescription; 
    }

    public static class ItemData
    {
        public static Dictionary<ushort, ItemInfo> itemList = new Dictionary<ushort, ItemInfo>();
        public static int FIELD_LENGTH = 370;

        // Reads directly from stream to structure
        public static T ReadFromItems<T>(Stream fs)
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
            val = Convert.ToUInt32((val ^ 0x0B80F4B4) - 9);
        }

        public static void DecodeItem32s(ref int val)
        {
            val = Convert.ToInt32((val ^ 0x0B80F4B4) - 109);
        }

        public static void DecodeItem16(ref UInt16 val)
        {
            val = Convert.ToUInt16((val ^ 0xEFC3) - 9);
        }

        public static void DecodeItem8(ref byte val)
        {
            val = Convert.ToByte((val ^ 0x9A) - 9);
        }

        public static bool GetItemByName(string Name, ref ItemInfo ItemPtr)
        {
            foreach (ItemInfo Inf in itemList.Values)
            {
                if (string.Compare(Name, Encoding.ASCII.GetString(Inf.name)) == 0)
                {
                    ItemPtr = Inf;
                    return true;
                }
            }

            return false;
        }

        public static bool LoadItems()
        {
            try
            {
                using (FileStream fs = new FileStream("item.dat", FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Seek(FIELD_LENGTH, 0);
 
                    while (FileLen > FIELD_LENGTH)
                    {
                        ItemInfo NewItem = ReadFromItems<ItemInfo>(fs);


                        DecodeItem8(ref NewItem.type);
                        DecodeItem16(ref NewItem.id);
                        DecodeItem16(ref NewItem.IconNum);
                        DecodeItem16(ref NewItem.LargeIconNum);
                        DecodeItem16(ref NewItem.EquipImageNum[0]);
                        DecodeItem16(ref NewItem.EquipImageNum[1]);
                        DecodeItem16(ref NewItem.prop1);
                        DecodeItem16(ref NewItem.prop2);
                        DecodeItem8(ref NewItem.unk1);
                        DecodeItem8(ref NewItem.unk2);
                        DecodeItem32s(ref NewItem.prop1_val);
                        if (NewItem.prop1 >= 65 && NewItem.prop1 <= 67) NewItem.prop1_val += 100;
                        DecodeItem32s(ref NewItem.prop2_val);
                        if (NewItem.prop2 >= 65 && NewItem.prop2 <= 67) NewItem.prop2_val += 100;
                        DecodeItem8(ref NewItem.contribute);
                        DecodeItem8(ref NewItem.sellprice);
                        DecodeItem8(ref NewItem.equippos);
                        DecodeItem8(ref NewItem.unk3);
                        DecodeItem32(ref NewItem.ColorDef[0]);
                        DecodeItem32(ref NewItem.ColorDef[1]);
                        DecodeItem32(ref NewItem.ColorDef[2]);
                        DecodeItem32(ref NewItem.ColorDef[3]);
                        DecodeItem32(ref NewItem.ColorDef[4]);
                        DecodeItem32(ref NewItem.ColorDef[5]);
                        DecodeItem32(ref NewItem.ColorDef[6]);
                        DecodeItem32(ref NewItem.ColorDef[7]);
                        DecodeItem8(ref NewItem.unk4);
                        DecodeItem8(ref NewItem.level);
                        DecodeItem32(ref NewItem.BuyingPrice);
                        DecodeItem32(ref NewItem.SellingPrice);
                        DecodeItem8(ref NewItem.EquipLimit);
                        DecodeItem8(ref NewItem.unk5);
                        DecodeItem32(ref NewItem.unk6);
                        DecodeItem8(ref NewItem.elem_type);
                        DecodeItem32s(ref NewItem.elem_val);
                        DecodeItem16(ref NewItem.unk9);
                        DecodeItem8(ref NewItem.unk10);
                        DecodeItem16(ref NewItem.unk11);
                        DecodeItem8(ref NewItem.unk12);
                        DecodeItem16(ref NewItem.unk13);
                        DecodeItem8(ref NewItem.unk14);
                        DecodeItem16(ref NewItem.unk15);
                        DecodeItem8(ref NewItem.unk16);

                        for (int i = 0; i < 10; i++)
                        {
                            byte tmp = NewItem.name[19 - i];
                            NewItem.name[19 - i] = NewItem.name[i];
                            NewItem.name[i] = tmp;
                        }

                        for (int i = 0; i < 127; i++)
                        {
                            byte tmp = NewItem.ItemDescription[253 - i];
                            NewItem.ItemDescription[253 - i] = NewItem.ItemDescription[i];
                            NewItem.ItemDescription[i] = tmp;
                        }

                        itemList.Add(NewItem.id, NewItem);

                        FileLen -= Marshal.SizeOf(typeof(ItemInfo));
                    }

                    fs.Close();
                    fs.Dispose();
                    return true;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
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
            s.Write("prop1 \t");
            s.Write("prop1_val \t");
            s.Write("prop2 \t");
            s.Write("prop2_val \t");
            s.Write("element \t");
            s.Write("elem_val \t");
            s.Write("contribute \t");
            s.Write("sellprice \t");
            s.Write("equippos \t");
            s.Write("level \t");
            s.Write("BuyingPrice \t");
            s.Write("SellingPrice \t");
            s.Write("ItemDescription \t");
            s.Write("unk1 \t");
            s.Write("unk2 \t");
            s.Write("unk3 \t");
            s.Write("unk4 \t");
            s.Write("unk5 \t");
            s.Write("unk6 \t");
            s.Write("unk9 \t");
            s.Write("unk10 \t");
            s.Write("unk11 \t");
            s.Write("unk12 \t");
            s.Write("unk13 \t");
            s.Write("unk14 \t");
            s.Write("unk15 \t");
            s.Write("unk16");
            s.Write("\n");

            foreach (ItemInfo i in itemList.Values)
            {
                s.Write(TextEncoder.convertToUniCode(i.name, 0, i.namelength) + "\t");
                s.Write(i.type + "\t");
                s.Write(i.id + "\t");
                s.Write(i.prop1 + "\t");
                s.Write(i.prop1_val + "\t");
                s.Write(i.prop2 + "\t");
                s.Write(i.prop2_val + "\t");
                s.Write(i.elem_type + "\t");
                s.Write(i.elem_val + "\t");
                s.Write(i.contribute + "\t");
                s.Write(i.sellprice + "\t");
                s.Write(i.equippos + "\t");
                s.Write(i.level + "\t");
                s.Write(i.BuyingPrice + "\t");
                s.Write(i.SellingPrice + "\t");
                s.Write(TextEncoder.convertToUniCode(i.ItemDescription, 0, i.ItemDescriptionLength) + "\t");
                s.Write(i.unk1 + "\t");
                s.Write(i.unk2 + "\t");
                s.Write(i.unk3 + "\t");
                s.Write(i.unk4 + "\t");
                s.Write(i.unk5 + "\t");
                s.Write(i.unk6 + "\t");
                s.Write(i.unk9 + "\t");
                s.Write(i.unk10 + "\t");
                s.Write(i.unk11 + "\t");
                s.Write(i.unk12 + "\t");
                s.Write(i.unk13 + "\t");
                s.Write(i.unk14 + "\t");
                s.Write(i.unk15 + "\t");
                s.Write(i.unk16);
                s.Write("\n");
            }

            s.Close();
            fs.Close();

        }
    }
}