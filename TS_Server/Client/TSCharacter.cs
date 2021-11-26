using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using MySql.Data.MySqlClient;
using TS_Server.DataTools;
using TS_Server;
using TS_Server.Server;
using TS_Server.Client;

namespace TS_Server.Client
{
    public class TSCharacter
    {
        public byte orient;
        public ushort horseID = 0;
        public TSParty party;
        public int agi;
        public int agi2;
        public int atk;
        public int atk2;
        public TSItemContainer bag, storage;
        public int charId;
        public TSClient client;
        public uint color1, color2;
        public int def;
        public int def2;
        public byte element;
        public TSEquipment[] equipment; // หมวก, ชุด, อาวุธ, มือ, เท้า, เพิ่มเติม;
        public byte face;
        public byte ghost, god;
        public uint gold, gold_bank;
        public byte hair;
        public uint honor;
        public int hp;
        public int hp2;
        public int hp_max;
        public int hpx;
        public TSItemContainer inventory;
        public byte job, level;
        public int mag;
        public int mag2;
        public ushort mapID, mapX, mapY;
        public byte[] name;
        public byte nb_equips;
        public int next_item;
        public int next_pet;
        public TSPet[] pet;
        public sbyte pet_battle;
        public TSPet[] pet_car;
        public TSPet[] pet_inn;
        public byte rb;
        public byte sex;
        public Dictionary<ushort, byte> skill;
        public int skill_point;
        public int sp;
        public int sp2;
        public int sp_max;
        public int spx;
        public int stt_point;
        public byte style;
        public int currentxp;
        public uint totalxp;
        public double xp_pow;
        public ushort[] hotkey;
        public byte ball_point;
        public bool[] ballList;
        public ushort[] skill_rb2;
        public ushort outfitId = 0;

        public TSCharacter(TSClient c)
        {
            client = c;
            pet = new TSPet[4];
            next_pet = 0;
            pet_battle = -1;
            equipment = new TSEquipment[6];
            inventory = new TSItemContainer(this, 25);
            bag = new TSItemContainer(this, 25);
            storage = new TSItemContainer(this, 50);
            hotkey = new ushort[10];
            ballList = new bool[12];
            skill_rb2 = new ushort[8];
            next_item = 0;
            nb_equips = 0;
            skill = new Dictionary<ushort,byte>();
        }

        public void loadCharDB()
        {
            //load db
            var c = new TSMysqlConnection();

            MySqlDataReader data = c.selectQuery("SELECT * FROM chars WHERE accountid = " + client.accID);
            data.Read();

            charId = data.GetInt32("id");
            level = data.GetByte("level");
            hp = data.GetInt32("hp");
            hp = Math.Max(1, hp); // 0 HP ออกจากระบบถ้าตายในการสู้รบ
            sp = data.GetInt32("sp");
            mag = data.GetInt32("mag");
            atk = data.GetInt32("atk");
            def = data.GetInt32("def");
            hpx = data.GetInt32("hpx");
            spx = data.GetInt32("spx");
            agi = data.GetInt32("agi");

            hp2 = data.GetInt32("hp2");
            sp2 = data.GetInt32("sp2");
            mag2 = data.GetInt32("mag2");
            atk2 = data.GetInt32("atk2");
            def2 = data.GetInt32("def2");
            agi2 = data.GetInt32("agi2");
            skill_point = data.GetInt32("sk_point");
            stt_point = data.GetInt32("stt_point");

            sex = data.GetByte("sex");
            ghost = data.GetByte("ghost");
            god = data.GetByte("god");
            style = data.GetByte("style");
            hair = data.GetByte("hair");
            face = data.GetByte("face");

            color1 = data.GetUInt32("color1");
            color2 = data.GetUInt32("color2");
            mapID = data.GetUInt16("map_id");
            mapX = data.GetUInt16("map_x");
            mapY = data.GetUInt16("map_y");

            currentxp = data.GetInt32("exp");
            totalxp = data.GetUInt32("exp_tot");
            honor = data.GetUInt32("honor");

            element = data.GetByte("element");
            rb = data.GetByte("reborn");
            job = data.GetByte("job");

            gold = data.GetUInt32("gold");
            gold_bank = data.GetUInt32("gold_bank");


            //name = data.GetString("name");
            name = (byte[])(data["name"]);

            var equip_data = (byte[])data["equip"];
            loadEquipment(equip_data);

            var inventory_data = (byte[])data["inventory"];
            inventory.loadContainer(inventory_data);

            var storage_data = (byte[])data["storage"];
            storage.loadContainer(storage_data);

            var bag_data = (byte[])data["bag"];
            bag.loadContainer(bag_data);
            var skill_data = (byte[])data["skill"];
            loadSkill(skill_data);

            var hotkey_data = (byte[])data["hotkey"];
            loadHotkey(hotkey_data);

            pet_battle = (sbyte)data.GetByte("pet_battle");

            data.Close();
            c.connection.Close();

            hp_max = getHpMax();
            sp_max = getSpMax();
            xp_pow = rb == 0 ? 2.9 : rb == 1 ? 3.0 : 3.05;

            if (!skill.ContainsKey(14001))
                skill.Add(14001, 1);

            if (!skill.ContainsKey(14015))
                skill.Add(14015, 10);

            if (!skill.ContainsKey(14021))
                skill.Add(14021, 5);

            if (!skill.ContainsKey(14023))
                skill.Add(14023, 1);

            //skill_point = 0;
            //mag = Math.Max(mag, 300);
            //atk = Math.Max(atk, 300);
            //def = Math.Max(def, 300);
            //agi = Math.Max(agi, 300);
            //stt_point = 50;
            //rb = 2;
            //job = 1;
            //hpx = 0;
            //level = 1;

            loadPet();
        }

