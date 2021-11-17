using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TS_Server.Client;
using TS_Server.Server.BattleClasses;

namespace TS_Server.Server
{
    class TSBattlePvp : BattleAbstract
    {
        public ushort npcmapid = 65000; //pk NPC only
        public TSClient t1;
        public TSClient t2;

        public TSBattlePvp(TSClient c, byte type, ushort npc_mapid, TSClient enemy)
            : base(c, type)
        {
            t1 = c;
            t2 = enemy;
            enemy.battle = this;
            initTeam1(c);

            npcmapid = npc_mapid;
            initTeam2(enemy);

            for (int i = 0; i < 5; i++)
            {

                if (position[0][i].chr != null)
                {
                    PacketCreator p = new PacketCreator(0xb, 5);
                    BattleParticipant bp = position[0][i];
                    //note sure, but 3 3 = pk with npcmapid, 3 7 = gate, 1 7 = pk with npcid
                    p.addBytes(bp.announce(1, 65000).getData());
                    battleBroadcast(p.send());
                }

                if (position[3][i].chr != null)
                {
                    PacketCreator p = new PacketCreator(0xb, 5);
                    BattleParticipant bp = position[3][i];
                    //note sure, but 3 3 = pk with npcmapid, 3 7 = gate, 1 7 = pk with npcid
                    p.addBytes(bp.announce(1, 65000).getData());
                    battleBroadcast(p.send());
                }
            }
            start_round();
        }

        private void initTeam2(TSClient c)
        {
            PacketCreator p = new PacketCreator();
            p.addBytes(announceStart(c.getChar(), 0, 2, 3, null));

            if (c.getChar().party != null)
            {
                if (c.getChar().party.member.Count > 1) p.addBytes(announceStart(c.getChar().party.member[1], 0, 1, 5, p));
                if (c.getChar().party.member.Count > 2) p.addBytes(announceStart(c.getChar().party.member[2], 0, 3, 5, p));
                if (c.getChar().party.member.Count > 3) p.addBytes(announceStart(c.getChar().party.member[3], 0, 0, 5, p));
                if (c.getChar().party.member.Count > 4) p.addBytes(announceStart(c.getChar().party.member[4], 0, 4, 5, p));
            }


        }

        private void initTeam1(TSClient c)
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
            if (row < 2)
                countAlly++;
            else countEnemy++;

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
                    if (position[row][i].exist && i != col)
                        position[row][i].chr.reply(announce);
            }

            if (c.pet_battle != -1)
            {
                int petPos = row == 3 ? 2 : 1;
                position[petPos][col].petIn(c.pet[c.pet_battle]);
                if (row < 2)
                    countAlly++;
                else countEnemy++;
                

                TSPet pet = c.pet[c.pet_battle];
                PacketCreator ret1 = position[petPos][col].announce(type, row < 2 ? countEnemy : countAlly);
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
                        if (position[row][i].exist && i != col)
                            position[row][i].chr.reply(announce);
                }

                ret.addBytes(ret1_array);
            }

            return ret.getData(); //will be used as prefix for next char in party
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
        }

        public override void checkDeath(byte row, byte col, BattleClasses.BattleCommand c)
        {
            if (position[row][col].getHp() <= 0)
            {
                BattleParticipant bp = position[row][col];
                if (!bp.death)
                {
                    if (row < 2)
                    {
                        countEnemy--;
                        if (countEnemy == 0) finish = position[3][2].death ? 2 : 1;
                    }
                    else
                    {
                        countAlly--;
                        if (countAlly == 0) finish = 2;
                    }
                    bp.death = true;
                    bp.purge_type = 3;
                    bp.purge_status();
                }
            }
        }

        public override void endBattle(bool win)
        {

            System.Threading.Thread.Sleep(1000);

            for (byte r = 0; r < 4; r+=3)
                for (byte c = 0; c < 5; c++)
            {
                if (position[r][c].exist && position[r][c].type == 1)
                {
                    TSCharacter chr = position[r][c].chr;

                    if (chr.hp == 0)
                    {
                        chr.hp = 1;
                        // lost xp
                        chr.refresh(1, 0x19);
                    }

                    for (int j = 0; j < 4; j++)
                        if (chr.pet[j] != null)
                        {
                            if (chr.pet[j].hp <= 0)
                            {
                                chr.pet[j].hp = 1;
                                chr.pet[j].refresh(1, 0x19);
                            }
                        }

                    chr.client.battle = null;

                    if (!chr.isJoinedTeam() || chr.isTeamLeader())
                    {
                        // Clear battle smoke
                        var p = new PacketCreator(0x0B);
                        p.add8(0); p.add32(chr.client.accID); p.add16(0);
                        chr.replyToMap(p.send(), false);
                    }

                    PacketCreator p1 = new PacketCreator(0xb, 0);
                    p1.add32(chr.client.accID);
                    p1.add16(0); //win / lose ?
                    battleBroadcast(p1.send());
                    battleBroadcast(new PacketCreator(new byte[] { 0xb, 1, r, c, 0 }).send()); //char walk out

                    chr.client.continueMoving();
                }
            }
            Console.WriteLine("Battle has ended");
        }
    }
}
