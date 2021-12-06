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
            if (packageToSend[3] == 0 & packageToSend[4] == 3 & packageToSend[6] == 0 & packageToSend[7] == 3)
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
            Console.WriteLine("Senddd click npc > " + String.Join(",", p2.getData()));
            client.reply(p2.send());
            client.idxDialog++;

        }
        public void ClickkNpc(byte[] data, TSClient client)
        {
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
                List<DataTools.Step> steps = DataTools.EveData.listStepOnMap[client.map.mapid].FindAll(item => item.npcIdInMap == id_talking);
                DataTools.Step step = steps[1];
                currentStep = step;
                processStep(client);

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
            //int idDialog = 10666;
            //int idDialog2 = DataTools.EveData.listNpcOnMap[client.map.mapid][index].idDialog;
            //if (index > -1 && idDialog2 > 10000 & idDialog2 < 65000)
            //{
            //    idDialog = idDialog2;
            //    //client.step = 0;
            //}


            //PacketCreator p = new PacketCreator(0x14, 1);
            //p.addByte(0); p.add16(0); p.addByte(0); p.addByte(1);
            //client.unkIdNpc = (ushort)(PacketReader.read16(data, 1) + 2);
            //p.add16(ushort.Parse((PacketReader.read16(data, 1) + 2).ToString()));
            //p.add16(0); p.add16(0); p.add16(0);
            //p.add16((ushort)idDialog);//you are hero :))

            //Console.WriteLine("Senddd click npc > " + String.Join(",", p.getData()));
            //client.reply(p.send());
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