        public void loadPet()
        {
            var c = new TSMysqlConnection();

            MySqlDataReader data = c.selectQuery("SELECT pet_sid, slot, location FROM pet WHERE charid = " + charId);
            while (data.Read())
            {
                int s = data.GetInt32("slot");
                int sid = data.GetInt32("pet_sid");
                pet[s - 1] = new TSPet(this, sid, (byte)s);
                pet[s - 1].loadPetDB();
            }
            data.Close();
            c.connection.Close();

            while (next_pet < 4)
            {
                if (pet[next_pet] == null) break;
                next_pet++;
            }
        }

        public void initChar(byte[] data, byte[] name)
        {
            //update pass1 pass2
            string pass1 = PacketReader.readString(data, 22, data[21]);
            string pass2 = PacketReader.readString(data, 22 + pass1.Length + 1, data[22 + pass1.Length]);

            var c = new TSMysqlConnection();

            c.updateQuery("UPDATE account SET password = '" + pass1 + "', password2 = '" + pass2 + "' WHERE id = " +
                          client.accID);
            c.connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = c.connection;
            cmd.CommandText = "INSERT INTO chars (accountid, name, mag, atk, def, hpx, spx, agi, sex, style, hair, face, color1, color2, element) "
                          + "VALUES (" + client.accID + ", @name ," + data[15] + "," + data[16] + "," + data[17] +
                          "," + data[18] + ","
                          + data[19] + "," + data[20] + "," + data[2] + "," + data[3] + "," + data[4] + "," + data[5] +
                          "," + PacketReader.read32(data, 6) + "," + PacketReader.read32(data, 10) + "," + data[14] +
                          ");";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
            c.connection.Close();

            charId = c.getLastId("chars");
        }

        public void loginChar()
        {
            loadCharDB();
            addSummonSkill(10);

            client.online = true; 
            
            refreshChr();

            reply(new PacketCreator(new byte[] { 0x14, 0x08 }).send());
            reply(new PacketCreator(new byte[] { 0x14, 0x21, 0x00 }).send());

            sendLook(false);
            sendInfo();
            sendPetInfo();

            reply(new PacketCreator(new byte[] { 0x21, 2, 0, 0 }).send());

            //0x17, 5 for invent, 0x1e, 0x1 for storage, 0x017, 0x2f for bag
            inventory.sendItems(0x17, 5);
            bag.sendItems(0x17, 0x2f);
            storage.sendItems(0x1e,1);
            sendEquip();

            client.UImportant();
            client.AllowMove();
            
            sendGold();
            announce("สวัสดีจร้า");
            sendHotkey();
            sendVoucher();

            refreshFull(0x6f, 1, 1); //???? ที่เพิ่ม?

            TSServer.getInstance().addPlayer(client);
            sendUpdateTeam();
        }

        public void addPet(ushort npcid, int bonus, byte quest) //สัตว์เลี้ยง
        {
            Console.WriteLine(next_pet + " " + npcid);
            for (int i = 0; i < next_pet; i++)
                if (pet[i].NPCid == npcid) return;
            if (next_pet < 4 && NpcData.npcList.ContainsKey(npcid))
            {
                pet[next_pet] = new TSPet(this, (byte)(next_pet + 1), quest);
                pet[next_pet].initPet(NpcData.npcList[npcid]);                            
                Console.WriteLine("Pet id " + npcid + ", sid " + pet[next_pet].pet_sid + " added in slot " + (next_pet + 1) + " Quest " + (pet[next_pet].quest));
                pet[next_pet].sendNewPet();
                for (int i = 0; i < bonus; i++)
                    pet[next_pet].getSttPoint();
                nextPet();
            }
        }
        public void changePetName(byte slot, byte[] newName)
        {
            if (pet[slot - 1] == null) return;
            var c = new TSMysqlConnection();

            c.connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = c.connection;
            cmd.CommandText = "UPDATE pet SET `name` = @name WHERE pet_sid=" + pet[slot - 1].pet_sid;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
            c.connection.Close();

            pet[slot - 1].name = newName;

            PacketCreator p = new PacketCreator(0xf, 9);
            p.add32(client.accID);
            p.add8(slot);
            p.addBytes(pet[slot - 1].name);
            reply(p.send());
        }
        public void removePet(byte slot)
        {
            if (pet[slot - 1] != null)
            {
                var c = new TSMysqlConnection();

                c.updateQuery("DELETE FROM pet WHERE pet_sid=" + pet[slot - 1].pet_sid);

                pet[slot - 1] = null;

                if (pet_battle == slot - 1) pet_battle = -1;
                nextPet();

                PacketCreator p = new PacketCreator(0xf, 2);
                p.add32(client.accID);
                p.add8(slot);
                reply(p.send());
            }
        }

