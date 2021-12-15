using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TS_Server.Server;
using System.Globalization;

namespace TS_Server.Client
{
    public class TSClient
    {
        private Socket socket;
        private String clientID;
        private TSCharacter chr;
        public bool creating, online;
        public BattleAbstract battle;
        public TSMap map;
        public uint accID;
        public byte[] name_temp;
        public TSWorld world;
        public ushort warpPrepare;
        public ushort idNpcTalking;
        public ushort unkIdNpc;
        public DataTools.Step currentStep;
        public ushort idxDialog;
        public ushort selectMenu;
        public ushort idxQ = 0;
        public ushort optionId;
        public ushort idNpc;
        public ushort idDialog;
        public ushort idBattle;
        public ushort resBattle;
        public bool finishQ = false;
        public TSClient(Socket s, String id)
        {
            socket = s;
            clientID = id;
            creating = false;
            online = false;
        }

        public void createChar(byte[] data)
        {
            chr = new TSCharacter(this);

            chr.initChar(data, name_temp);
            chr.loginChar();
            world = TSServer.getInstance().getWorld();
        }

        public int checkLogin(uint acc_id, string password)
        {
            //check exist, online, create char(เช็คตัวละครออนไลน์)
            int ret = 0;
            var c = new TSMysqlConnection();

            MySqlDataReader data = c.selectQuery("SELECT password, loggedin FROM account WHERE id = " + acc_id);

            if (!data.Read())
                ret = 1;
            else if (data.GetString(0) != password)
                ret = 1;
            else if (data.GetBoolean(1))
                ret = 2;
            else
            {
                var c2 = new TSMysqlConnection();
                MySqlDataReader data2 = c2.selectQuery("SELECT accountid FROM chars WHERE accountid = " + acc_id);

                if (!data2.Read())
                {
                    accID = acc_id;
                    ret = 3;
                }
                data2.Close();
                c2.connection.Close();
            }

            data.Close();
            c.connection.Close();

            if (ret == 0)
            {
                accID = acc_id;
                chr = new TSCharacter(this);
            }

            return ret;
        }


        public int checkQuest(TSClient client, ushort questId, ushort bit_3 = 0, ushort bit_4 = 0, ushort bit_5 = 0)
        {
            uint userId = client.accID;
            //check exist, online, create char(เช็คตัวละครออนไลน์)
            int ret = -1;
            var c = new TSMysqlConnection();
            string sql = "SELECT questId FROM quest WHERE charId = " + userId + " and questId = " + questId;
            if (bit_3 != 0)
            {
                sql += " and bit_3 = " + bit_3;
            }
            if (bit_4 != 0)
            {
                sql += " and bit_4 = " + bit_4;
            }
            if (bit_5 != 0)
            {
                sql += " and bit_5 = " + bit_5;
            }
            MySqlDataReader data = c.selectQuery(sql);

            if (!data.Read())
                ret = -1;
            else
            {
                ret = data.GetInt16(0);
            }


            data.Close();
            c.connection.Close();
            return ret;
        }
        public List<int> getCurrentStep(TSClient client, ushort questId)
        {
            List<int> res = new List<int>();
            uint userId = client.accID;
            var c = new TSMysqlConnection();
            string sql = "SELECT bit_3, bit_4, bit_5 FROM quest WHERE charId = " + userId + " and questId = " + questId;

            MySqlDataReader data = c.selectQuery(sql);

            if (!data.Read())
            {
                res.Add(0);
                res.Add(0);
                res.Add(0);
            }
            else
            {
                res.Add(data.GetInt16(0));
                res.Add(data.GetInt16(1));
                res.Add(data.GetInt16(2));
            }


            data.Close();
            c.connection.Close();
            return res;
        }
        //public void checkMyQuest(TSClient client, )
        //{
        //    uint userId = client.accID;
        //    uint mapId = client.map.mapid;
        //    var c = new TSMysqlConnection();
        //    MySqlDataReader data = c.selectQuery("SELECT questId, isFinish, bit_3, bit_4, bit_5 FROM quest WHERE charId = " + userId + " and mapId = " + mapId);
        //    while (data.Read())
        //    {
        //        int s = data.GetInt32("slot");
        //        int sid = data.GetInt32("pet_sid");
        //        pet[s - 1] = new TSPet(this, sid, (byte)s);
        //        pet[s - 1].loadPetDB();
        //    }
        //    data.Close();
        //    c.connection.Close();
        //}

