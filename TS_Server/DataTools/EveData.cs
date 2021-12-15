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
        public ushort totalTalking;
        public byte type;

    }
    public struct PackageSend
    {
        public byte[] package;
        public string type;
    }
    public struct Step
    {
        public ushort questId;
        public ushort stepId;
        public PackageSend[] packageSend;
        public ushort npcIdInMap;
        public List<ushort> subStepIds;
        public ushort type; // 2 = Talk | 8 = Reward | A(10) = Select Option | 7 = Condition 
        public ushort optionId;
        public ushort idDialog;
        public ushort qIndex;
        public ushort resBattle;
        public ushort status;
        public ushort requiredLevel;
        public Dictionary<ushort, List<ushort>> requiredQ;
        public Dictionary<ushort, List<ushort>> receivedQ;
        public List<ushort> requiredNpc;
        public Dictionary<ushort, ushort> requiredItem;
        public bool requiredSlotPet;
        public bool normalTalk;
        public List<ushort> rootBit;
    }
    public struct Talks
    {
        public ushort npcIdInMap;
        public byte[] dialog;
    }
    public struct Quest
    {
        public ushort questId;
        public List<Step> steps;
        public int[] requiredQuests;
        public int status;
        public ushort npcIdOnMap;

    }

    public struct ItemOnMap
    {
        public ushort idItemOnMap;
        public ushort idItem;
        public ushort posX;
        public ushort posY;
        public ushort timeDelay;
    }
    public static class EveData
    {
        public static Dictionary<ushort, EveInfo> eveList = new Dictionary<ushort, EveInfo>();
        public static Dictionary<ushort, Tuple<int, int>> offsets = new Dictionary<ushort, Tuple<int, int>>();
        public static Dictionary<ushort, NpcOnMap[]> listNpcOnMap = new Dictionary<ushort, NpcOnMap[]>();
        public static Dictionary<ushort, List<Quest>> questOnMap = new Dictionary<ushort, List<Quest>>();
        public static Dictionary<ushort, List<Step>> listStepOnMap = new Dictionary<ushort, List<Step>>();
        public static Dictionary<ushort, List<ItemOnMap>> listItemOnMap = new Dictionary<ushort, List<ItemOnMap>>();
        public static Dictionary<ushort, Dictionary<ushort, BattleInfo>> battleListOnMap = new Dictionary<ushort, Dictionary<ushort, BattleInfo>>();
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
        public static void addIntToArray(ref ushort[] a, ushort val)
        {
            List<ushort> temp = a.ToList();
            temp.Add(val);
            a = temp.ToArray();
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
                uint id = read32(data, pos);
                pos += 3;
                uint posX = read32(data, pos);
                pos += 4;
                uint posY = read32(data, pos);
                pos += 6;

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
                    
                    pos += 4;
                    pos += 4;
                    byte type = (byte)(read16(data, pos));
                    npcOnMap.type = type;
                    list_on_map.Add(npcOnMap);
                    pos += 37;
                    
                    if (mapid == 12015 & clickID == 1) Console.WriteLine(" type " + type);
                    
                }
                listNpcOnMap.Add(mapid, list_on_map.ToArray());
                ushort nb_entry_exit = read16(data, pos);
                pos++;
                List<ItemOnMap> listItemOnMaps = new List<ItemOnMap>();
                for (int i = 0; i < nb_entry_exit; i++)
                {
                    ItemOnMap itemOnMap = new ItemOnMap();
                    pos++;
                    
                    ushort id = data[pos];
                    itemOnMap.idItemOnMap = id;
                    pos++;
                    ushort idItem = read16(data, pos);
                    itemOnMap.idItem = idItem;
                    pos += 2;
                    ushort posX = read16(data, pos);
                    itemOnMap.posX = posX;
                    pos += 4;
                    ushort posY = read16(data, pos);
                    itemOnMap.posY = posY;
                    pos += 4;
                    ushort timeDelay = read16(data, pos);
                    itemOnMap.timeDelay = timeDelay;
                    pos++;
                    listItemOnMaps.Add(itemOnMap);
                }
                listItemOnMap.Add(mapid, listItemOnMaps);
                pos++;
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
                }

                ushort nb_warp = read16(data, pos);
                pos += 2;
                uint X = 0, Y = 0;
                for (int i = 0; i < nb_warp; i++)
                {

                    ushort warp_id = read16(data, pos);
                    //if (mapid == 12000 & warp_id == 10)
                    //{
                    //    Console.WriteLine("Wrap 10 >> ");
                    //    for (int sm = pos; sm < pos + 40; sm++)
                    //    {
                    //        Console.Write(" " + data[sm]);
                    //    }
                    //    Console.WriteLine();
                    //}
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

                        ushort map1 = mapid;
                        ushort warpId = warp_id;
                        ushort map2 = dest_map;
                        //if (map1 == 15000)
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
                ushort nb_random = read16(data, pos);

                pos += 2;
                for (int i = 0; i < nb_random; i++)
                {
                    ushort idx = read16(data, pos);
                    pos += 2;
                    ushort unk_1 = read16(data, pos);
                    pos += 2;
                    ushort unk_2 = read16(data, pos);
                    pos += 2;
                    ushort unk_3 = read16(data, pos);

                    ushort total = data[pos];
                    pos++;

                    pos = pos + total * 2 + 1;

                }
                
                ushort nb_talk_quest = read16(data, pos);
                pos += 2;
                if (nb_talk_quest == 0)
                {
                    return;

                }

                if (pos >= data.Length)
                {
                    return;
                }
                //List<ushort> mapExclude = new List<ushort>() { 10965, 10966, 10987, 10995, 10996, 11552, 14552, 15025 };
                //if (mapExclude.FindIndex(item => item == mapid) >= 0)
                //{
                //    continue;
                //}
                //List<Quest> listQuests = new List<Quest>();
                List<Step> steps = new List<Step>();
                for (int i = 0; i < nb_talk_quest; i++)
                {
                    #region
                    ushort idx = data[pos];
                    pos += 15;
                    ushort totalTalk = data[pos];
                    pos++;
                    ushort conditions = 0;
                    ushort condition = 0;
                    ushort idxStepAddCondition = 0;
                    for (int j = 0; j < totalTalk; j++)
                    {
                        if (conditions > 1 & condition < conditions)
                        {
                            //01 3C A8 01 02 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 
                            //07 00 00 01 04 0F 00 00 00 00 00 00 00 00 00 00 00 04 00 00 00 00
                            //02 11 27 01 05 01 00 00 00 00 00 00 00 00 00 00 00 0C 01 00 00 01
                            //07 05 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
                            //09 00 00 01 06 86 2F 00 00 00 00 00 00 00 00 00 00 09 00 00 00 00
                            condition++;
                            
                            pos++;
                            ushort typeCondition = data[pos];
                            pos++;

                            ushort questRequired = read16(data, pos); // quest Id or Item Id 
                            ushort unknown_1 = data[pos]; // 0 + 5 
                           
                            ushort unknown_2 = data[pos + 1]; // 0
                            pos += 2;
                           
                           
                            ushort quantity = data[pos]; // 1 - 2 or quantity
                            ushort unknown_4 = data[pos+1]; // 0 - 4 - 5 
                            pos += 2;
                            ushort requiredLevel = read16(data, pos); // Level required - NPC ID 
                            ushort required = data[pos]; // 0 1 2
                            if (mapid == 12001 & i == 15)
                            {
                                Console.WriteLine(" unknown_1 >>> " + unknown_1);
                                Console.WriteLine(" unknown_2 >>> " + quantity);
                            }
                            if (typeCondition == 7)
                            {
                                Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                                steps[idxStepAddCondition - 1] = tempStep;
                                if (unknown_1 == 0 & requiredLevel > 0)
                                {
                                    tempStep.requiredLevel = requiredLevel;
                                }
                                if (unknown_1 == 5 & quantity == 1)
                                {
                                    if (mapid == 12001 & i == 15)
                                    {
                                        Console.WriteLine("  tempStep.stepId >>> " + tempStep.stepId);
                                    }
                                    // Full pet 
                                    tempStep.requiredSlotPet = true;
                                }
                                if (unknown_1 == 5 && quantity == 2)
                                {
                                    // available slot pet 
                                    tempStep.requiredSlotPet = false;
                                }
                                steps[idxStepAddCondition - 1] = tempStep;
                            }
                            if (typeCondition == 2)
                            {
                                Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                
                                if (required == 1 | required == 2)
                                {
                                    if (!tempStep.requiredQ.ContainsKey(questRequired)) {
                                        tempStep.requiredQ.Add(questRequired, new List<ushort> { quantity, unknown_4, required });
                                    }
                                    //ushort temp = (ushort)(questRequired + quantity + unknown_4 + required);
                                    
                                }
                                if (required == 0 || unknown_1 == 2)
                                {
                                    //ushort temp = (ushort)(questRequired + quantity + unknown_4 + required);
                                    if (!tempStep.receivedQ.ContainsKey(questRequired))
                                    {
                                        tempStep.receivedQ.Add(questRequired, new List<ushort> { quantity, unknown_4, required });
                                    }
                                        
                                }
                                steps[idxStepAddCondition - 1] = tempStep;
                            }
                            if (typeCondition == 9)
                            {
                                Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                                tempStep.requiredNpc.Add(requiredLevel);
                                steps[idxStepAddCondition - 1] = tempStep;
                            }
                            if (typeCondition == 1)
                            {
                                Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                                tempStep.requiredItem.Add(questRequired, quantity);
                                steps[idxStepAddCondition - 1] = tempStep;
                            }
                            //if (typeCondition == 8)
                            //{
                            //    // Res Battle 1 ==> Win 
                            //    // Res Battle 2 ==> Lose
                            //    // Res battle 3 ==> Runout
                            //    resBattle = read16(data, pos + 1);
                            //    if (mapid == 12001)
                            //    {
                            //        Console.WriteLine("resBattle >>" + resBattle);
                            //    }
                            //    step.resBattle = resBattle;
                            //}

                            //if (type == 3)
                            //{
                            //    idBox = data[pos - 1];
                            //}
                            pos += 17;
                            continue;
                        }
                        else {
                            conditions = 0;
                            condition = 0;
                        }
                        Step step = new Step();
                        step.requiredItem = new Dictionary<ushort, ushort>();
                        step.requiredNpc = new List<ushort>();
                        step.requiredQ = new Dictionary<ushort, List<ushort>>();
                        step.receivedQ = new Dictionary<ushort, List<ushort>>();
                        step.rootBit = new List<ushort>();
                        step.qIndex = idx;
                        step.subStepIds = new List<ushort>();
                        ushort idxStep = data[pos];
                        step.stepId = idxStep;
                        pos++;
                        ushort type = data[pos];
                        step.type = type;
                        pos += 2;
                        ushort questId = read16(data, pos - 1);
                        //if (mapid == 12001 & i == 15) Console.WriteLine("questId >>" + questId);
                        ushort optionId = 0;
                        ushort resBattle = 0;
                        ushort idBox = 0;
                        // Select options

                        if (type == 10)
                        {
                            ushort idDialog = data[pos - 1];
                            step.idDialog = idDialog;
                            optionId = read16(data, pos + 1);
                            step.optionId = optionId; 
                        }
                        if (type == 2)
                        {
                            step.questId = questId;
                            ushort unknown_1 = data[pos + 1]; // 1 + 2 + 3 
                            ushort unknown_2 = data[pos + 2]; // 0 + 5
                            ushort required = data[pos + 3]; // 0 + 1 + 2 
                            if (required == 1 | required == 2)
                            {
                                step.requiredQ.Add(questId, new List<ushort> { unknown_1, unknown_2, required });
                            }
                            if (required == 0 || unknown_1 == 2)
                            {
                                step.receivedQ.Add(questId, new List<ushort> { unknown_1, unknown_2, required });
                            }
                            step.rootBit.Add(unknown_1);
                            step.rootBit.Add(unknown_2);
                            step.rootBit.Add(required);
                        }
                        // 02 07 00 00 01 04 0F 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00
                        // 11 07 05 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
                        // 11 07 05 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
                        if (type == 7)
                        {
                            ushort type_required = data[pos - 1];
                            ushort unknown_1 = data[pos];
                            ushort unknown_2 = data[pos + 1];
                            ushort unknown_3 = data[pos + 2];
                            ushort requiredLevel = read16(data, pos + 3);
                            if (type_required == 0 & requiredLevel > 0)
                            {
                                step.requiredLevel = requiredLevel;
                            }
                            if (type_required == 5 & unknown_2 == 1)
                            {
                                // Full pet 
                                step.requiredSlotPet = true;
                            }
                            if (type_required == 5 && unknown_2 == 2)
                            {
                                // available slot pet 
                                step.requiredSlotPet = false;
                            }
                            
                        }
                        if (type == 9)
                        {
                            //sample 09 00 00 01 05 86 2F
                            ushort unknown_1 = read16(data, pos-1);
                            ushort unknown_2 = data[pos + 1];
                            ushort unknown_3 = data[pos + 2];
                            ushort requiredNpc = read16(data, pos + 3);
                            step.requiredNpc.Add(requiredNpc);
                        }
                        if (type == 1)
                        {
                            ushort itemRequired = read16(data, pos - 1);
                            ushort quantity = data[pos + 1];
                            ushort unknown_1 = data[pos + 2];
                            ushort unknown_2 = read16(data, pos + 3);
                            step.requiredItem.Add(itemRequired, quantity);
                        }
                        if (type == 8)
                        {
                            // Res Battle 1 ==> Win 
                            // Res Battle 2 ==> Lose
                            // Res battle 3 ==> Runout
                            resBattle = read16(data, pos + 1);
                            if (mapid == 12001)
                            {
                                Console.WriteLine("resBattle >>" + resBattle);
                            }
                            step.resBattle = resBattle;
                        }

                        if (type == 3)
                        {
                            idBox = data[pos - 1];
                        }
                        if (type == 0)
                        {
                            step.normalTalk = true;
                        }
                        else {
                            step.normalTalk = false;
                        }
                        pos += 2;
                        ushort unk2 = read16(data, pos-1);
                       
                        pos += 2;
                        ushort unk3 = read16(data, pos-1);
                        if (type == 2)
                        {
                            step.status = unk3;
                           
                        }
                        pos += 2;
                        
                        ushort unk4 = read16(data, pos);
                        pos += 9;
                        ushort prevStep = data[pos];
                        pos++;
                        conditions = data[pos];
                        if (conditions > 1)
                        {
                            condition++;
                            idxStepAddCondition = (ushort)(steps.Count + 1);
                        }
                        // Here pos is total sub step
                        pos += 3;
                        ushort totalPackages = data[pos];
                        int toPos = pos;
                        pos++;
                        List<PackageSend> listPackages = new List<PackageSend>();
                        ushort npcId = 0;
                        for (int k = 0; k < totalPackages; k++)
                        {
                            byte[] package = new byte[] {data[pos], data[pos+1], data[pos+2], data[pos+3], data[pos+4],
                            data[pos+5], data[pos+6], data[pos+7], data[pos+8], data[pos+9], data[pos+10],
                            data[pos+11], data[pos+12], data[pos+13]};
                            PackageSend pg = new PackageSend();
                            pg.package = package;
                            listPackages.Add(pg);
                            if (idBox > 0)
                            {
                                step.npcIdInMap = idBox;
                            }
                            else if ((data[pos + 3] == 1 | data[pos + 3] == 6) & data[pos + 4] == 3)
                            {
                                npcId = data[pos + 5];
                                if (step.npcIdInMap ==0)
                                    step.npcIdInMap = npcId;
                            }
                            if (data[pos + 3] == 6 & data[pos + 4] == 3)
                            {
                                ushort idDialogOption = (ushort)(PacketReader.read16(data, pos + 12));
                            }
                            if (data[pos + 3] == 3)
                            {
                                ushort idBattle = (ushort)(PacketReader.read16(data, pos + 12));
                            }
                            if (data[pos + 3] == 0 & data[pos + 4] == 3)
                            {
                                ushort idNpcInMapJoin = data[pos + 5];
                            }
                            if (data[pos + 3] == 0 & data[pos + 4] == 1)
                            {
                                ushort idItemReceived = (ushort)(PacketReader.read16(data, pos + 5));
                                ushort unknown = data[pos + 7];
                                ushort quantity = data[pos + 8];
                            }
                            pos += 14;
                        }
                        //if (option > 1)
                        //{
                        //    step.options = new Dictionary<ushort, List<PackageSend>>();
                        //    step.options.Add(option, listPackages);
                        //}
                        //if (mapid == 12002 & npcId <= nb_npc & npcId > 0) Console.WriteLine("NPC >>> "+ npcId);
                        step.packageSend = listPackages.ToArray();
                        steps.Add(step);
                    }

                    //quest.steps = steps;

                    //listQuests.Add(quest);
                    //TalkQuestItem talkQuestItem = new TalkQuestItem();
                    //ushort id_talk_quest = data[pos];
                    //talkQuestItem.idTalking = id_talk_quest;
                    //pos += 16;
                    //if (pos >= data.Length)
                    //{
                    //    return;
                    //}
                    //ushort total_talk_quest = data[pos];
                    //talkQuestItem.totalTalking = total_talk_quest;
                    ////pos += 1;
                    //for (int j = 0; j < total_talk_quest; j++)
                    //{
                    //    pos += 23;
                    //    ushort total_step = data[pos];
                    //    //pos += 1;
                    //    for (int k = 0; k < total_step; k++)
                    //    {
                    //        pos += 2;
                    //        byte[] dialog = new byte[12];
                    //        for (int l = 0; l < 12; l++)
                    //        {
                    //            dialog[l] = data[pos + l];
                    //        }
                    //        pos += 14;
                    //    }

                    //}
                    #endregion
                }

                listStepOnMap.Add(mapid, steps);
                //questOnMap.Add(mapid, listQuests);

                ushort nb_battle = read16(data, pos);
                pos += 2;
                Dictionary<ushort, BattleInfo> battleList = new Dictionary<ushort, BattleInfo>();
                ushort[] listNpcId;
                for (int i = 0; i < nb_battle; i++)
                {
                    ushort defaultGround = 876;
                    ushort index = data[pos];
                    pos += 5;
                    ushort quantityNpc = data[pos];
                    pos += 2;
                    listNpcId = new ushort[11];
                    for (int j = 0; j < quantityNpc; j++)
                    {
                        ushort indexNpc = read16(data, pos);
                        pos += 2;
                        ushort npcId = read16(data, pos);
                        pos += 2;
                        ushort posNpc = (ushort)(data[pos] - 1);
                        pos++;
                        ushort turnBatle = data[pos];
                        pos++;
                        //if (mapid == 12001) { Console.WriteLine(" index >>> " + indexNpc);
                        //    Console.WriteLine(" index >>> " + indexNpc);
                        //    Console.WriteLine(" npcId >>> " + npcId);
                        //    Console.WriteLine(" posNpc >>> " + posNpc);
                        //    Console.WriteLine(" turnBatle >>> " + turnBatle);
                        //}
                        if (posNpc > 10) continue;
                        listNpcId[posNpc] = npcId;
                    }
                    battleList.Add(index, new BattleInfo(defaultGround, listNpcId));
                    ushort unknow_1 = read16(data, pos);

                    pos += 2;
                    ushort unknow_2 = read16(data, pos);

                    pos += 2;
                    for (int j = 0; j < unknow_2; j++)
                    {
                        ushort unknow_3 = data[pos + 11];
                        pos += 12;
                        pos += unknow_3 * 13;
                    }
                }
                battleListOnMap.Add(mapid, battleList);
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

        //public static Step getStepByQuest(TSClient client, ushort npcIdOnMap) {
        //    List<Step> steps = listStepOnMap[client.map.mapid].FindAll(item => item.npcIdInMap == npcIdOnMap);

        //    List<Step> stepsQ = steps.FindAll(item => !item.questId.Equals(null) | item.questId > 0);
        //    Step step;
        //    if (stepsQ.Count > 0)
        //    {
        //        ushort questId = stepsQ[0].questId;
        //        int currentStep = client.checkQuest(client, questId);
        //        if (currentStep > -1)
        //        {
        //            step = stepsQ.Find(item => item.stepId == currentStep);
        //        }
        //        else
        //        {
        //            step = stepsQ.Find(item => item.status != 1);
        //            currentStep = step.stepId;
        //        }
        //        client.insertOrUpdateQuest(client, questId, (ushort)currentStep);
        //    }
        //    else
        //    {
        //        step = steps[0];
        //    }
        //    return step;
        //}
    }
}