        public void nextPet()
        {
            next_pet = 0;
            while (next_pet < 4)
                if (pet[next_pet] != null)
                    next_pet++;
                else break;
        }

        public void addEquipBonus(ushort prop, int prop_val, int type) //type 0 : equip on, type 1 : unequip
        {
            int val = type == 0 ? prop_val : -prop_val;
            switch (prop)
            {
                case 207:
                    hp2 += val;
                    break;
                case 208:
                    sp2 += val;
                    break;
                case 210:
                    atk2 += val;
                    break;
                case 211:
                    def2 += val;
                    break;
                case 212:
                    mag2 += val;
                    break;
                case 214:
                    agi2 += val;
                    break;
                default:
                    break;
            }
        }

        public void sendLook(bool forReborn)
        {
            var p = new PacketCreator(3);
            p.add32(client.accID);
            p.addByte(sex);
            p.addByte(ghost);
            p.addByte(god);
            p.add16(mapID);
            p.add16(mapX);
            p.add16(mapY);
            p.addByte(style);
            p.addByte(hair);
            p.addByte(face);
            p.add32(color1);
            p.add32(color2);
            p.addByte(nb_equips);
            for (int i = 0; i < 6; i++)
                if (equipment[i] != null) p.add16(equipment[i].Itemid);
            p.add32(0);
            p.addByte(5);
            p.addByte(rb);
            p.addByte(job);
            if (!forReborn)
                p.addBytes(name);
            reply(p.send());
        }

        public byte[] sendLookForOther()
        {
            var p = new PacketCreator(0x03);
            p.add32(client.accID);
            p.addByte(sex);
            p.addByte(element);
            p.addByte(level);
            p.addByte(ghost);
            p.addByte(god);
            p.add16(mapID); 
            p.add16(mapX); 
            p.add16(mapY); 
            p.addByte(style); 
            p.addByte(hair);
            p.addByte(face); 
            p.add32(color1);
            p.add32(color2); 
            p.addByte(nb_equips);
            for (int i = 0; i < 6; i++)
                if (equipment[i] != null) p.add16(equipment[i].Itemid);

            p.add32(0);
            p.add16(0); //p.addByte(5);
            p.addByte(rb);
            p.addByte(job);
            p.addBytes(name);

            return p.send();
        }

        public byte[] setExpress(byte expressType, byte expressCode)
        {
            var p = new PacketCreator(0x20);
            p.add8(expressType);
            p.add32(client.accID);
            p.add8(expressCode);

            return p.send();
        }

        public void sendInfo()
        {
            var p = new PacketCreator(5, 3);
            p.addByte(element);
            p.add16((UInt16)hp);
            p.add16((UInt16)sp);
            p.add16((UInt16)mag);
            p.add16((UInt16)atk);
            p.add16((UInt16)def);
            p.add16((UInt16)agi);
            p.add16((UInt16)hpx);
            p.add16((UInt16)spx);
            p.addByte(level);
            p.add32(totalxp);
            p.add16((UInt16)skill_point);
            p.add16((UInt16)stt_point);
            p.add32(honor);
            p.add16((UInt16)hp_max);
            p.add16((UInt16)sp_max);
            p.add32((UInt32)atk2); 
            p.add32((UInt32)def2); 
            p.add32((UInt32)mag2);  
            p.add32((UInt32)agi2);
            p.add32((UInt32)hp2);
            p.add32((UInt32)sp2);
            //ค่ายทหาร
            p.add16(500);
            p.add16(500);
            p.add16(500);
            p.add16(500);
            p.add16(500);
            //ปฏิบัติ ฯลฯ หักบัญชี
            p.addZero(0x2B);

            foreach (ushort s in skill.Keys)
            {
                p.add16(s);
                p.addByte(skill[s]);
            }

            reply(p.send());

            //บอลจุติ info
            if (rb == 2)
                sendBallList();
        }

        public void sendBallList()
        {
            PacketCreator p = new PacketCreator(0x17, 0x4d);
            p.add8(ball_point);
            for (int i = 0; i < 12; i++)
                if (ballList[i])
                    p.add8((byte)(i + 1));
            reply(p.send());

            PacketCreator p1 = new PacketCreator(0x17, 0x4e);
            for (int i = 0; i < 8; i++)
                if (skill_rb2[i] != 0)
                {
                    p1.add8((byte)(i + 1));
                    p1.add16(skill_rb2[i]);
                }

            reply(p1.send());
        }

