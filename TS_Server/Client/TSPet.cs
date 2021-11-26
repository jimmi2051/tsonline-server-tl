using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TS_Server.DataTools;

namespace TS_Server.Client
{
    public class TSPet
    {
        public int pet_sid;
        public ushort NPCid;
        public byte[] name;
        public int hp, sp, mag, atk, def, agi, hpx, spx, hp_max, sp_max;
        public int hp2, sp2, mag2, atk2, def2, agi2;
        public uint totalxp;
        public double xp_pow;
        public int currentxp;
        public int skill_pt;
        public byte level, fai, reborn, skill1_lvl, skill2_lvl, skill3_lvl, skill4_lvl;        
        public TSEquipment[] equipment; // helm, armor, weapon, wrist, foot, special;
        public byte slot, location, quest;
        public TSCharacter owner;

        public TSPet(TSCharacter chr, byte sl)
        {
            owner = chr;
            slot = sl;
            location = 0;
            equipment = new TSEquipment[6];
            quest = 1;
        }

        public TSPet(TSCharacter chr, int sid, byte sl)
        {
            owner = chr;
            pet_sid = sid;
            slot = sl;
            location = 0;
            equipment = new TSEquipment[6];
            quest = 1;
        }

        public TSPet(TSCharacter chr, byte sl, byte _quest)
        {
            owner = chr;
            slot = sl;
            location = 0;
            equipment = new TSEquipment[6];
            quest = _quest;
        }

        public void loadPetDB()
        {
            //load db
            TSMysqlConnection c = new TSMysqlConnection();

            MySqlDataReader data = c.selectQuery("SELECT * FROM pet WHERE pet_sid = " + pet_sid);
            data.Read();

            NPCid = data.GetUInt16("npcid");
            name = (byte[])(data["name"]);
            level = data.GetByte("level");
            currentxp = data.GetInt32("exp");
            totalxp = data.GetUInt32("exp_tot");
            hp = data.GetInt32("hp"); sp = data.GetInt32("sp"); mag = data.GetInt32("mag"); atk = data.GetInt32("atk");
            def = data.GetInt32("def"); hpx = data.GetInt32("hpx"); spx = data.GetInt32("spx"); agi = data.GetInt32("agi");

            hp = Math.Max(1, hp); //prevent 0 HP if logout death in battle

            skill_pt = data.GetInt32("sk_point");
            fai = data.GetByte("fai");
            fai = 100;

            slot = data.GetByte("slot");

            location = data.GetByte("location");

            skill1_lvl = data.GetByte("sk1_lvl");
            skill2_lvl = data.GetByte("sk2_lvl");
            skill3_lvl = data.GetByte("sk3_lvl");
            skill4_lvl = data.GetByte("sk4_lvl");
            quest = data.GetByte("quest");

            var equip_data = (byte[])data["equip"];
            loadEquipment(equip_data);

            data.Close();
            c.connection.Close();

            reborn = NpcData.npcList[NPCid].reborn;
            hp_max = getHpMax();
            sp_max = getSpMax();
            xp_pow = reborn == 0 ? 2.9 : reborn == 1 ? 2.9 : 3.0;
        }

        public void initPet(NpcInfo n)
        {
            level = 1;
            NPCid = n.id;
            mag = n.mag; atk = n.atk; def = n.def; agi = n.agi; hpx = n.hpx; spx = n.spx;
            name = n.name;

            hp2 = 0; sp2 = 0; mag2 = 0; atk2 = 0; def2 = 0; agi2 = 0;
            skill_pt = 0;
            fai = 60; reborn = n.reborn;

            hp_max = getHpMax();
            sp_max = getSpMax();
            hp = hp_max; sp = sp_max;
            totalxp = 6;
            xp_pow = reborn == 0 ? 2.9 : reborn == 1 ? 2.9 : 3.0;

            skill1_lvl = 1; skill2_lvl = 1; skill3_lvl = 1; skill4_lvl = 0;

            TSMysqlConnection c = new TSMysqlConnection();
            c.connection.Open();
            savePetDB(c.connection, true);
            c.connection.Close();
        }

