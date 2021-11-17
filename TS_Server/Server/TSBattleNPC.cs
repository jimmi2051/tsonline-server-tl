using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.DataTools;
using TS_Server.Client;
using System.Timers;
using TS_Server.Server.BattleClasses;

namespace TS_Server.Server
{
    public class TSBattleNPC : BattleAbstract
    {
        public ushort npcmapid = 65000; //pk NPC only

        public TSBattleNPC(TSClient c, byte type, ushort npc_mapid, ushort[] listNPC) : base(c,type)
        {
            initAlly(c);

            npcmapid = npc_mapid;
            initNPCs(listNPC);

            start_round();
        }

        public void initAlly(TSClient c) //rewrite later for party
        {
            PacketCreator p = new PacketCreator();
            p.addBytes(announceStart(c.getChar(), 3, 2, 3, null));

            if (c.getChar().party != null)
            {
                if (c.getChar().party.member.Count > 1) p.addBytes(announceStart(c.getChar().party.member[1], 3, 1, 5, p));
                if (c.getChar().party.member.Count > 2) p.addBytes(announceStart(c.getChar().party.member[2], 3, 3, 5, p));
                if (c.getChar().party.member.Count > 3) p.addBytes(announceStart(c.getChar().party.member[3], 3, 0, 5, p));
                if (c.getChar().party.member.Count > 4) p.addBytes(announceStart(c.getChar().party.member[4], 3, 4, 5, p));
            }
        }

        public byte[] announceStart(TSCharacter c, byte row, byte col, byte type, PacketCreator prefix)
        {
            if (prefix != null)
                c.client.battle = this;

            position[row][col].charIn(c);
            countAlly++;

            PacketCreator ret = position[row][col].announce(type, 0); //get the battle info of char
            byte[] ret_array = ret.getData();

            PacketCreator p = new PacketCreator(0xb, 0xfa); //announce battle
            //ushort rand1 = RandomGen.getUShort(0, 65535);
            //Console.WriteLine("combat gen " + rand1);
            p.add8(0x70); p.add8(0);
            p.addBytes(ret_array);
            if (prefix != null)
                p.addBytes(prefix.getData()); //prefix contains info for next char in party

            c.reply(p.send());

            c.reply(new PacketCreator(new byte[] { 0xb, 0xa, 1 }).send());

            if (prefix != null)
            {
                PacketCreator p_announce = new PacketCreator(0xb, 5);
                p_announce.addBytes(ret_array);
                byte[] announce = p_announce.send();
                for (int i = 0; i < 5; i++)
                    if (position[3][i].exist && i != col)
                        position[3][i].chr.reply(announce);
            }

            if (c.pet_battle != -1)
            {
                position[row - 1][col].petIn(c.pet[c.pet_battle]);
                countAlly++;

                TSPet pet = c.pet[c.pet_battle];
                PacketCreator ret1 = position[row - 1][col].announce(type, countAlly);
                byte[] ret1_array = ret1.getData();

                PacketCreator p1 = new PacketCreator(0xb, 5);
                p1.addBytes(ret1_array);
                c.reply(p1.send());

                if (prefix != null)
                {
                    PacketCreator p_announce = new PacketCreator(0xb, 5);
                    p_announce.addBytes(ret1_array);
                    byte[] announce = p_announce.send();
                    for (int i = 0; i < 5; i++)
                        if (position[3][i].exist && i != col)
                            position[3][i].chr.reply(announce);
                }

                ret.addBytes(ret1_array);
            }

            return ret.getData(); //will be used as prefix for next char in party
        }

        public void initNPCs(ushort[] listNPC)
        {
            for (ushort i = 0; i < listNPC.Length; i++)
            {
                byte r = (byte)(i % 2);
                byte c = (byte)(i / 2);
                if (listNPC[i] != 0)
                {
                    countEnemy++;

                    BattleNpcAI ai = new BattleNpcAI(this, countEnemy, listNPC[i]);
                    position[r][c].npcIn(ai);

                    PacketCreator p = new PacketCreator(0xb, 5);
                    BattleParticipant bp = position[r][c];
                    //note sure, but 3 3 = pk with npcmapid, 3 7 = gate, 1 7 = pk with npcid
                    p.addBytes(bp.announce(3, npcmapid != 65000 ? npcmapid : bp.npc.count).getData());
                    battleBroadcast(p.send()); 
                }
            }
        }