        public void sendUpdateTeam()
        {
            if (isTeamLeader())
            {
                var p = new PacketCreator(0x0D);
                p.add8(6);
                p.add32((uint)client.accID);
                p.add8((byte)(party.member.Count-1));

                foreach (TSCharacter c in party.member)
                {
                    c.refreshTeam();
                    if (c.client.accID != party.leader_id)
                        p.add32((uint)c.client.accID);
                }
                replyToMap(p.send(), true);
            }

            var p1 = new PacketCreator(0x0f);
            p1.add8(0x07);
            p1.add32((uint)client.accID);
            if (pet != null)
            {
                for (int i = 0; i < pet.Length; i++)
                {
                    //&& pet[i].NPCid != horseID
                    if (pet[i] != null)
                    {
                        p1.addByte((byte)(i + 1));
                        p1.add16(pet[i].NPCid);
                        p1.addZero(7);
                        p1.add8(0x01);
                        p1.addByte((byte)pet[i].name.Length);
                        p1.addBytes(pet[i].name);
                    }
                }
            }
            replyToMap(p1.send(), false);

            // Update horse ride (อัพเวลาขี่ม้า)
            if (horseID > 0)
            {
                rideHorse(true, horseID);
            }
            else
            {
                rideHorse(false);
            }
        }
        public void sendPetInfo()
        {
            var p1 = new PacketCreator(0x0f, 8);
            for (int i = 0; i < pet.Length; i++)
                if (pet[i] != null)
                    p1.addBytes(pet[i].sendInfo());
            reply(p1.send());

            //สัตว์เลี้ยงในรถ
            reply(new PacketCreator(new byte[] { 0x0f, 0x14, 1, 0, 0 }).send());
            reply(new PacketCreator(new byte[] { 0x0f, 0x14, 2, 0, 0 }).send());
            reply(new PacketCreator(new byte[] { 0x0f, 0x14, 3, 0, 0 }).send());
            reply(new PacketCreator(new byte[] { 0x0f, 0x14, 4, 0, 0 }).send());

            reply(new PacketCreator(new byte[] { 0x0f, 0x0a }).send());

            //สัตว์เลี้ยงในโรงแรม
            reply(new PacketCreator(new byte[] { 0x0f, 0x12, 1, 0, 0, 2, 0, 0, 3, 0, 0, 4, 0, 0 }).send());

            reply(new PacketCreator(new byte[] { 0x0f, 0x13, 1, 0 }).send());
            if (pet_battle != -1)
            {
                var p2 = new PacketCreator(0x13);
                p2.addByte(1);
                p2.add16(pet[pet_battle].NPCid);
                p2.add16(0);
                reply(p2.send());
            }

            if (pet != null)
                for (int i = 0; i < pet.Length; i++)
                    if (pet[i] != null)
                        pet[i].refreshPet();
        }

        public void sendEquip()
        {
            var p = new PacketCreator(0x17, 0x0b);

            for (int i = 0; i < 6; i++)
                if (equipment[i] != null)
                {
                    p.add16(equipment[i].Itemid);
                    p.addByte(equipment[i].duration);
                    p.addZero(7);
                }
            reply(p.send());
        }

        public void sendGold()
        {
            var p = new PacketCreator(0x1a, 4);
            p.add32(gold);
            p.add32(gold_bank);
            reply(p.send());
        }

        public void sendHotkey()
        {
            var p = new PacketCreator(0x28, 1);

            for (byte i = 1; i <= 10; i++)
                if (hotkey[i - 1] != 0)
                {
                    p.add8(2);
                    p.add16(hotkey[i - 1]);
                    p.add8(i);
                }
            reply(p.send());
        }

        public void sendVoucher()
        {
            var p = new PacketCreator(0x23, 4);
            p.add32(999999);
            p.addZero(12);
            reply(p.send());
        }

        public void refreshChr()
        {
            refresh(hpx, 0x1f);
            refresh(spx, 0x20);
            refresh(atk, 0x1c);
            refresh(def, 0x1d);
            refresh(mag, 0x1b);
            refresh(agi, 0x1e);
            refresh(hp, 0x19);
            refresh(sp, 0x1a);

            refreshBonus();
        }
        public void showOutfit()
        {
            if (!NpcData.npcList.ContainsKey(outfitId)) return;
                PacketCreator p = new PacketCreator( 5, 5 );
                p.add32(client.accID);
                p.add16(outfitId);
                replyToMap(p.send(), true);
        }
        public void refreshBonus()
        {
            refresh(mag2, 0xd4);
            refresh(atk2, 0xd2);
            refresh(def2, 0xd3);
            refresh(hp2, 0xcf);
            refresh(sp2, 0xd0);
            refresh(agi2, 0xd6);
        }

        public void refresh(int prop, byte prop_code, bool team = false)
        {
           
            var p = new PacketCreator(0x08);

            if (party != null && team)
            {
                p.addByte(0x03);
                p.add32((uint)client.accID);
            }
            else
                p.addByte(0x01);

            p.addByte(prop_code);
            if (prop >= 0)
            {
                p.addByte(0x01);
                p.add32((UInt32)prop);
            }
            else
            {
                p.addByte(0x02);
                p.add32((UInt32)(-prop));
            }
            p.add32(0);
            //Console.WriteLine("Receive Exp CHAR> " + String.Join(",", p.getData()));
            if (party != null && team)
                replyToTeam(p.send());
            else
                reply(p.send());

        }

        public void refreshTeam()
        {
            refresh(hpx, 0x1f, true);
            refresh(spx, 0x20, true);
            refresh(atk, 0x1c, true);
            refresh(def, 0x1d, true);
            refresh(mag, 0x1b, true);
            refresh(agi, 0x1e, true);
            refresh(hp, 0x19, true);
            refresh(sp, 0x1a, true);

            refresh(mag2, 0xd4, true);
            refresh(atk2, 0xd2, true);
            refresh(def2, 0xd3, true);
            refresh(hp2, 0xcf, true);
            refresh(sp2, 0xd0, true);
            refresh(agi2, 0xd6, true);
        }