        public byte[] sendInfo()
        {
            var p = new PacketCreator();
            p.addByte((byte)(slot));
            p.add16(NPCid);
            p.add32(totalxp);
            p.addByte(level);
            p.add16((UInt16)hp);
            p.add16((UInt16)sp);
            p.add16((UInt16)mag);
            p.add16((UInt16)atk);
            p.add16((UInt16)def);
            p.add16((UInt16)agi);
            p.add16((UInt16)hpx);
            p.add16((UInt16)spx);
            p.addByte(1);
            p.addByte(fai);
            p.addByte(quest);
            p.add16((ushort)skill_pt);
            p.addByte((byte)name.Length);
            p.addBytes(name);
            p.addByte(skill1_lvl);
            p.addByte(skill2_lvl);
            p.addByte(skill3_lvl);

            for (int j = 0; j < 6; j++)
            {
                if (equipment[j] != null)
                {
                    p.add16(equipment[j].Itemid);
                    p.addByte(equipment[j].duration);
                    p.addZero(7);
                }
                else p.addZero(10);
            }
            p.addZero(6);
            return p.getData();
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

        public void refreshPet()
        {
            refreshBonus(); 

            refresh(hp, 0x19); refresh(sp, 0x1a); 
            //refresh(mag, 0x1b); refresh(atk, 0x1c); refresh(def, 0x1d); refresh(agi, 0x1e); refresh(hpx, 0x1f); refresh(spx, 0x20);

        }

        public void refreshBonus()
        {
            refresh(mag2, 0xd4); refresh(atk2, 0xd2); refresh(def2, 0xd3);
            refresh(hp2, 0xcf); refresh(sp2, 0xd0); refresh(agi2, 0xd6);
        }

        public void refresh(int prop, byte prop_code)
        {
            PacketCreator p = new PacketCreator(8, 2);
            p.addByte(4); p.addByte(slot); p.addByte(0);            
            p.addByte(prop_code);
            if (prop >= 0)
            { p.addByte(0x01); p.add32((UInt32)prop); }
            else
            { p.addByte(0x02); p.add32((UInt32)(-prop)); }
            p.add32(0);
            //Console.WriteLine("Receive Exp PET> " + String.Join(",", p.getData()));
            owner.reply(p.send());
        }

        public void refreshFull(byte prop_code, int prop1, int prop2)
        {
            var p = new PacketCreator(8, 2);
            p.addByte(4); p.addByte(slot); p.addByte(0);
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
            owner.reply(p.send());
        }
        public void sendNewPet()
        {
            refresh(hp, 0x19); refresh(sp, 0x1a);
            PacketCreator p1 = new PacketCreator(0x0f, 1);
            p1.add32(owner.client.accID);
            p1.addByte(slot); p1.add16(NPCid); p1.add16(0);
            
            p1.addByte(quest);
            owner.reply(p1.send());
            PacketCreator p2 = new PacketCreator(0x0f, 7);
            p2.add32(NPCid); p2.addByte(slot); p2.add16(NPCid);
            p2.addZero(7);
            
            p2.addByte(quest); p2.addByte((byte)name.Length);
            p2.addBytes(name);
            owner.reply(p2.send());
        }

        public void savePetDB(MySqlConnection conn, bool newPet)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            var c = new TSMysqlConnection();
            if (!newPet)
                cmd.CommandText = "UPDATE pet SET name = @name , charid = @charid , npcid = @npcid, level = @level , exp = @curr_exp, exp_tot = @exp_tot , hp = @hp , " + 
                "sp = @sp , mag = @mag , atk = @atk , def = @def , hpx = @hpx , spx = @spx , agi = @agi , sk_point = @sk_point , " +
                "fai = @fai , slot = @slot , location = @location , sk1_lvl = @sk1_lvl , sk2_lvl = @sk2_lvl , sk3_lvl = @sk3_lvl , " + 
                "sk4_lvl = @sk4_lvl, equip = @equip, quest = @quest WHERE pet_sid = @pet_sid";
            else
                cmd.CommandText = "INSERT INTO pet (name , charid , npcid, hp , sp , mag , atk , def , hpx , spx , agi , fai , slot , location, quest) " +
                    " VALUES (@name , @charid , @npcid, @hp , @sp , @mag , @atk , @def , @hpx , @spx , @agi , @fai , @slot , @location, @quest)";

            //convert name from client charset (TIS620 for Thai) back to UTF8 before save to DB

            cmd.Prepare();
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@charid", owner.charId);
            cmd.Parameters.AddWithValue("@npcid", NPCid);
            cmd.Parameters.AddWithValue("@hp", hp);
            cmd.Parameters.AddWithValue("@sp",sp);
            cmd.Parameters.AddWithValue("@mag",mag);
            cmd.Parameters.AddWithValue("@atk",atk);
            cmd.Parameters.AddWithValue("@def",def);
            cmd.Parameters.AddWithValue("@hpx",hpx);
            cmd.Parameters.AddWithValue("@spx",spx);
            cmd.Parameters.AddWithValue("@agi",agi);
            cmd.Parameters.AddWithValue("@fai",fai);
            cmd.Parameters.AddWithValue("@slot",slot);
            cmd.Parameters.AddWithValue("@location",location);
            cmd.Parameters.AddWithValue("@quest", quest);

            if (!newPet)
            {
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@curr_exp", currentxp);
                cmd.Parameters.AddWithValue("@exp_tot", totalxp);
                cmd.Parameters.AddWithValue("@sk_point", skill_pt);
                cmd.Parameters.AddWithValue("@sk1_lvl", skill1_lvl);
                cmd.Parameters.AddWithValue("@sk2_lvl", skill2_lvl);
                cmd.Parameters.AddWithValue("@sk3_lvl", skill3_lvl);
                cmd.Parameters.AddWithValue("@sk4_lvl", skill4_lvl);
                cmd.Parameters.AddWithValue("@equip", saveEquipment());
                cmd.Parameters.AddWithValue("@pet_sid", pet_sid);
                
            }

            cmd.ExecuteNonQuery();

            if (newPet)
                pet_sid = Convert.ToInt32(new MySqlCommand("SELECT LAST_INSERT_ID()", cmd.Connection).ExecuteScalar());
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
                    equipment[data[pos] - 1].pet_owner = this;
                    addEquipBonus(ItemData.itemList[itemid].prop1, ItemData.itemList[itemid].prop1_val, 0);
                    addEquipBonus(ItemData.itemList[itemid].prop2, ItemData.itemList[itemid].prop2_val, 0);
                    pos += 7;
                }
                else
                    break;
            }
        }

        public void setHp(int amount) //to do later : battle death
        {
            hp += amount;
            if (hp > hp_max)
                hp = hp_max;
            if (hp <= 0)
                if (owner.client.battle != null)
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

        public void setFai(int amount)
        {
            if (fai == 100)
                return;
            fai = (byte)(fai + amount);         
        }

        public int getHpMax()
        {
            if (reborn <= 1)
                return (int)Math.Round((Math.Pow(level, 0.35) + 1) * hpx * 2 + 80 + level);
            else
                return (int)Math.Round((Math.Pow(level, 0.35) + 2) * hpx * 2 + 180 + level); //need to update formula
        }

        public int getSpMax()
        {
            if (reborn <= 1)
                return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 60 + level);
            else
                return (int)Math.Round(Math.Pow(level, 0.25) * spx * 2 + 110 + level);
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
                    levelUp();
                    next_level_xp = (int)(Math.Pow(level + 1, xp_pow) + 5);
                }
            }
            else if (currentxp < 0) currentxp = 0;
            refresh((int)totalxp, 0x24);
        }

        public void levelUp() //0x24 = totxp, 0x23  =lvl, 0x25 = sk_point 0x26 = stt_point
        {
            level++;
            skill_pt += 1;
            setFai(1);
            hp_max = getHpMax();
            sp_max = getSpMax();
            hp = hp_max;
            sp = sp_max;
            refresh(hp, 0x19);
            refresh(sp, 0x1a); 
            refresh(level, 0x23);
            refresh(skill_pt, 0x25);
            getSttPoint();
        }

        public void getSttPoint()
        {
            int totstat = mag + atk + def + hpx + spx + agi;
            int randomNumber = RandomGen.getInt(0, totstat);
            if (randomNumber < mag)
            {
                mag++; refresh(mag, 0x1b);
            }
            else if (randomNumber < mag + atk)
            {
                atk++; refresh(atk, 0x1c);
            }
            else if (randomNumber < mag + atk + def)
            {
                def++; refresh(def, 0x1d);
            }
            else if (randomNumber < mag + atk + def + hpx)
            {
                hpx++; refresh(hpx, 0x1f); hp_max = getHpMax();
            }
            else if (randomNumber < mag + atk + def + hpx + spx)
            {
                spx++; refresh(spx, 0x20); sp_max = getSpMax();
            }
            else
            {
                agi++; refresh(agi, 0x1e);
            }
        }

        public void setSkill(ushort skillid, byte sk_lvl)
        {
            if (skillid == NpcData.npcList[NPCid].skill1)
            {
                if (sk_lvl - skill1_lvl > skill_pt) return;
                skill_pt -= (sk_lvl - skill1_lvl);
                skill1_lvl = sk_lvl;
            }
            else if (skillid == NpcData.npcList[NPCid].skill2)
            {
                if (sk_lvl - skill2_lvl > skill_pt) return;
                skill_pt -= (sk_lvl - skill2_lvl);
                skill2_lvl = sk_lvl;
            }
            else if (skillid == NpcData.npcList[NPCid].skill3)
            {
                if (sk_lvl - skill3_lvl > skill_pt) return;
                skill_pt -= (sk_lvl - skill3_lvl);
                skill3_lvl = sk_lvl;
            }
            else if (skillid == NpcData.npcList[NPCid].skill4)
            {
                if (sk_lvl - skill4_lvl > skill_pt) return;
                skill_pt -= (sk_lvl - skill4_lvl);
                skill4_lvl = sk_lvl;
            }
            else return;
            refresh(skill_pt, 0x25);
            refreshFull(0x6e, sk_lvl, skillid);            
        }
    }
}