        public override void start_round()
        {
            System.Threading.Thread.Sleep(300);

            //prepare for new round
            for (byte i = 0; i < 4; i++)
                for (byte j = 0; j < 5; j++)
                {
                    position[i][j].updateStatus();
                    position[i][j].alreadyCommand = false;

                    // drop
                    if (i < 2 && position[i][j].npc != null)
                        if (position[i][j].npc.drop != 0)
                        {
                            foreach (Tuple<byte, byte> k in position[i][j].npc.killer)
                                giveDrop(position[i][j].npc.drop, (byte)i, (byte)j, k.Item1, k.Item2);
                            position[i][j].npc.drop = 0;
                        }
                }

            System.Threading.Thread.Sleep(200);
            Console.WriteLine("New round, ally = " + countAlly + ", enemy = " + countEnemy + ", disabled " + countDisabled);
            cmdReg.Clear();
            cmdNeeded = countAlly + countEnemy - countDisabled;
            Console.WriteLine("cmd needed " + cmdNeeded);

            if (finish != 0)
            {
                endBattle(finish == 1);
                return;
            }

            battleBroadcast(new PacketCreator(0x34, 1).send());

            for (byte i = 0; i < 4; i++)
                for (byte j = 0; j < 5; j++)
                {
                    if (position[i][j].disable == 0 &&
                        (position[i][j].debuff_type == 14021 || position[i][j].debuff_type == 20014))
                    {
                        Console.WriteLine("Confuse on " + i + " " + j);
                        int r = i < 2 ? 0 : 2;
                        byte i1 = 0;
                        byte j1 = 0;
                        do
                        {
                            i1 = RandomGen.getByte(0, 2);
                            j1 = RandomGen.getByte(0, 5);
                        } while (!position[i1 + r][j1].exist || position[i1 + r][j1].death);
                        pushCommand(i, j, (byte)(i1 + r), (byte)(j1 + r), 0, 10000);
                        battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, i, j }).send());
                    }
                }

            aTimer.Start();
            giveCommandAI();
        }

        public void giveCommandAI()
        {
            for (byte i = 0; i < 2; i++)
                for (byte j = 0; j < 5; j++)
                    if (position[i][j].type == 3 && position[i][j].disable == 0 && !position[i][j].death && position[i][j].debuff_type != 14021 && position[i][j].exist)
                    {
                        //pushCommand(i, j, i, j, 17001); //def 
                        seekTarget(i, j);
                    }
        }

        public void seekTarget(byte row, byte col) //AI in position (row, col) seeking for its target :)))
        {
            byte dest_row = 5;
            byte dest_col = 5;
            int skill = 10000;


            ushort id = (ushort)position[row][col].npc.npcid;
            int rand = RandomGen.getInt(0, 100);

            // 20% skill4, else 25% skill 3, else 33% skill 2, else 50% skill1, else attack
            if (NpcData.npcList[id].skill4 != 0 && rand % 5 == 0)
                skill = NpcData.npcList[id].skill4;
            else if (NpcData.npcList[id].skill3 != 0 && rand % 4 == 0)
                skill = NpcData.npcList[id].skill3;
            else if (NpcData.npcList[id].skill2 != 0 && rand % 3 == 0)
                skill = NpcData.npcList[id].skill2;
            else if (NpcData.npcList[id].skill1 != 0 && rand % 2 == 0)
                skill = NpcData.npcList[id].skill1;

            byte r = 2;
            byte nb_row = 2;
            byte type = SkillData.skillList[(ushort)skill].skill_type;
            if (type == 4 || type == 6 || type == 7 || type == 14 || type == 19) r = 0;
            if (type == 5 || type == 18) { nb_row = 4; r = 0; }

            if (type != 8)
                while (dest_col == 5)
                {
                    rand = RandomGen.getInt(0, 5 * nb_row);
                    if (position[rand % nb_row + r][rand / nb_row].exist && !position[rand % nb_row + r][rand / nb_row].death)
                    {
                        dest_row = (byte)(rand % nb_row + r);
                        dest_col = (byte)(rand / nb_row);
                        break;
                    }
                }
            else
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 5; j++)
                        if (position[i][j].exist && position[i][j].death)
                        {
                            dest_row = (byte)(i);
                            dest_col = (byte)(j);
                            break;
                        }
            if (dest_row == 5) { dest_row = 0; dest_col = 0; }

            if (skill == 10016 || skill == 11016 || skill == 12016 || skill == 13015) //NPC trieu goi lvl 10
                skill += 3;

            pushCommand(row, col, dest_row, dest_col, 0, (ushort)skill);
        }

        public override void checkDeath(byte row, byte col, BattleCommand c)
        {
            if (position[row][col].getHp() <= 0)
            {
                BattleParticipant bp = position[row][col];
                if (!bp.death)
                {
                    if (row < 2)
                    {
                        countEnemy--;
                        bp.npc.drop = bp.npc.generateDrop();
                        if (c.init_row >= 2) bp.npc.killer.Add(new Tuple<byte, byte>(c.init_row, c.init_col));
                        if (countEnemy == 0) finish = position[3][2].death ? 2 : 1;
                    }
                    else
                    {
                        countAlly--;
                        if (countAlly == 0) finish = 2;
                    }
                    bp.death = true;
                    bp.purge_type = 3;
                    //bp.purge_status();
                }
                else if ((row < 2) && (c.init_row >= 2)) bp.npc.killer.Add(new Tuple<byte, byte>(c.init_row, c.init_col));
            }
            else if ((row < 2) && (c.init_row >= 2)) position[row][col].npc.attacker.Add(new Tuple<byte, byte>(c.init_row, c.init_col));
        }

        public void giveDrop(ushort itemid, byte init_row, byte init_col, byte dest_row, byte dest_col)
        {
            PacketCreator p = new PacketCreator(0x35, 4);
            p.add16(itemid);
            p.add8(init_row); p.add8(init_col);
            p.add8(dest_row); p.add8(dest_col);

            if (position[dest_row][dest_col].type == 1)
            {
                position[dest_row][dest_col].chr.inventory.addItem(itemid, 1,true);
                battleBroadcast(p.send());
            }
            else if (position[dest_row][dest_col].type == 2)
            {
                position[dest_row][dest_col].pet.owner.inventory.addItem(itemid, 1,true);
                battleBroadcast(p.send());
            }
        }

        public override void endBattle(bool win)
        {
            int[,] exp_gain = new int[2, 5];
            int exp_rate = 10000;

            System.Threading.Thread.Sleep(100);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (position[i][j].exist && position[i][j].type == 3)
                    {
                        foreach (Tuple<byte, byte> k in position[i][j].npc.attacker)
                            exp_gain[k.Item1 - 2, k.Item2] += position[i][j].npc.level * exp_rate / 4;
                        foreach (Tuple<byte, byte> k in position[i][j].npc.killer)
                            exp_gain[k.Item1 - 2, k.Item2] += position[i][j].npc.level * exp_rate;
                    }

                    if (position[i][j].exist && position[i][j].type == 1)
                    {
                        TSCharacter c = position[i][j].chr;
                        if (!c.isJoinedTeam() || c.isTeamLeader())
                        {
                            // Clear battle smoke
                            var p = new PacketCreator(0x0B);
                            p.add8(0); p.add32(c.client.accID); p.add16(0);
                            c.replyToMap(p.send(), false);
                        }
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (position[3][i].exist)
                {
                    TSCharacter chr = position[3][i].chr;

                    if (chr.hp == 0)
                    {
                        chr.hp = 1;
                        // lost xp
                        chr.refresh(1, 0x19);
                    }
                    else
                    {
                        //gain xp;
                        chr.setExp(exp_gain[1, i]);
                    }
                    for (int j = 0; j < 4; j++)
                        if (chr.pet[j] != null)
                        {
                            if (chr.pet[j].hp <= 0)
                            {
                                chr.pet[j].hp = 1;
                                //chr.pet[j].fai--;
                                //lost xp
                                chr.pet[j].refresh(1, 0x19);
                            }
                            else if (j == chr.pet_battle)
                            {
                                //gain xp
                                chr.pet[j].setExp(exp_gain[0, i]);
                            }
                        }

                    chr.client.battle = null;

                    PacketCreator p = new PacketCreator(0xb, 0);
                    p.add32(chr.client.accID);
                    p.add16(0); //win / lose ?
                    battleBroadcast(p.send());
                    battleBroadcast(new PacketCreator(new byte[] { 0xb, 1, 3, (byte)i, 0 }).send()); //char walk out

                    chr.client.continueMoving();
                }
            }
            Console.WriteLine("Battle has ended");
        }
    }
}