        //รุ่นที่สมบูรณ์มากขึ้นของการตอบสนองต่อการฟื้นฟู
        public void refreshFull(byte prop_code, int prop1, int prop2)
        {
            var p = new PacketCreator(8, 1);
            p.addByte(prop_code);
            if (prop1 >= 0)
            {
                p.addByte(0x01);
                p.add32((UInt32)prop1);
            }
            else
            {
                p.addByte(0x02);
                p.add32((UInt32)(-prop1));
            }
            p.add32((UInt32)prop2);

            reply(p.send());
        }

        public void announce(string msg)
        {
            var p = new PacketCreator(2, 0x0b);
            p.add32(0);
            p.addString(msg);
            reply(p.send());
        }

        public void saveCharDB(MySqlConnection conn)
        {
            var cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText =
                "UPDATE chars SET level = @level , exp = @curr_exp, exp_tot = @exp_tot , hp = @hp , sp = @sp , mag = @mag , atk = @atk," +
                "def = @def , hpx = @hpx , spx = @spx , agi = @agi , sk_point = @sk_point , stt_point = @stt_point," +
                "ghost = @ghost , god = @god , map_id = @map_id , map_x = @map_x , map_y = @map_y , gold = @gold , " +
                "gold_bank = @gold_bank , honor = @honor , pet_battle = @pet_battle, equip = @equip, inventory = @inventory, bag = @bag, storage = @storage, " +
                "skill = @skill, hotkey = @hotkey, reborn = @rb, job = @job WHERE accountid = @id";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@level", level);
            cmd.Parameters.AddWithValue("@curr_exp", currentxp);
            cmd.Parameters.AddWithValue("@exp_tot", totalxp);
            cmd.Parameters.AddWithValue("@hp", hp);
            cmd.Parameters.AddWithValue("@sp", sp);
            cmd.Parameters.AddWithValue("@mag", mag);
            cmd.Parameters.AddWithValue("@atk", atk);
            cmd.Parameters.AddWithValue("@def", def);
            cmd.Parameters.AddWithValue("@hpx", hpx);
            cmd.Parameters.AddWithValue("@spx", spx);
            cmd.Parameters.AddWithValue("@agi", agi);
            cmd.Parameters.AddWithValue("@sk_point", skill_point);
            cmd.Parameters.AddWithValue("@stt_point", stt_point);
            cmd.Parameters.AddWithValue("@ghost", ghost);
            cmd.Parameters.AddWithValue("@god", god);
            cmd.Parameters.AddWithValue("@map_id", mapID);
            cmd.Parameters.AddWithValue("@map_x", mapX);
            cmd.Parameters.AddWithValue("@map_y", mapY);
            cmd.Parameters.AddWithValue("@gold", gold);
            cmd.Parameters.AddWithValue("@gold_bank", gold_bank);
            cmd.Parameters.AddWithValue("@honor", honor);
            cmd.Parameters.AddWithValue("@pet_battle", pet_battle);
            cmd.Parameters.AddWithValue("@id", client.accID);
            cmd.Parameters.AddWithValue("@equip", saveEquipment());
            cmd.Parameters.AddWithValue("@inventory", inventory.saveContainer());
            cmd.Parameters.AddWithValue("@bag", bag.saveContainer());
            cmd.Parameters.AddWithValue("@storage", storage.saveContainer());
            cmd.Parameters.AddWithValue("@skill", saveSkill());
            cmd.Parameters.AddWithValue("@hotkey", saveHotkey());
            cmd.Parameters.AddWithValue("@rb", rb);
            cmd.Parameters.AddWithValue("@job", job);
            cmd.ExecuteNonQuery();
        }

        public byte[] saveEquipment()
        {
            var data = new byte[100];
            int pos = 0;
            for (int i = 0; i < 6; i++)
                if (equipment[i] != null)
                    equipment[i].generateEquipBinary(ref data, ref pos);
            return data;
        }

        public void loadEquipment(byte[] data)
        {
            int pos = 0;
            ushort itemid;
            while (pos < data.Length)
            {
                if (data[pos] != 0)
                {
                    itemid = (ushort)(data[pos + 1] + (data[pos + 2] << 8));
                    equipment[data[pos] - 1] = new TSEquipment(null, itemid, data[pos], 1);
                    equipment[data[pos] - 1].equip.duration = data[pos + 3];
                    equipment[data[pos] - 1].equip.elem_type = data[pos + 4];
                    equipment[data[pos] - 1].equip.elem_val = data[pos + 5] + (data[pos + 6] << 8);
                    equipment[data[pos] - 1].char_owner = this;
                    nb_equips++;
                    addEquipBonus(ItemData.itemList[itemid].prop1, ItemData.itemList[itemid].prop1_val, 0);
                    addEquipBonus(ItemData.itemList[itemid].prop2, ItemData.itemList[itemid].prop2_val, 0);
                    pos += 7;
                }
                else
                    break;
            }
        }