        public void insertOrUpdateQuest(TSClient client, ushort questId, ushort bit_3 = 0, ushort bit_4 = 0, ushort bit_5 = 0)
        {
            #region Old Code 
            //var c = new TSMysqlConnection();
            //c.connection.Open();
            //int currentStep = checkQuest(client, questId);
            //if (currentStep == -1)
            //{
            //    // Add new quest
            //    var cmd = new MySqlCommand();
            //    cmd.Connection = c.connection;
            //    cmd.CommandText = "INSERT INTO quest (questId, charId, stepId, mapId, isFinish) "
            //                  + "VALUES (" + questId + " , " + client.accID + "," + stepId + "," + client.map.mapid + "," + isFinish + ");";
            //    cmd.Prepare();
            //    cmd.ExecuteNonQuery();

            //}
            //else
            //{
            //    var cu = new TSMysqlConnection();
            //    Console.WriteLine(" Update Q " + questId + " step >> " + stepId);
            //    Console.WriteLine(" Update charId " + client.accID + " client.map.mapid >> " + client.map.mapid);
            //    cu.updateQuery("UPDATE `quest` SET `stepId` = " + stepId  + " `isFinish` = " + isFinish + " WHERE `charId` = " + client.accID + " and `questId` = " + questId + " and `mapId` = " + client.map.mapid + ";");

            //}
            #endregion
            var c = new TSMysqlConnection();
            c.connection.Open();
            int currentStep = checkQuest(client, questId);
            if (currentStep == -1)
            {
                // Add new quest
                var cmd = new MySqlCommand();
                cmd.Connection = c.connection;
                cmd.CommandText = "INSERT INTO quest (questId, charId, bit_3, bit_4, bit_5) "
                              + "VALUES (" + questId + " , " + client.accID + "," + bit_3 + "," + bit_4 + "," + bit_5 + ");";
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            else
            {
                var cu = new TSMysqlConnection();
                //Console.WriteLine(" Update Q " + questId + " step >> " + stepId);
                Console.WriteLine(" Update charId " + client.accID + " client.map.mapid >> " + client.map.mapid);
                cu.updateQuery("UPDATE `quest` SET `bit_3` = " + bit_3 + ", `bit_4` = " + bit_4 + ", `bit_5` = " + bit_5 + " WHERE `charId` = " + client.accID + " and `questId` = " + questId + ";");

            }

            c.connection.Close();
        }

        public bool isTeamLeader()
        {
            return false;
        }
        public bool isJoinedTeam()
        {
            return false;
        }

        public bool isOnline()
        {
            return online;
        }

        public TSCharacter getChar()
        {
            return chr;
        }

        public Socket getSocket()
        {
            return socket;
        }

        public String getClientID()
        {
            return clientID;
        }

        public void reply(byte[] data)
        {
            try
            {
                socket.Send(data);
            }
            catch (Exception e)
            {
                Console.WriteLine("Socket down, client " + clientID + " disconnect");
                disconnect();
            }
        }
        public byte[] appendArray(byte[] arr1, byte[] arr2)
        {
            byte[] z = new byte[arr1.Length + arr2.Length];
            arr1.CopyTo(z, 0);
            arr2.CopyTo(z, arr1.Length);
            return z;
        }
        public void savetoDB()
        {
            var c = new TSMysqlConnection();
            c.connection.Open();
            chr.saveCharDB(c.connection);
            for (int i = 0; i < 4; i++)
                if (chr.pet[i] != null)
                    chr.pet[i].savePetDB(c.connection, false);
            c.connection.Close();
        }

        public void continueMoving()
        {
            RequestComplete();
            AllowMove();
        }
        public void RequestComplete()
        {
            reply(new PacketCreator(0x14, 8).send());
        }
        public void AllowMove()
        {
            reply(new PacketCreator(5, 4).send());
            reply(new PacketCreator(0x0F, 0x0A).send());
        }
        public static byte[] encodeByteWith0xAD(byte[] byte_0)
        {
            checked
            {
                byte[] array = new byte[byte_0.Length - 1 + 1];
                int arg_14_0 = 0;
                int num = byte_0.Length - 1;
                for (int i = arg_14_0; i <= num; i++)
                {
                    array[i] = (byte)(byte_0[i] ^ 0xAD);
                }
                return array;
            }
        }
        public static byte[] convertStringToByteArray(string string_0)
        {
            checked
            {
                byte[] array = new byte[(int)Math.Round(unchecked((double)string_0.Length / 2.0 - 1.0)) + 1];
                try
                {
                    for (int i = 0; i < string_0.Length; i += 2)
                    {
                        array[(int)Math.Round((double)i / 2.0)] = byte.Parse(string_0.Substring(i, 2), NumberStyles.HexNumber);
                    }
                }
                catch (Exception expr_62)
                {
                    return new byte[] { };
                }
                return array;
            }
        }
        public void Sendpacket(string packet)
        {
            try
            {
                byte[] array = encodeByteWith0xAD(convertStringToByteArray(packet));
                socket.Send(array, 0, array.Length, SocketFlags.None);
            }
            catch (Exception expr_2E)
            {
                return;
            }
        }
        public static string convertIntToStr8Byte(int int_2)
        {
            return int_2.ToString("X8").Substring(6, 2) + int_2.ToString("X8").Substring(4, 2) + int_2.ToString("X8").Substring(2, 2) + int_2.ToString("X8").Substring(0, 2);
        }
        public static byte[] convertIntToArrayByte(uint int_2)
        {
            string sp1 = int_2.ToString("X8").Substring(6, 2);
            byte f1 = byte.Parse(sp1, NumberStyles.HexNumber);
            string sp2 = int_2.ToString("X8").Substring(4, 2);
            byte f2 = byte.Parse(sp2, NumberStyles.HexNumber);
            string sp3 = int_2.ToString("X8").Substring(2, 2);
            byte f3 = byte.Parse(sp3, NumberStyles.HexNumber);
            string sp4 = int_2.ToString("X8").Substring(0, 2);
            byte f4 = byte.Parse(sp4, NumberStyles.HexNumber);
            return new byte[] { f1, f2, f3, f4 };
        }
        public static byte[] convertIntToArrayByte4(int int_2)
        {
            string sp3 = int_2.ToString("X4").Substring(2, 2);
            byte f3 = byte.Parse(sp3, NumberStyles.HexNumber);
            string sp4 = int_2.ToString("X4").Substring(0, 2);
            byte f4 = byte.Parse(sp4, NumberStyles.HexNumber);
            return new byte[] { f3, f4 };
        }
        public void stopTalking(TSClient client)
        {
            client.idNpcTalking = 0;
            client.idxDialog = 0;
            client.selectMenu = 0;
        }
        public void processStep(TSClient client)
        {
            PacketCreator p2 = new PacketCreator();
            if ((client.idNpc == 16080 | client.idNpc == 16004 | client.idNpc == 16011) & client.selectMenu > 0)
            {
                switch (client.selectMenu)
                {
                    case 30:
                        {
                            p2 = new PacketCreator(0x1D, 09);
                            p2.addByte(0);
                            client.reply(p2.send());
                            p2 = new PacketCreator(0x1D, 04);
                            uint my_gold_bank = client.getChar().gold_bank;
                            byte[] gold_bank = convertIntToArrayByte(my_gold_bank);
                            p2.addByte(gold_bank[0]);
                            p2.addByte(gold_bank[1]);
                            p2.addByte(gold_bank[2]);
                            p2.addByte(gold_bank[3]);
                            client.reply(p2.send());
                            p2 = new PacketCreator(0x1D, 05);
                            client.reply(p2.send());
                            p2 = new PacketCreator(0x1D, 09);
                            p2.addByte(0);
                            client.reply(p2.send());
                            break;
                        }
                    case 31:
                        {
                            p2 = new PacketCreator(0x1D, 06);
                            client.reply(p2.send());
                            break;
                        }
                }
                return;
            }
            if (client.selectMenu == 40)
            {
                stopTalking(client);
                client.continueMoving();
                return;
            }
            DataTools.Step step = client.currentStep;
            if (client.selectMenu != 0)
            {
                //Case select menu -->
                DataTools.Step temp_step = DataTools.EveData.listStepOnMap[client.map.mapid].Find(item => client.selectMenu == item.optionId & item.idDialog == client.idDialog);
                if (temp_step.packageSend != null)
                {
                    step = temp_step;
                    client.currentStep = temp_step;
                    client.idxDialog = 0;
                    client.selectMenu = 0;
                }
                else
                {
                    stopTalking(client);
                    client.continueMoving();
                    return;
                }
            }
            if (client.idxQ > 0)
            {
                DataTools.Step temp_step = DataTools.EveData.listStepOnMap[client.map.mapid].Find(item => client.idxQ == item.qIndex & item.resBattle == client.resBattle);
                client.idxQ = 0;
                if (temp_step.packageSend != null)
                {
                    step = temp_step;
                    client.currentStep = temp_step;
                    client.idxDialog = 0;
                    client.selectMenu = 0;
                }
                else
                {
                    stopTalking(client);
                    client.continueMoving();
                    return;
                }
            }
            if (client.finishQ == true)
            {
                DataTools.Step temp_step = new DataTools.Step();
                DataTools.Step[] steps = DataTools.EveData.listStepOnMap[client.map.mapid].FindAll(item => item.npcIdInMap == client.idNpcTalking).ToArray();
                for (int i = 0; i < steps.Length; i++)
                {
                    DataTools.Step item = steps[i];
                    if (item.stepId == 15)
                        Console.WriteLine(item.stepId);
                    if (item.requiredQ.Count > 0)
                    {

                        foreach (KeyValuePair<ushort, List<ushort>> entry in item.requiredQ)
                        {
                            if (entry.Value.ElementAt(0) == 1 & entry.Value.ElementAt(1) == 2 & entry.Value.ElementAt(2) == 1)
                            {
                                temp_step = item;
                            }
                        }
                    }
                }
                if (!temp_step.questId.Equals(null))
                {
                    step = temp_step;
                    client.currentStep = temp_step;
                    client.idxDialog = 0;
                    client.selectMenu = 0;
                }
                client.finishQ = false;
            }
            //if (step.packageSend.Equals(null))
            //{
            //    stopTalking(client);
            //    client.continueMoving();
            //    return;
            //}
            if (client.idxDialog >= step.packageSend.Length)
            {
                stopTalking(client);
                client.continueMoving();
                return;
            }
            List<DataTools.PackageSend> packages = step.packageSend.ToList();
            byte[] packageToSend = packages.ElementAt(client.idxDialog).package;

            p2 = new PacketCreator(0x14, 1);
            p2.addByte(0); p2.addByte(packageToSend[0]); p2.addByte(packageToSend[1]); p2.addByte(packageToSend[2]); p2.addByte(packageToSend[3]);
            p2.addByte(packageToSend[4]);
            p2.addByte(packageToSend[5]); p2.addByte(packageToSend[6]);

            p2.addByte(packageToSend[7]);
            p2.addByte(packageToSend[8]); p2.addByte(packageToSend[9]); p2.addByte(packageToSend[10]); p2.addByte(packageToSend[11]);
            int idDialog = convertArrayByteToInt(new byte[] { packageToSend[12], packageToSend[13] });
            ushort idDialog_2 = (ushort)(PacketReader.read16(packageToSend, 12));
            p2.add16(idDialog_2);
            if (packageToSend[3] == 6 & packageToSend[4] == 3)
            {
                client.idDialog = idDialog_2;
            }
            if (packageToSend[3] == 3)
            {
                ushort idBattle = (ushort)(PacketReader.read16(packageToSend, 12));
                client.idBattle = idBattle;
                client.idxQ = step.qIndex;
            }
            if (packageToSend[3] == 0 & packageToSend[4] == 3 & packageToSend[6] == 0 & packageToSend[7] == 1)
            {
                ushort idNpcInMapJoin = packageToSend[5];
                var index = Array.FindIndex(DataTools.EveData.listNpcOnMap[client.map.mapid], row => row.idOnMap == idNpcInMapJoin);
                ushort idNpc = DataTools.EveData.listNpcOnMap[client.map.mapid][index].idNpc;
                byte typePet = DataTools.EveData.listNpcOnMap[client.map.mapid][index].type;
                client.getChar().addPet(idNpc, 0, typePet);
            }
            if (packageToSend[3] == 0 & packageToSend[4] == 1)
            {
                ushort idItem = (ushort)(PacketReader.read16(packageToSend, 5));
                ushort quantity = packageToSend[8];
                client.getChar().inventory.addItem(idItem, quantity, true);
            }
            if (packageToSend[3] == 0 & packageToSend[4] == 2)
            {
                ushort questId = (ushort)(PacketReader.read16(packageToSend, 5));
                ushort unknown = packageToSend[7];
                ushort isFinish = packageToSend[8];
                if (unknown == 1 & isFinish == 1)
                {
                    List<int> currentPackageStep = getCurrentStep(client, questId);
                    int bit_1 = currentPackageStep[0];
                    int bit_2 = currentPackageStep[1];
                    int bit_3 = currentPackageStep[2];
                    if ((bit_1 == 2 | bit_1 == 0 | bit_1 == 3) & bit_2 == 0)
                    {
                        client.insertOrUpdateQuest(client, questId, 1, 5, 1);
                    }
                    else
                    {
                        client.insertOrUpdateQuest(client, questId, (ushort)bit_1, (ushort)bit_2, (ushort)(bit_3 + 1));
                    }
                    if (bit_1 == 1 & bit_1 == 5 & bit_3 == 1)
                    {
                        client.finishQ = true;
                    }
                }
            }
            Console.WriteLine("Senddd click npc > " + String.Join(",", p2.getData()));
            client.reply(p2.send());
            client.idxDialog++;
        }

        public void ClickkNpc(byte[] data, TSClient client)
        {
            //TSCharacter ch = client.getChar();
            byte[] unknow = new byte[] { data[0], data[1] };
            int id_unknow = convertArrayByteToInt(unknow);

            byte[] _id_talking = new byte[] { data[2], data[3] };
            int id_talking = PacketReader.read16(data, 2);
            client.idNpcTalking = (ushort)id_talking;


            var index = Array.FindIndex(DataTools.EveData.listNpcOnMap[client.map.mapid], row => row.idOnMap == id_talking);
            if (index > -1)
            {
                ushort idNpc = DataTools.EveData.listNpcOnMap[client.map.mapid][index].idNpc;
                client.idNpc = idNpc;

                DataTools.Step[] steps = DataTools.EveData.listStepOnMap[client.map.mapid].FindAll(item => item.npcIdInMap == id_talking).ToArray();
                //Console.WriteLine("Count >> " + steps.Count);
                List<DataTools.Step> stepValidate = new List<DataTools.Step>();
                for (int i = 0; i < steps.Length; i++)
                {
                    DataTools.Step item = steps[i];
                    if (item.requiredQ.Count > 0)
                    {
                        bool isValidate = true;
                        foreach (KeyValuePair<ushort, List<ushort>> entry in item.requiredQ)
                        {
                            int idxCurrentQ = checkQuest(client, entry.Key, entry.Value.ElementAt(0), entry.Value.ElementAt(1), entry.Value.ElementAt(2));
                            if (idxCurrentQ == -1)
                            {
                                isValidate = false;
                                break;
                            }
                        }
                        if (!isValidate)
                        {
                            continue;
                        }
                    }
                    if (item.requiredSlotPet && chr.next_pet != 4)
                    {
                        continue;
                    }
                    if (item.requiredItem.Count > 0)
                    {
                        bool isValidate = true;
                        foreach (KeyValuePair<ushort, ushort> entry in item.requiredItem)
                        {
                            int idxItem = chr.inventory.haveItem(entry.Key);
                            if (idxItem == -1)
                            {
                                isValidate = false;
                                break;
                            }

                        }
                        if (!isValidate)
                        {
                            continue;
                        }
                    }
                    if (item.requiredNpc.Count > 0)
                    {
                        bool isFound = false;
                        item.requiredNpc.ForEach(npcId =>
                        {
                            for (int sl = 0; sl < chr.next_pet; sl++)
                                if (chr.pet[sl].NPCid == npcId)
                                    isFound = true;
                        });
                        if (!isFound)
                        {
                            continue;
                        }
                    }
                    if (item.receivedQ.Count > 0)
                    {
                        
                        foreach (KeyValuePair<ushort, List<ushort>> entry in item.receivedQ)
                        {
                            ushort bit_1 = entry.Value.ElementAt(0);
                            ushort bit_2 = entry.Value.ElementAt(1);
                            ushort bit_3 = entry.Value.ElementAt(2);
                            int idxCurrentQ = checkQuest(client, entry.Key);
                            if (idxCurrentQ == -1 & ((bit_1 == 3 && bit_2 == 5 && bit_3 == 0) | (bit_1 == 2 & bit_2 == 0)))
                            {
                                
                                insertOrUpdateQuest(client, entry.Key, entry.Value.ElementAt(0), entry.Value.ElementAt(1), entry.Value.ElementAt(2));
                            }                         
                        }
                      
                        //if (isValidate)
                        //{
                        //    stepValidate.Add(item);
                        //}
                    }

                    if (!stepValidate.Contains(item))
                    {
                        stepValidate.Add(item);
                    }
                    //Console.WriteLine("Item valid >> " + item.stepId);
                    //stepValidate.Add(item);
                }
                if (stepValidate.Count > 0)
                {
                    

                    currentStep = stepValidate.FirstOrDefault();
                    stepValidate.ForEach(item =>
                    {
                        if (item.questId > 0)
                        {
                            Console.WriteLine(" here is goo >" + item.stepId);
                            List<int> currentPackageStep = getCurrentStep(client, item.questId);
                            int bit_1 = currentPackageStep[0];
                            int bit_2 = currentPackageStep[1];
                            int bit_3 = currentPackageStep[2];
                            ushort r_bit_1 = item.rootBit.ElementAt(0);
                            ushort r_bit_2 = item.rootBit.ElementAt(1);
                            ushort r_bit_3 = item.rootBit.ElementAt(2);

                            if (bit_1 == (int)r_bit_1 & bit_2 == (int)r_bit_2 & bit_3 == (int)r_bit_3)
                            {
                                currentStep = item;
                            }
                        }
                        
                    });

                    Console.WriteLine("Steppp validated >>>" + currentStep.stepId);
                    processStep(client);

                }
                else
                {
                    client.continueMoving();
                }


                //byte[] arr = new byte[] { 244, 68, 17, 0, 20, 1, 0, 0, 0, 1, 6, 3, 2, 0, 0, 0, 0, 0, 0, 6, 0 };
                ////// Chu Tien trang
                //if (id_talking == 7 || id_talking == 2 || id_talking == 1)
                //{
                //    //byte[] arrSend = new byte[arr.Length];
                //    //for (int i = 0; i < arr.Length; i++)
                //    //{
                //    //    arrSend[i] = (byte)(arr[i] ^ 0xAD);
                //    //}
                //    //client.reply(arrSend);
                //    // User talk
                //    //14 01 00 00 00 02 01 07 00 00 00 00 00 00 00 7F 28    
                //    // NPC Talk
                //    //14 01 00 00 00 03 01 03 01 00 00 00 00 00 00 CA 28
                //    // User talk
                //    //14 01 00 00 00 04 01 07 00 00 00 00 00 00 00 CB 28
                //    // NPC Talk
                //    //14 01 00 00 00 05 01 03 01 00 00 00 00 00 00 CC 28
                //    // NPC talk
                //    //14 01 00 00 00 06 01 03 01 00 00 00 00 00 00 CD 28

                //    //14 01 00 00 00 03 00 02 2C 27 01 01 00 00 00 00 00
                //    PacketCreator p2 = new PacketCreator();

                //    for (int i = 0; i < arr.Length; i++)
                //    {

                //    }

                //    p2 = new PacketCreator(0x14, 1);
                //    p2.addByte(0); p2.addByte(0); p2.addByte(0); p2.addByte(3); p2.addByte(0);
                //    p2.addByte(2);
                //    p2.addByte(0x2C); p2.addByte(27);

                //    p2.addByte(01);
                //    p2.addByte(01); p2.addByte(0); p2.addByte(0); p2.addByte(0);
                //    p2.add16(0);
                //    Console.WriteLine("Senddd click npc > " + String.Join(",", p2.getData()));

                //    //client.getChar().addPet(14106, 0, 0);
                //    client.reply(p2.send());

                //    //client.getChar().inventory.addItem(10168, 1, true);




                //    return;
                //}

            }
            else
            {
                int idDialog = 10666;
                PacketCreator p = new PacketCreator(0x14, 1);
                p.addByte(0); p.add16(0); p.addByte(0); p.addByte(1);
                client.unkIdNpc = (ushort)(PacketReader.read16(data, 1) + 2);
                p.add16(ushort.Parse((PacketReader.read16(data, 1) + 2).ToString()));
                p.add16(0); p.add16(0); p.add16(0);
                p.add16((ushort)idDialog);//you are hero :))

                Console.WriteLine("Unknown NPC > " + String.Join(",", p.getData()));
                client.reply(p.send());
            }

        }

        public void TalkQuestNpc(byte[] data, TSClient client)
        {
            //return;
            //if (client.idNpcTalking > 0 && client.selectMenu == 31)
            //{
            //    PacketCreator storage = new PacketCreator(0x1D, 06);
            //    client.reply(storage.send());
            //}
            if (client.idNpcTalking > 0)
                processStep(client);
            else
                client.continueMoving();
            //switch (client.selectMenu)
            //{
            //    case 30:
            //        {
            //            PacketCreator p2 = new PacketCreator();
            //            p2 = new PacketCreator(0x1D, 09);
            //            p2.addByte(0);
            //            client.reply(p2.send());
            //            p2 = new PacketCreator(0x1D, 04);
            //            uint my_gold_bank = client.getChar().gold_bank;
            //            byte[] gold_bank = convertIntToArrayByte(my_gold_bank);
            //            p2.addByte(gold_bank[0]);
            //            p2.addByte(gold_bank[1]);
            //            p2.addByte(gold_bank[2]);
            //            p2.addByte(gold_bank[3]);
            //            client.reply(p2.send());
            //            p2 = new PacketCreator(0x1D, 05);
            //            client.reply(p2.send());
            //            p2 = new PacketCreator(0x1D, 09);
            //            p2.addByte(0);
            //            client.reply(p2.send());
            //            break;
            //        }
            //    case 31:
            //        {
            //            PacketCreator p2 = new PacketCreator();
            //            p2 = new PacketCreator(0x1D, 06);
            //            client.reply(p2.send());
            //            break;
            //        }
            //    case 40:
            //        {
            //            Console.WriteLine("Come here stop talking");
            //            stopTalking(client);
            //            client.continueMoving();
            //            return;
            //        }
            //}
            //if (client.idNpcTalking > 0)
            //{
            //    if (idxDialog < currentStep.packageSend.Length - 1)
            //    {
            //        idxDialog++;
            //        List<DataTools.PackageSend> packages = currentStep.packageSend.ToList();
            //        byte[] packageToSend = packages.ElementAt(idxDialog).package;

            //        PacketCreator p2 = new PacketCreator();

            //        p2 = new PacketCreator(0x14, 1);
            //        p2.addByte(0); p2.addByte(packageToSend[0]); p2.addByte(packageToSend[1]); p2.addByte(packageToSend[2]); p2.addByte(packageToSend[3]);
            //        p2.addByte(packageToSend[4]);
            //        p2.addByte(packageToSend[5]); p2.addByte(packageToSend[6]);

            //        p2.addByte(packageToSend[7]);
            //        p2.addByte(packageToSend[8]); p2.addByte(packageToSend[9]); p2.addByte(packageToSend[10]); p2.addByte(packageToSend[11]);
            //        ushort idDialog_2 = (ushort)(PacketReader.read16(packageToSend, 12));

            //        p2.add16(idDialog_2);
            //        if (packageToSend[3] == 6 & packageToSend[4] == 3)
            //        {
            //            client.optionId = idDialog_2;
            //        }
            //        Console.WriteLine("Senddd click npc > sm " + String.Join(",", p2.getData()));

            //        client.reply(p2.send());
            //    }
            //    else
            //    {
            //        client.idNpcTalking = 0;
            //        idxDialog = 0;
            //        client.continueMoving();
            //    }

            //}
            //else
            //{
            //    client.continueMoving();
            //}

            //Console.WriteLine("Click NPC end ++ " + client.idNpcTalking);
            //Console.WriteLine("idxDialog end ++ " + idxDialog);


        }
        public void UImportant()
        {
            // Important
            reply(new PacketCreator(new byte[] { 0x18, 0x07, 0x03, 0x04 }).send());

            PacketCreator p = new PacketCreator(0x29);
            p.add8(0x05); p.add8(0x01); p.add8(0x01);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0x02000000); p.add32(0x00000001); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0x00000103); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0x00010400);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0x01050000); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add32(0); p.add32(0); p.add32(0);
            p.add32(0); p.add16(0); p.add8(0);
            reply(p.send());

            //UpdateMap2Npc();

            p = new PacketCreator(0x0B);
            p.add32(0xF24B0204); p.add32(0x00000001); p.add8(0);
            reply(p.send());
        }
        public void U0602()
        {
            reply(new PacketCreator(0x06, 0x06).send());
        }
        public void U1406()
        {
            reply(new PacketCreator(0x14, 6).send());
        }

        public void disconnect()
        {
            if (battle != null)
            {
                battle.outBattle(this);
            }
            if (online)
            {
                savetoDB();

                // Disappear
                var p = new PacketCreator(0x0D, 0x04);
                p.add32((uint)accID);
                chr.replyToMap(p.send(), false);
                p = new PacketCreator(0x01, 0x01);
                p.add32((uint)accID);
                chr.replyToMap(p.send(), false);

                map.listPlayers.Remove(accID);
                //map.removePlayer(accID);
                TSServer.getInstance().removePlayer(accID);
                online = false;
            }
        }
        public static string convertArrayByteToString(byte[] byte_0)
        {
            string text = "";
            checked
            {
                for (int i = 0; i < byte_0.Length; i++)
                {
                    byte b = byte_0[i];
                    text += b.ToString("X2");
                }
                return text;
            }
        }
        public static int convertArrayByteToInt(byte[] byte_0)
        {
            try
            {
                string str = convertArrayByteToString(new byte[] { byte_0[1], byte_0[0] });
                int result = Convert.ToInt32(str);
                return result;
            }
            catch
            {
                return -1;
            }

        }

    }
}
