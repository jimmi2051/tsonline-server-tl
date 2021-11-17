using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TS_Server.Client;

namespace TS_Server.DataTools
{
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EveInfo
    {
        public ushort id;
        public byte length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 254)]
        public byte[] dialog;
    }
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NpcOnMap
    {
        public ushort idOnMap;
        public ushort idNpc;
        public ushort idDialog;
        public ushort unk_1;
        public ushort unk_2;
        public ushort unk_3;
    }

    public static class EveData
    {
        public static Dictionary<ushort, EveInfo> eveList = new Dictionary<ushort, EveInfo>();
        public static Dictionary<ushort, Tuple<int, int>> offsets = new Dictionary<ushort, Tuple<int, int>>();
        public static Dictionary<ushort, NpcOnMap[]> listNpcOnMap = new Dictionary<ushort, NpcOnMap[]>();

        //public static object[] arr = new object[] { };

        // Reads directly from stream to structure
        public static T ReadFromItems<T>(Stream fs, int off)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, off, Marshal.SizeOf(typeof(T)));
            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }
        public static void showList(ushort map_id)
        {
            foreach (NpcOnMap npcOnMap in listNpcOnMap[map_id])
            {
                Console.WriteLine("Show NPCC ");
                Console.WriteLine(npcOnMap.idOnMap);
                Console.WriteLine(npcOnMap.idNpc);
            }
        }
        public static bool loadHeaders()
        {
            try
            {
                using (FileStream fs = new FileStream("eve.Emg", FileMode.Open, FileAccess.Read))
                {
                    int nb_headers = 0;
                    fs.Seek(2, 0);
                    int pos = 2;
                    int endHeader = 1000000;
                    byte[] buffer = new byte[0x20];

                    while (fs.Position < endHeader)
                    {
                        fs.Read(buffer, 0, buffer.Length);
                        pos += buffer.Length;
                        //Console.WriteLine("buffer >>" + String.Join(",", buffer));
                        ushort mapid = UInt16.Parse(Encoding.Default.GetString(buffer, 1, 5));
                        //Console.WriteLine("map id >> "+ mapid.ToString());
                        string content = Encoding.Default.GetString(buffer);
                        //Console.WriteLine("content id >> " + content);
                        int off = BitConverter.ToInt32(buffer, 0x18);
                        int len = BitConverter.ToInt32(buffer, 0x1c);
                        offsets[mapid] = new Tuple<int, int>(off, len);
                        nb_headers++;
                        if (nb_headers == 1) endHeader = off;
                    }
                    //Console.WriteLine(">>> offsets >>" + String.Join(",", offsets));
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

        public static ushort read16(byte[] data, int off)
        {
            try
            {
                return (ushort)(data[off] + (data[off + 1] << 8));
            }
            catch
            {
                return 0;
            }

        }

        public static ushort read16_reverse(byte[] data, int off)
        {
            return (ushort)(data[off + 1] + (data[off] << 8));
        }

        public static uint read32(byte[] data, int off)
        {
            return (uint)(data[off] + (data[off + 1] << 8) + (data[off + 2] << 16) + (data[off + 3] << 24));
        }

        public static Tuple<ushort, ushort> loadCoor(ushort mapid, ushort destid, ushort warpId)
        {


            if (!offsets.ContainsKey(mapid)) return null;

            byte[] data = new byte[offsets[mapid].Item2];
            try
            {
                using (FileStream fs = new FileStream("eve.Emg", FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(offsets[mapid].Item1, 0);
                    fs.Read(data, 0, data.Length);
                    fs.Close();
                    fs.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }



            int pos = 0x67;
            int nb_npc = data[pos];

            //Console.WriteLine((data[0] + (data[1] << 8)));
            //Console.WriteLine("NPC POst << " + nb_npc);

            pos += 4;
            //NPC, later
            for (int i = 0; i < nb_npc; i++)
            {
                ushort clickID = (ushort)(data[pos] + (data[pos + 1] << 8));
                //Console.WriteLine("click Id ID >>>" + clickID.ToString());
                pos += 2;
                ushort npcID = (ushort)(data[pos] + (data[pos + 1] << 8));
                //Console.WriteLine("npc ID >>>" + npcID.ToString()); 
                pos += 2;
                ushort nb1 = (ushort)(data[pos] + (data[pos + 1] << 8));
                pos += (nb1 + 2);
                byte nb2 = data[pos];
                pos += (nb2 + 1);
                byte nb_f = data[pos];
                pos += 9;
                pos += (8 * nb_f);

                pos += 31;
                int posX = BitConverter.ToInt32(data, pos);
                pos += 4;
                int posY = BitConverter.ToInt32(data, pos);
                pos += 4;
                pos += 41;
            }

            ushort nb_entry_exit = read16(data, pos); pos += 2;
            for (int i = 0; i < nb_entry_exit; i++)
            {
                pos += 3;
                uint posX = read32(data, pos);
                pos += 4;
                uint posY = read32(data, pos);
                pos += 6;
                //Console.WriteLine(posX + " " + posY); 
            }

            ushort nb_unk1 = read16(data, pos);
            pos += 2;
            for (int i = 0; i < nb_unk1; i++)
            {
                pos += 2;
                ushort nb = read16(data, pos);
                pos += 2;
                pos += nb;
                pos += 21;
                //Console.WriteLine(posX + " " + posY); 
            }

            ushort nb_unk2 = read16(data, pos);
            //Console.WriteLine(nb_unk2); 
            pos += 2;
            for (int i = 0; i < nb_unk2; i++)
            {
                pos += 2;
                ushort nb = read16(data, pos);
                pos += 2;
                pos += nb;
                pos += 17;
                //Console.WriteLine(posX + " " + posY); 
            }

            ushort nb_dialog = read16(data, pos);
            pos += 2;
            for (int i = 0; i < nb_dialog; i++)
            {
                pos += 4;
                byte nb_d = data[pos];
                pos += 4;
                pos += 5 * nb_d;
                //Console.WriteLine(nb_d); 
            }

            ushort nb_warp = read16(data, pos);
            pos += 2;
            uint X = 0, Y = 0;
            int count = 0;
            for (int i = 0; i < nb_warp; i++)
            {
                ushort warp_id = read16(data, pos);
                pos += 2;
                ushort dest_map = read16(data, pos);
                pos += 4;
                uint posX = read32(data, pos) * 20 - 10;
                pos += 4;
                uint posY = read32(data, pos) * 20 - 10;
                pos += 4;
                pos += 0x19;

                if (dest_map == destid & warpId == warp_id)
                {
                    X = posX; Y = posY;
                    count++;
                }

                //Console.WriteLine(mapid + " " + warp_id + " " + dest_map + " " + posX + " " + posY);

            }

            if (count == 1)
                return new Tuple<ushort, ushort>((ushort)X, (ushort)Y);
            return null;
        }
        public static void loadAllWarp()
        {

            foreach (KeyValuePair<ushort, Tuple<int, int>> entry in offsets)
            {
                // do something with entry.Value or entry.Key
                ushort mapid = entry.Key;
                byte[] data = new byte[offsets[mapid].Item2];
                try
                {
                    using (FileStream fs = new FileStream("eve.Emg", FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(offsets[mapid].Item1, 0);
                        fs.Read(data, 0, data.Length);
                        fs.Close();
                        fs.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
                int pos = 0x67;
                int nb_npc = data[pos];
                pos += 4;
                //NPC, later
                //Console.WriteLine("MAP >> " + mapid);
                List<NpcOnMap> list_on_map = new List<NpcOnMap>();
                for (int i = 0; i < nb_npc; i++)
                {

                    ushort clickID = (ushort)(data[pos] + (data[pos + 1] << 8));

                    pos += 2;
                    ushort npcID = (ushort)(data[pos] + (data[pos + 1] << 8));

                    pos += 2;
                    ushort nb1 = (ushort)(data[pos] + (data[pos + 1] << 8));

                    pos += (nb1 + 2);
                    byte nb2 = data[pos];

                    pos += (nb2 + 1);
                    byte nb_f = data[pos];

                    pos += 9;
                    pos += (8 * nb_f);

                    pos += 31;
                    int posX = BitConverter.ToInt32(data, pos);
                    pos += 4;
                    int posY = BitConverter.ToInt32(data, pos);

                    NpcOnMap npcOnMap = new NpcOnMap() { };
                    npcOnMap.idOnMap = clickID;
                    npcOnMap.idNpc = npcID;
                    npcOnMap.unk_1 = nb1;
                    npcOnMap.unk_2 = nb2;
                    npcOnMap.unk_3 = nb_f;
                    //Console.WriteLine("npcOnMap >>" + npcOnMap.idOnMap);
                    list_on_map.Add(npcOnMap);
                    //Console.WriteLine("list_on_map >>" + list_on_map.Length);
                    //if (mapid == 12002)
                    //{
                    //    Console.WriteLine("click Id ID >>>" + clickID.ToString());
                    //    Console.WriteLine("npc ID >>>" + npcID.ToString());
                    //    Console.WriteLine("nb1 Id ID >>>" + nb1.ToString());
                    //    Console.WriteLine("nb2 Id ID >>>" + nb2.ToString());
                    //    Console.WriteLine("nb_f Id ID >>>" + nb_f.ToString());
                    //}
                    pos += 4;
                    pos += 41;
                }
                //Console.WriteLine("list on map >>" + list_on_map.Length);
                //Console.WriteLine("mapid >>" + mapid);
                listNpcOnMap.Add(mapid, list_on_map.ToArray());
                ushort nb_entry_exit = read16(data, pos); pos += 2;
                for (int i = 0; i < nb_entry_exit; i++)
                {
                    pos += 3;
                    uint posX = read32(data, pos);
                    pos += 4;
                    uint posY = read32(data, pos);
                    pos += 6;
                    //Console.WriteLine(posX + " " + posY); 
                }

                ushort nb_unk1 = read16(data, pos);
                pos += 2;
                for (int i = 0; i < nb_unk1; i++)
                {
                    pos += 2;
                    ushort nb = read16(data, pos);
                    pos += 2;
                    pos += nb;
                    pos += 21;
                    //Console.WriteLine(posX + " " + posY); 
                }

                ushort nb_unk2 = read16(data, pos);
                //Console.WriteLine(nb_unk2); 
                pos += 2;
                for (int i = 0; i < nb_unk2; i++)
                {
                    pos += 2;
                    ushort nb = read16(data, pos);
                    pos += 2;
                    pos += nb;
                    pos += 17;
                    //Console.WriteLine(posX + " " + posY); 
                }

                ushort nb_dialog = read16(data, pos);
                pos += 2;
                for (int i = 0; i < nb_dialog; i++)
                {
                    byte first = data[pos];
                    ushort first_ = data[pos];
                    pos += 4;
                    byte nb_d = data[pos];
                    ushort nb_dd = read16(data, pos);
                    pos += 4;
                    byte nb_d_2 = data[pos];
                    ushort nb_dd_2 = read16(data, pos);
                    pos += 5 * nb_d;
                    byte nb_d_3 = data[pos];
                    ushort nb_dd_3 = read16(data, pos);

                    //if (mapid == 12002)
                    //{
                    //    Console.WriteLine("first " + first);
                    //    Console.WriteLine("first_ " + first_.ToString());
                    //    Console.WriteLine("nb_d " + nb_d);
                    //    Console.WriteLine("nb_d " + nb_dd.ToString());
                    //    Console.WriteLine("nb_d_2 " + nb_d_2);
                    //    Console.WriteLine("nb_dd_2 " + nb_dd_2.ToString());
                    //    Console.WriteLine("nb_d_3 " + nb_d_3);
                    //    Console.WriteLine("nb_dd_3 " + nb_dd_3.ToString());
                    //}
                }

                ushort nb_warp = read16(data, pos);
                pos += 2;
                uint X = 0, Y = 0;
                for (int i = 0; i < nb_warp; i++)
                {
                    ushort warp_id = read16(data, pos);
                    pos += 2;
                    ushort dest_map = read16(data, pos);
                    pos += 4;
                    uint posX = read32(data, pos) * 20 - 10;
                    pos += 4;
                    uint posY = read32(data, pos) * 20 - 10;
                    pos += 4;
                    pos += 0x19;

                    try
                    {
                        continue;
                        //ushort map1 = mapid;
                        //ushort warpId = warp_id;
                        //ushort map2 = dest_map;
                        //if (map1 == 15000 & map2 == 18000)
                        //{
                        //    Console.WriteLine("Cur Warp Id >> " + warpId);
                        //    Console.WriteLine("Cur map1 >> " + map1);
                        //    Console.WriteLine("Cur map2 >> " + map2);
                        //    Console.WriteLine("Cur posX >> " + posX);
                        //    Console.WriteLine("posY >>" + posY);
                        //}

                        //WarpData.addGateway(map1, warpId, map2, (ushort)posX, (ushort)posY);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }
                int[] firstQuest = new int[] { 0x01, 0x00, 0x06, 0xB9, 0x75, 0xAA, 0xC8, 0xA7, 0xF5 };
                //if (mapid == 12002)
                //{
                //    for (int i = 0; i < firstQuest.Length; i++)
                //    {
                //        Console.Write(firstQuest[i] + " ");
                //    }
                //    Console.WriteLine();
                //}
                //if (mapid == 12002)
                //{
                //    Console.WriteLine(">>> pos >>> " + pos);
                //    Console.WriteLine(">>> 9769 >>> " + read16(data, 9766));

                //}
                int newPos = findSubArrInArr(data, pos, firstQuest);
                if (newPos > -1)
                {
                    pos = newPos - 2;
                    if (mapid == 12002)
                        Console.WriteLine("come here ===>" + pos);
                }

                //for (int i = pos; i < data.Length - 9; i++)
                //{

                // //   int newPos = 
                //    bool firstCondition = data[i] == firstQuest[0] & data[i + 1] == firstQuest[1] & data[i + 2] == firstQuest[2] & data[i + 3] == firstQuest[3] & data[i + 4] == firstQuest[4];
                //    bool seccondCondition = data[i + 5] == firstQuest[5] & data[i + 6] == firstQuest[6] & data[i + 7] == firstQuest[7] & data[i + 8] == firstQuest[8];
                //    if (firstCondition & seccondCondition)
                //    {
                //        pos = i - 2;
                //        if (mapid == 12002)
                //            Console.WriteLine("Come here ===>" + pos);
                //        break;
                //    }
                //}

                if (mapid == 12002)
                {
                    Console.WriteLine(offsets[mapid].Item1);
                    Console.WriteLine("pos + " + pos);
                }

                ushort nb_talk_quest = read16(data, pos);
                if (mapid == 12002)
                    Console.WriteLine("nb_talk_quest + " + nb_talk_quest);
                pos += 2;
                if (mapid == 12002)
                {

                    Console.WriteLine(">>> nb_talk_quest 9769 >>> " + data[pos]);

                }
                if (nb_talk_quest == 0)
                {
                    return;

                }

                for (int i = 0; i < nb_talk_quest; i++)
                {
                    int[] continueQuest = new int[] { i + 1, 0x00, 0x06, 0xB9, 0x75, 0xAA, 0xC8, 0xA7, 0xF5 };
                    int posNextQuest = findSubArrInArr(data, pos, continueQuest);
                    //Console.WriteLine(" post +++ " + posNextQuest);
                    if (posNextQuest > -1)
                    {
                        pos += 0x2C;
                        ushort unknow_id_1 = read16(data, pos);

                        pos += 7;
                        ushort unknow_id_2 = read16(data, pos);
                        //showList(mapid);
                        NpcOnMap[] temp = listNpcOnMap[mapid];
                        var index = Array.FindIndex(listNpcOnMap[mapid], row => row.idOnMap == unknow_id_1);
                        if (index > -1 & unknow_id_2 >= 10000 & unknow_id_2 <= 65535)
                        {
                            listNpcOnMap[mapid][index].idDialog = unknow_id_2;
                        }
                        if (pos >= data.Length)
                        {
                            return;
                        }
                        //Console.WriteLine("pos >> " + pos + " " + "nb_talk_quest 1 " + unknow_id_1 + " idTalk " + unknow_id_2 + " " + data[pos] + " " + data[pos + 1]);
                        //while (pos < posNextQuest)
                        //{
                        //    pos += 0x2C;
                        //    ushort unknow_id_1 = read16(data, pos);
                        //    ushort unknow_id_1_ = data[pos];
                        //    int idTalk = read16_reverse(data, pos);
                        //    pos += 6;

                        //    ushort unknow_id_2 = read16(data, pos);
                        //    ushort unknow_id_2_ = data[pos];

                        //    if (unknow_id_1_ == 0 & unknow_id_2_ == 0)
                        //    {
                        //        continue;
                        //    }
                        //    Console.Write("pos >> " + pos +  " " + "nb_talk_quest 1 " + unknow_id_1+ " idTalk  " + idTalk);
                        //    Console.WriteLine();
                        //    pos += 2;
                        //}
                        pos = posNextQuest;
                    }



                }
            }
        }
        public static int findSubArrInArr(byte[] data, int curr, int[] subArr)
        {
            //bool firstCondition = data[i] == firstQuest[0] & data[i + 1] == firstQuest[1] & data[i + 2] == firstQuest[2] & data[i + 3] == firstQuest[3] & data[i + 4] == firstQuest[4];
            //bool seccondCondition = data[i + 5] == firstQuest[5] & data[i + 6] == firstQuest[6] & data[i + 7] == firstQuest[7] & data[i + 8] == firstQuest[8];
            for (int i = curr; i < (data.Length - subArr.Length - 1); i++)
            {
                if (data[i] == subArr[0])
                {
                    bool is_existed = true;
                    for (int j = 1; j < subArr.Length; j++)
                    {
                        if (data[i + j] != subArr[j])
                        {
                            is_existed = false;
                            break;
                        }
                    }
                    if (is_existed)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

    }
}