        public byte[] saveSkill()
        {
            var data = new byte[600];
            int pos = 0;
            foreach (ushort s in skill.Keys)
            {
                data[pos] = (byte)s;
                data[pos + 1] = (byte)(s >> 8);
                data[pos + 2] = skill[s];
                pos += 3;
            }
            if (rb == 2)
            {
                data[pos] = 0xff;
                data[pos + 1] = 0xff;
                data[pos + 2] = ball_point;
                pos += 3;
                for (int i = 0; i < 12; i++)
                    if (ballList[i])
                    {
                        data[pos] = (byte)(i + 1);
                        pos++;
                    }
                data[pos] = 0xff;
                pos++;
                for (int i = 0; i < 8; i++)
                    if (skill_rb2[i] != 0)
                    {
                        data[pos] = (byte)(i + 6);
                        data[pos + 1] = (byte)skill_rb2[i];
                        data[pos + 2] = (byte)(skill_rb2[i] >> 8);
                        pos += 3;
                    }
            }

            return data;
        }

        public void loadSkill(byte[] data)
        {
            int pos = 0;
            ushort sk_id;

            if (data.Length < 3) return;

            while (pos < data.Length)
            {
                sk_id = (ushort)(data[pos] + (data[pos + 1] << 8));
                if (sk_id != 0 && sk_id != 0xffff)
                {
                    skill.Add(sk_id, data[pos + 2]);
                    pos += 3;
                }
                else if (sk_id == 0xffff)
                {
                    ball_point = data[pos + 2];
                    pos += 3;
                    while (data[pos] != 0xff && data[pos] != 0)
                    {
                        ballList[data[pos] - 1] = true;
                        pos++;
                    }
                    pos++;
                    while (data[pos] != 0)
                    {
                        skill_rb2[data[pos] - 6] = PacketReader.read16(data, pos + 1);
                        pos += 3;
                    }
                    break;
                }
                else
                    break;
            }
        }

        public byte[] saveHotkey()
        {
            var data = new byte[30];
            int pos = 0;
            for (byte i = 1; i <= 10; i++)
                if (hotkey[i - 1] != 0)
                {
                    data[pos] = i;
                    data[pos + 1] = (byte)hotkey[i - 1];
                    data[pos + 2] = (byte)(hotkey[i - 1] >> 8);
                    pos += 3;
                }
            return data;
        }

        public void loadHotkey(byte[] data)
        {
            int pos = 0;
            while (pos < data.Length)
            {
                if (data[pos] != 0)
                {
                    hotkey[data[pos] - 1] = (ushort)(data[pos + 1] + (data[pos + 2] << 8));
                    pos += 3;
                }
                else break;
            }
        }

        public void reply(byte[] data)
        {
            if (client.online)
                client.reply(data);
        }
        public void replyToMap(byte[] data, bool self)
        {
            client.map.BroadCast(this.client, data, self);
        }
        public void replyToAll(byte[] data, bool self)
        {
            foreach (TSMap m in TSWorld.getInstance().listMap.Values)
            {
                m.BroadCast(client, data, self);
            }
        }
        public void replyToTeam(byte[] data)
        {
            foreach (TSCharacter c in party.member)
            {
                c.reply(data);
            }
        }

        public bool isTeamLeader()
        {
            if (party == null)
                return false;
            else 
            {
                if (party.leader_id == client.accID)
                    return true;
                else
                    return false;
            }
        }

        public bool isJoinedTeam()
        {
            if (party == null)
                return false;
            else return true;
        }

        public void setHp(int amount)
        {
            hp += amount;
            if (hp > hp_max)
                hp = hp_max;
            if (hp <= 0)
                if (client.battle != null)
                    hp = 0;
                else hp = 1;
        }

        public void setSp(int amount)
        {
            sp += amount;
            if (sp > sp_max)
                sp = sp_max;
            if (sp < 0) sp = 0;
        }

        public int getHpMax()
        {
            if (rb == 0)
                return (int)Math.Round((Math.Pow(level, 0.35) + 1) * hpx * 2 + 80 + level);
            else if (rb == 1)
                return (int)Math.Round((Math.Pow(level, 0.35) + 2) * hpx * 2 + 180 + level);
            else
            {
                if (job == 1)
                    return (int)Math.Round((Math.Pow(level, 0.35)*2 + 25) * hpx + 280 + level);
                else if (job == 2)
                    return (int)Math.Round((Math.Pow(level, 0.35) * 3 + 30) * hpx + 380 + level);
                else if (job == 3)
                    return (int)Math.Round((Math.Pow(level, 0.35) + 11.5) * hpx * 2 + 180 + level);
                else
                    return (int)Math.Round((Math.Pow(level, 0.35) + 10.5) * hpx * 2 + 180 + level);
            }
        }

        public int getSpMax()
        {
            if (rb == 0)
                return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 60 + level);
            else if (rb == 1)
                return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 110 + level);
            else
            {
                if (job == 1)
                    return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 160 + level);
                else if (job == 2)
                    return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 160 + level);
                else if (job == 3)
                    return (int)Math.Round(Math.Pow(level, 0.25) * spx * 3 + 310 + level);
                else
                    return (int)Math.Round(Math.Pow(level, 0.25) * spx * 3.5 + 410 + level);
            }
        }

        public void setExp(int amount)
        {
            if (level >= 200) return;

            totalxp = (uint)(totalxp + amount);
            currentxp += amount;

            if (amount > 0)
            {
                int next_level_xp = (int)(Math.Pow(level + 1, xp_pow) + 5);
                while (currentxp >= next_level_xp)
                {
                    currentxp -= next_level_xp;
                    if (level >= 200) return;
                    levelUp();
                    next_level_xp = (int)(Math.Pow(level + 1, xp_pow) + 5);
                }
            }
            else if (currentxp < 0) currentxp = 0;
            refresh((int)totalxp, 0x24);
        }

        public void levelUp() //0x24 = totxp, 0x23  =lvl, 0x25 = sk_point 0x26 = stt_point
        {
            if (level >= 200) return;
            level++;
            stt_point += 2;
            skill_point += 1;
            hp_max = getHpMax();
            sp_max = getSpMax();
            hp = hp_max;
            sp = sp_max;
            refresh(level, 0x23);
            refresh(skill_point, 0x25);
            refresh(stt_point, 0x26);
            refresh(hp, 0x19);
            refresh(sp, 0x1a);
        }

        public void setStat(byte prop_code, int val)
        {
            switch (prop_code)
            {
                case 0x1b:
                    checkSetStat(ref mag, prop_code, val);
                    break;
                case 0x1c:
                    checkSetStat(ref atk, prop_code, val);
                    break;
                case 0x1d:
                    checkSetStat(ref def, prop_code, val);
                    break;
                case 0x1e:
                    checkSetStat(ref agi, prop_code, val);
                    break;
                case 0x1f:
                    checkSetStat(ref hpx, prop_code, val);
                    hp_max = getHpMax();
                    break;
                case 0x20:
                    checkSetStat(ref spx, prop_code, val);     
                    sp_max = getSpMax();
                    break;
            }
        }

        public void checkSetStat(ref int prop, byte prop_code, int val) //รหัสสถิติการตั้งค่าในภายหลัง
        {
            if (val > prop + 1 || stt_point == 0)
                return;
            prop++;
            stt_point--;
            refresh(stt_point, 0x26);
            refresh(prop, prop_code);
        }

        public void setSkill(ushort skillid, byte sk_lvl)
        {
            if (SkillData.skillList.ContainsKey(skillid) && skill_point > 0) //รีเซ็ตรหัสทักษะในภายหลัง
            {
                SkillInfo s = SkillData.skillList[skillid];

                int skillpt_needed;
                bool newskill;

                if (skill.ContainsKey(skillid))
                {
                    skillpt_needed = sk_lvl - skill[skillid];
                    newskill = false;
                }
                else if (s.require_sk == 0 || skill.ContainsKey(s.require_sk) || s.id == 13014)
                {
                    if (SkillData.skillList[skillid].elem != element) 
                        skillpt_needed = s.sk_point * 2 + sk_lvl - 1;
                    else
                        skillpt_needed = s.sk_point + sk_lvl - 1;
                    newskill = true;
                }
                else return; 

                if (skillpt_needed > 0 && skill_point >= skillpt_needed)
                {
                    if (newskill) 
                        skill.Add(skillid, sk_lvl);
                    else skill[skillid] = sk_lvl;
                    skill_point -= skillpt_needed;
                    refresh(skill_point, 0x25);
                    refreshFull(0x6e, sk_lvl, skillid);
                }
            }
        }

        public void setSkillRb2(byte[] data)
        {
            int pos = 2;
            uint ball_use = PacketReader.read32(data, pos);
            if (ball_point < ball_use) return;
            ball_point -= (byte)ball_use;
            pos += 4;
            for (int i = 0; i < ball_use; i++)
                ballList[data[pos + i] - 1] = true;

            pos += (int)ball_use;
            uint nbskill = PacketReader.read32(data, pos);
            pos += 4;
            for (int i = 0; i < nbskill; i++)
            {
                setSkill(PacketReader.read16(data,pos),data[pos + 2]);
                pos += 3;
            }

            uint skill_place = PacketReader.read32(data, pos) / 3;
            pos += 4;
            for (int i = 0; i < skill_place; i++)
            {
                skill_rb2[data[pos] - 6] = PacketReader.read16(data, pos + 1);
                pos += 3;
            }

            sendBallList();
        }

        public bool setBattlePet(ushort npcid)
        {
            for (int i = 0; i < 4; i++)
                if (pet[i] != null)
                    if (pet[i].NPCid == npcid)
                    {
                        pet_battle = (sbyte)i;
                        return true;
                    }
            return false;
        }

        public bool unsetBattlePet()
        {
            if (pet_battle != -1)
            {
                pet_battle = -1;
                return true;
            }
            return false;
        }

        public void rebornChar(byte nb_reborn, byte j)
        {
            if (level < 120) return;
            if (rb != nb_reborn - 1) return;
            if (rb == 2 && (j < 1 || j > 4)) return;
            if (nb_equips > 0) return;

            rb++;
            if (rb == 2) job = j;
            stt_point = 6 + (int)(level / (10 / nb_reborn));
            skill_point = (int)(level / (4 / nb_reborn));
            atk = 0; mag = 0; def = 0; agi = 0; hpx = 0; spx = 0;
            level = 1;

            totalxp = 0;
            currentxp = 0;
            hp_max = getHpMax(); hp = hp_max;
            sp_max = getSpMax(); sp = sp_max;
            honor = 0;

            foreach (ushort sk_id in skill.Keys)
            {
                refreshFull(0x6e, 0, sk_id);
            }

            skill.Clear();
            skill.Add(14001, 1);
            skill.Add(14015, 10);
            skill.Add(14021, 5);
            skill.Add(14023, 1);
            skill.Add(14035, 1);

            refresh(stt_point, 0x26);
            refresh(skill_point, 0x25);
            refresh((int)totalxp, 0x24);
            refresh(level, 0x23);
            refreshChr();
            sendLook(true);
            sendInfo();

        }

        public bool checkPetReborn(byte nb_reborn) //ตรวจสอบว่ามีสัตว์เลี้ยงที่มีสิทธิ์ในการเกิดใหม่

        {
            int rb_prop = nb_reborn == 1 ? 65 : 67;
            for (int i = 0; i < 4; i++)
                if (pet[i] != null)
                    if (pet[i].reborn == nb_reborn - 1 && pet[i].level >= nb_reborn * 30 && pet[i].fai >= nb_reborn * 40 + 20)
                    {
                        ushort rb_item = 0;  //locket or star
                        foreach (ItemInfo it in ItemData.itemList.Values)
                            if (it.prop1 == rb_prop && it.prop1_val == pet[i].NPCid)
                            {
                                rb_item = it.id;
                                break;
                            }
                        if (rb_item != 0)
                            if (inventory.getItemById(rb_item) != 25)
                                return true;
                    }
            return false;
        }

        public void rebornPet(byte nb_reborn, byte slot)
        {
            int rb_prop = nb_reborn == 1 ? 65 : 67;
            ushort rb_item = 0; //locket or star
            ushort pet_rb = 0;
            byte item_slot = 25;
            foreach (ItemInfo i in ItemData.itemList.Values)
                if (i.prop1 == rb_prop && i.prop1_val == pet[slot - 1].NPCid)
                {
                    rb_item = i.id;
                    pet_rb = (ushort)i.prop2_val;
                    break;
                }
            if (rb_item == 0) return;            

            item_slot = inventory.getItemById(rb_item);
            if (item_slot == 25) return;

            inventory.dropItem((byte)(item_slot + 1), 1);

            int stt_point_bonus = (int)((pet[pet_battle].level) / (nb_reborn * 2));
            removePet(slot);
            // Pet not quest
            addPet(pet_rb, stt_point_bonus, 1);
        }

        public void rideHorse(bool ride, ushort horseid = 0)
        {
            if (ride)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (pet[i] != null)
                        if (pet[i].NPCid == horseid)
                        {
                            if (NpcData.npcList[horseid].type == 9)
                            {
                                PacketCreator p = new PacketCreator(0xf, 5);
                                p.add32(this.client.accID);
                                p.add16(horseid);
                                p.addZero(6);
                                replyToMap(p.send(), true);
                                break;
                            }
                        }
                }
                horseID = horseid;
            }
            else
            {
                PacketCreator p1 = new PacketCreator(0xf, 6);
                p1.add32(this.client.accID);
                p1.addZero(2);
                replyToMap(p1.send(), true);
                horseID = 0;
            }            
        }

        public void setCharElement(byte element)
        {
            var c = new TSMysqlConnection();

            c.updateQuery("UPDATE chars SET `element` = " + element + " WHERE id=" + charId);
            this.element = element;
        }
        public void sleep()
        {            
            reply(new PacketCreator(0x1f, 0xa).send());

            setHp(1000000);
            refresh(hp, 0x19);
            setSp(1000000);
            refresh(sp, 0x1a);
            for (int i = 0; i < 4; i++)
                if (pet[i] != null)
                {
                    pet[i].setHp(100000);
                    pet[i].refresh(pet[i].hp, 0x19);
                    pet[i].setSp(100000);
                    pet[i].refresh(pet[i].sp, 0x1a);
                }

            client.reply(new PacketCreator(new byte[] { 0x1f, 1, 0 }).send());

        }

        public void addSummonSkill(byte level)
        {
            if (skill.ContainsKey(14026))
            {
                return;
            }
            skill.Add(14026, level);
        }

        public void addSummonSkill()
        {
            if (skill.ContainsKey(14026))
            {
                switch (element)
                {
                    case 1:
                        skill.Add(10016, skill[14026]);
                        refreshFull(0x6e, skill[14026], 10016);
                        break;
                    case 2:
                        skill.Add(11016, skill[14026]);
                        refreshFull(0x6e, skill[14026], 11016);
                        break;
                    case 3:
                        skill.Add(12016, skill[14026]);
                        refreshFull(0x6e, skill[14026], 12016);
                        break;  
                    case 4:
                        skill.Add(13015, skill[14026]);
                        refreshFull(0x6e, skill[14026], 13015);
                        break;  
                    default:
                        break;
                }
            }
        }

        public void removeSummonSkill()
        {
            if (skill.ContainsKey(14026))
            {
                switch (element)
                {
                    case 1:
                        skill.Remove(10016);
                        refreshFull(0x6e, 0, 10016);
                        break;
                    case 2:
                        skill.Remove(11016);
                        refreshFull(0x6e, 0, 11016);
                        break;
                    case 3:
                        skill.Remove(12016);
                        refreshFull(0x6e, 0, 12016);
                        break;
                    case 4:
                        skill.Remove(13015);
                        refreshFull(0x6e, 0, 13015);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}