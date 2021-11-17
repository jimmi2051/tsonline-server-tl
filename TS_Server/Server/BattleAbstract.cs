using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TS_Server.Server.BattleClasses;
using TS_Server.Client;
using TS_Server.DataTools;

namespace TS_Server.Server
{
    public abstract class BattleAbstract
    {
        public TSMap map;
        public BattleParticipant[][] position;
        public List<BattleCommand> cmdReg;
        public int cmdNeeded;
        public int nextcmd;
        public int countDisabled = 0;
        public ushort countEnemy = 0;
        public ushort countAlly = 0;
        public byte battle_type;
        public int finish = 0;
        public Timer aTimer = new Timer(21000);

        protected BattleAbstract()
        {
        }

        protected BattleAbstract(TSClient c, byte type)
        {
            c.battle = this;
            map = c.map;
            map.announceBattle(c);
            battle_type = type;

            position = new BattleParticipant[4][];
            for (byte i = 0; i < 4; i++)
            {
                position[i] = new BattleParticipant[5];
                for (byte j = 0; j < 5; j++)
                    position[i][j] = new BattleParticipant(this, i, j);
            }

            aTimer.Elapsed += new ElapsedEventHandler(timeOut);

            cmdReg = new List<BattleCommand>();
        }

        public abstract void start_round();

        public abstract void checkDeath(byte row, byte col, BattleCommand c);

        public abstract void endBattle(bool win);

        public void timeOut(object sender, EventArgs e)
        {
            if (countAlly == 0)
            {
                endBattle(false);
                aTimer.Dispose(); // Fix
            }
            else
            {
                execute();
            }
        }

        public void registerCommand(TSClient c, byte[] data, byte type)
        {
            if (!aTimer.Enabled || position[data[2]][data[3]].alreadyCommand) return;

            battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, data[2], data[3] }).send());
            Console.WriteLine("receive cmd " + data[2] + " " + data[3]);

            pushCommand(data[2], data[3], data[4], data[5], type, PacketReader.read16(data, 6));
        }

        public void pushCommand(byte init_row, byte init_col, byte dest_row, byte dest_col, byte type, ushort command_id)
        {
            BattleCommand cmd = new BattleCommand(init_row, init_col, dest_row, dest_col, type);

            position[init_row][init_col].alreadyCommand = true;

            if (type == 0)
            {
                cmd.skill = command_id;
                cmd.skill_lvl = position[init_row][init_col].getSkillLvl(command_id);
                if (command_id == 13015 || command_id == 10016 || command_id == 11016 || command_id == 12016) //Trieu goi
                {
                    cmd.skill += (ushort)((cmd.skill_lvl - 1) / 3);
                }
                //cmd.dmg = calcDmg(init_row, init_col, cmd.skill, cmd.skill_lvl); //calculate base dmg here
                cmd.dmg = CalcDmg(init_row, init_col, dest_row, dest_col, cmd.skill, cmd.skill_lvl); //calculate base dmg here


            }
            if (type == 1) // use item
            {
                cmd.skill = command_id;
                if (ItemData.itemList[command_id].type != 16)
                    cmd.dmg = (ushort)ItemData.itemList[command_id].prop1_val;
                else
                {
                    cmd.type = 2; // bua` ngai? =))
                    cmd.dmg = CalcDmg(init_row, init_col, dest_row, dest_col, ItemData.itemList[command_id].unk9, 1);
                }
            }

            cmd.priority = position[init_row][init_col].getAgi();

            if (cmd.skill == 17001) //def
            {
                //to do : handle def      
                position[cmd.init_row][cmd.init_col].def = true;
                cmdNeeded--;
            }
            else
            {
                int pos = 0;
                while (pos < cmdReg.Count) //find the proper order to place the BattleCommand
                {
                    if (cmd.priority <= cmdReg[pos].priority) pos++;
                    else break;
                }
                lock (cmdReg)
                    cmdReg.Insert(pos, cmd);
                cmdNeeded--;
            }

            if (cmdNeeded == 0) execute();
        }

        public ushort CalcDmg(byte initRow, byte initCol, byte destRow, byte destCol, ushort skill, byte skill_lvl)
        {
            int dmgbase;
            BattleParticipant init = position[initRow][initCol];
            BattleParticipant dest = position[destRow][destCol];
            int initLvl = init.getLvl();
            int initEle = init.getElem();
            int initAtk = init.getAtk();
            int initMag = init.getMag();


            byte skillGrade = SkillData.skillList[skill].grade;
            byte skillRb = SkillData.skillList[skill].unk20;
            int skillBaseDmg = (int)(skillGrade * 15 * skillRb * (1 + 0.2 * (skill_lvl)));

            // Normal atk
            if (skill == 10000)
            {
                dmgbase = (int)(initAtk * 1.6);
                skillBaseDmg = initLvl * 2;
            }
            else if (SkillData.skillList[skill].unk17 == 1) // Skill atk
            {
                dmgbase = (int)(initAtk * 1.6 + initMag * 0.4);

            }
            else // Int
            {
                dmgbase = (int)(initMag * 2);
            }
            dmgbase = (int)((dmgbase + initLvl) * (1 + (skillGrade * 0.1 * skillRb) + skill_lvl * 0.02)) + skillBaseDmg;

            if (dmgbase > 50000) dmgbase = 50000;
            return (ushort)dmgbase;
        }

        public ushort calcDmg(byte row, byte col, ushort skill, byte skill_lvl)
        {
            //int dmgbase = (int)(position[row][col].getAtk()*0.2 + position[row][col].getMag()*0.2);
            //dmgbase = (int)(dmgbase * (SkillData.skillList[skill].grade + 0.1 * skill_lvl));
            //dmgbase = (int)(dmgbase * Math.Pow(position[row][col].getLvl(),0.3));

            //int dmgbase = (int)((2 * position[row][col].getLvl() + 10) / 2.5 * (SkillData.skillList[skill].grade + 0.1 * skill_lvl));
            //dmgbase = (int)(dmgbase * (position[row][col].getAtk() * 0.2 + position[row][col].getMag() * 0.2));

            int dmgbase;
            if (SkillData.skillList[skill].unk17 == 1 || skill == 10000)
                dmgbase = position[row][col].getLvl() + position[row][col].getAtk();
            else
                dmgbase = (int)(position[row][col].getLvl() + position[row][col].getMag() * 0.75);
            dmgbase = (int)(dmgbase * (SkillData.skillList[skill].grade + 0.1 * skill_lvl));

            if (dmgbase > 50000) dmgbase = 50000;
            return (ushort)dmgbase;
        }

        public void execute() // spartannnnnn!!!!!
        {
            BattleParticipant init, dest;
            nextcmd = 0;

            aTimer.Stop();
            System.Threading.Thread.Sleep(500);

            while (nextcmd < cmdReg.Count)
            {
                if (finish != 0) break;

                try
                {
                    BattleCommand cmd = cmdReg[nextcmd];
                    init = position[cmd.init_row][cmd.init_col];
                    dest = position[cmd.dest_row][cmd.dest_col];

                    if (init.exist && init.disable == 0 && !init.death)
                    {

                        if (!dest.exist || dest.death || dest.buff_type == 13005 || dest.buff_type == 13025) //auto choose another target if former one not available
                        {
                            if (changeTarget(cmd)) // target is changable
                                execCommand(cmd);
                            else nextcmd++;
                        }
                        else
                            execCommand(cmd);
                    }
                    else nextcmd++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    finish = 2;
                }
            }

            start_round();
        }

        public bool changeTarget(BattleCommand c)
        {
            if (position[c.dest_row][c.dest_col].exist)
            {
                if (sameSide(c.init_row, c.dest_row)) return true;
                if (c.type == 1) return true;
                if (SkillData.skillList[c.skill].skill_type > 2) return true;
                if (c.type == 2)
                    if (SkillData.skillList[ItemData.itemList[c.skill].unk9].skill_type > 2) return true;

            }

            int r = c.dest_row < 2 ? 0 : 2;
            for (int i = 0; i < 10; i++)
                if (position[i % 2 + r][i / 2].exist && !position[i % 2 + r][i / 2].death)
                    if (position[i % 2 + r][i / 2].buff_type != 13005 && position[i % 2 + r][i / 2].buff_type != 13025)
                    {
                        Console.WriteLine("change target to " + (i % 2 + r) + " " + (i / 2));
                        c.dest_row = (byte)(i % 2 + r);
                        c.dest_col = (byte)(i / 2);
                        return true;
                    }
            return false;
        }

        public void execCommand(BattleCommand c)
        {
            if (c.skill == 18001) //run
            {
                //insert here some RNG
                if (c.init_col == 2) // leader run
                    finish = 2;
                else position[c.init_row][c.init_col].outBattle = true;
                nextcmd++;
                return;
            }

            PacketCreator p = new PacketCreator(0x32, 1);

            if (c.skill >= 20001 && c.skill <= 20003)
                p.addBytes(makeExecutionPacket(c, false));
            else if (cmdReg[nextcmd].type == 0 && checkCombo(nextcmd + 1))
            {
                c.dmg = (ushort)(c.dmg * 1.2);
                p.addBytes(makeExecutionPacket(c, false));
                nextcmd++;
                while (checkCombo(nextcmd))
                {
                    c.dmg = (ushort)(c.dmg * 1.2);
                    p.addBytes(makeExecutionPacket(cmdReg[nextcmd], true));
                    nextcmd++;
                }
            }
            else
            {
                p.addBytes(makeExecutionPacket(c, false));
                nextcmd++;
            }

            //Console.WriteLine("send : " + BitConverter.ToString(p.getData()));
            battleBroadcast(p.send());

            if (c.skill < 20001 || c.skill > 20003)
            {
                if (c.type == 0) System.Threading.Thread.Sleep(SkillData.skillList[c.skill].delay * 100);
                else if (c.type == 1) System.Threading.Thread.Sleep(2400);
                else System.Threading.Thread.Sleep(SkillData.skillList[c.skill].delay * 100);

                if (finish > 0) return;

                //after-BattleCommand effect
                for (byte i = 0; i < 4; i++)
                    for (byte j = 0; j < 5; j++)
                    {
                        position[i][j].checkCommandEffect();
                        if (position[i][j].outBattle) checkOutBattle(position[i][j]);
                        position[i][j].purge_status();
                    }
            }
        }

        public byte[] makeExecutionPacket(BattleCommand c, bool combo)
        {
            byte count = 0;
            PacketCreator temp = new PacketCreator();
            int nb_target = 1;

            if (c.type != 0)
            {
                position[c.init_row][c.init_col].useItem(c.skill);
                if (c.type == 1) nb_target = 1;
                else if (c.type == 2)
                {
                    c.skill = ItemData.itemList[c.skill].unk9;
                    nb_target = SkillData.skillList[c.skill].nb_target;
                }
            }
            else
            {
                if (c.skill > 20003 || c.skill < 20001) nb_target = SkillData.skillList[c.skill].nb_target;

                if (c.skill == 11009 || c.skill == 11010)
                    nb_target = c.skill_lvl < 4 ? 1 : c.skill_lvl < 7 ? 3 : c.skill_lvl < 10 ? 6 : 8;

                // sp_cost
                int sp_cost = 0;
                if (SkillData.skillList.ContainsKey(c.skill))
                {
                    sp_cost = SkillData.skillList[c.skill].sp_cost;
                }
                position[c.init_row][c.init_col].setSp(-sp_cost);
                position[c.init_row][c.init_col].refreshSp();
            }

            switch (nb_target)
            {
                case 8:
                    c.dmg = (ushort)(c.dmg / 4);
                    byte r = (byte)(c.dest_row >= 2 ? 2 : 0);
                    for (byte i = r; i < r + 2; i++)
                        for (byte j = 0; j < 5; j++)
                            if (position[i][j].exist && !position[i][j].death)
                            {
                                count++;
                                temp.addBytes(getSkillEffect(i, j, c));
                            }
                    break;
                case 7: // hong thuy, ngu loi
                    c.dmg = (ushort)(c.dmg / 3);
                    for (byte j = 0; j < 5; j++)
                        if (position[c.dest_row][j].exist && !position[c.dest_row][j].death)
                        {
                            count++;
                            temp.addBytes(getSkillEffect(c.dest_row, j, c));
                        }
                    break;
                case 6:
                    c.dmg = (ushort)(c.dmg / 3);
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    byte r1 = (byte)(c.dest_row == 0 || c.dest_row == 1 ? 0 : 2);
                    for (byte i = r1; i < r1 + 2; i++)
                        for (int j = c.dest_col - 1; j <= c.dest_col + 1; j++)
                            if (j >= 0 && j < 5)
                                if (position[i][j].exist && !position[i][j].death)
                                {
                                    count++;
                                    if (i != c.dest_row || j != c.dest_col)
                                        temp.addBytes(getSkillEffect(i, (byte)j, c));
                                }
                    break;
                case 5: // bang da', phi sa tau thach
                    c.dmg = (ushort)(c.dmg / 3);
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    for (int j = c.dest_col - 1; j <= c.dest_col + 1; j += 2)
                        if (j >= 0 && j < 5)
                            if (position[c.dest_row][j].exist && (combo || !position[c.dest_row][j].death))
                            {
                                count++;
                                temp.addBytes(getSkillEffect(c.dest_row, (byte)j, c));
                            }
                    sbyte r2 = (sbyte)(c.dest_row == 0 || c.dest_row == 2 ? 1 : -1);
                    if (position[c.dest_row + r2][c.dest_col].exist && (combo || !position[c.dest_row + r2][c.dest_col].death))
                    {
                        count++;
                        temp.addBytes(getSkillEffect((byte)(c.dest_row + r2), c.dest_col, c));
                    }
                    break;
                case 4: // loan kich,
                    c.dmg = (ushort)(c.dmg / 2);
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    for (int j = c.dest_col - 1; j <= c.dest_col + 1; j += 2)
                        if (j >= 0 && j < 5)
                            if (position[c.dest_row][j].exist && (combo || !position[c.dest_row][j].death))
                            {
                                count++;
                                temp.addBytes(getSkillEffect(c.dest_row, (byte)j, c));
                            }
                            else
                            {
                                count++;
                                temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                            }
                        else
                        {
                            count++;
                            temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                        }
                    break;
                case 3:
                    c.dmg = (ushort)(c.dmg / 2);
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    for (int j = c.dest_col - 1; j <= c.dest_col + 1; j += 2)
                        if (j >= 0 && j < 5)
                            if (position[c.dest_row][j].exist && (combo || !position[c.dest_row][j].death))
                            {
                                count++;
                                temp.addBytes(getSkillEffect(c.dest_row, (byte)j, c));
                            }
                    break;
                case 2:
                    c.dmg = (ushort)(c.dmg / 2);
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    sbyte r3 = (sbyte)(c.dest_row == 0 || c.dest_row == 2 ? 1 : -1);
                    if (position[c.dest_row + r3][c.dest_col].exist && (combo || !position[c.dest_row + r3][c.dest_col].death))
                    {
                        count++;
                        temp.addBytes(getSkillEffect((byte)(c.dest_row + r3), c.dest_col, c));
                    }
                    break;
                default:
                    count++;
                    temp.addBytes(getSkillEffect(c.dest_row, c.dest_col, c));
                    break;
            }

            byte[] command_data = temp.getData();
            PacketCreator ret = new PacketCreator();
            ret.add16((ushort)(6 + command_data.Length)); //total length
            ret.add8(c.init_row); ret.add8(c.init_col);
            if (c.type != 1) ret.add16(c.skill);
            else ret.add16(19001);
            ret.add8((byte)nb_target);
            ret.add8(count); //nb of target affected
            ret.addBytes(command_data);

            return ret.getData();
        }

        public byte[] getSkillEffect(byte row, byte col, BattleCommand c)
        {
            BattleParticipant init = position[c.init_row][c.init_col];
            BattleParticipant dest = position[row][col];
            byte dest_anim = 0;
            byte nb_effect = 1;
            byte effect_code = 0;
            byte effect_type = 1;
            int final_dmg = 0;
            int init_elem = 0;
            if (SkillData.skillList.ContainsKey(c.skill))
            {
                init_elem = SkillData.skillList[c.skill].elem;
            }

            //int init_elem = init.getElem();
            if (c.skill == 10000)
            {
                init_elem = init.getElem();
            }
            int dest_elem = dest.getElem();
            double elem_coef = 1;


            switch (init_elem)
            {
                case 1: //earth
                    if (dest_elem == 2) elem_coef = 1.2;
                    else if (dest_elem == 4) elem_coef = 0.8;
                    break;
                case 2: //water
                    if (dest_elem == 3) elem_coef = 1.2;
                    else if (dest_elem == 1) elem_coef = 0.8;
                    break;
                case 3: //fire
                    if (dest_elem == 4) elem_coef = 1.2;
                    else if (dest_elem == 2) elem_coef = 0.8;
                    break;
                case 4: //wind
                    if (dest_elem == 1) elem_coef = 1.2;
                    else if (dest_elem == 3) elem_coef = 0.8;
                    break;
                default:
                    break;
            }

            byte effect = 1;
            if (c.skill >= 13016 && c.skill <= 13018) { effect = 3; }//thanh long;
            else if (c.skill >= 10017 && c.skill <= 10019) { effect = 15; } //nham quai;
            else if (c.skill >= 11017 && c.skill <= 11019) effect = 18; //thuy than;
            else if (c.skill >= 12017 && c.skill <= 12019) c.dmg = Math.Min((ushort)(c.dmg * (c.skill_lvl / 3)), (ushort)50000); //boost dmg phoenix
            else if (c.skill >= 20001 && c.skill <= 20003) effect = 20;
            else if (c.type == 1) effect = 13; //item
            else effect = SkillData.skillList[c.skill].skill_type;

            byte hit = 1;
            if (effect != 20) hit = calculateHit(row, col, ref c, effect, elem_coef);

            switch (effect)
            {
                case 1:
                case 2:
                    effect_code = 0x19;
                    if (hit == 1)
                    {
                        if (dest.buff_type == 10015) //kinh'
                        {
                            final_dmg = 0;
                            dest_anim = 1;
                            init.reflect = 10015;
                            init.reflect_dmg += (ushort)(c.dmg / 3);
                            dest.reflect_hp--;
                            if (dest.reflect_hp == 0)
                            {
                                dest.buff = 0;
                                dest.buff_type = 0;
                                battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 2, 0, 0 }).send());
                            }
                        }
                        else
                        {
                            //final_dmg = (int)(c.dmg * elem_coef - position[row][col].getDef() * Math.Pow(position[row][col].getLvl(), 0.3));
                            //final_dmg = (int)((c.dmg / position[row][col].getDef() + 2) * elem_coef);
                            //int defbase = (int)(dest.getLvl() * 2 + dest.getDef() * 1.75);
                            //final_dmg = (int)((c.dmg - defbase) * elem_coef);
                            int rbMul = init.getRb() - dest.getRb();
                            if (rbMul < 0)
                            {
                                rbMul = 0;
                            }
                            final_dmg = (int)((c.dmg - dest.getLvl() - dest.getDef()) * elem_coef * (1 + 0.1 * rbMul));
                            final_dmg += RandomGen.getInt(-2, 2);
                            if (dest.chr != null || dest.pet != null)
                            {
                                final_dmg = (final_dmg >> 1); // /2 dmg when dmg to player or pet T_T
                            }
                            if (final_dmg <= 0) final_dmg = 1;
                            if (dest.def)
                            {
                                final_dmg = final_dmg / 2;
                                if (elem_coef == 0.8)
                                {
                                    final_dmg = 1;
                                }
                            }
                            dest_anim = (byte)(dest.def ? 1 : 0);
                            dest.setHp(-final_dmg);
                            dest.refreshHp();
                            checkDeath(row, col, c);
                        }
                    }
                    else
                    {
                        if (dest.buff_type == 10010) dest_anim = 1;
                        else dest_anim = 2;
                        final_dmg = 0;
                    }
                    break;
                case 3: //disable skills
                    final_dmg = 0;
                    effect_code = 0xdd;
                    if (hit == 1)
                    {
                        dest_anim = 1;
                        //dest.disable += (byte)(Math.Ceiling((double)c.skill_lvl / 2) + 1); // Sao lai la += ??
                        dest.disable = (byte)(Math.Ceiling((double)c.skill_lvl / 2) + 1);
                        if (c.skill >= 13015 && c.skill <= 13018) //Trieu thanh long
                        {
                            dest.disable = 5;
                        }
                        dest.disable_type = c.skill;
                        countDisabled++;
                        Console.WriteLine(row + " " + col + " get disable " + c.skill);
                    }
                    else
                        dest_anim = 2;
                    break;
                case 4: //buff skills
                    final_dmg = 0;
                    effect_code = 0xde;
                    if (hit == 1)
                    {
                        dest_anim = 1;
                        dest.buff = (byte)(Math.Ceiling((double)c.skill_lvl / 2) + 1); //not really true, fix later
                        dest.buff_type = c.skill;
                        if (c.skill == 10015) dest.reflect_hp = 4;
                        Console.WriteLine(row + " " + col + " get buff " + c.skill);
                    }
                    else
                        dest_anim = 0;
                    break;
                case 5: //giai tru
                    final_dmg = 0;
                    dest_anim = 0;
                    effect_code = 0;
                    nb_effect = 5;
                    break;
                case 6: //hoi SP                    
                    final_dmg = c.dmg;
                    dest_anim = 0;
                    effect_code = 0x1a;
                    effect_type = 0;
                    break;
                case 7: //hoi hp
                    final_dmg = c.dmg;
                    dest_anim = 0;
                    effect_code = 0x19;
                    effect_type = 0;
                    break;
                case 8: //ressurrection
                    if (hit == 1)
                    {
                        dest.death = false;
                        if (row < 2) countEnemy++; else countAlly++;
                        goto case 7;
                    }
                    final_dmg = 0;
                    dest_anim = 1;
                    effect_code = 0x19;
                    break;
                case 9: //phan than, laterrrr T_T
                    final_dmg = 0;
                    dest_anim = 1;
                    effect_code = 0x19;
                    break;
                case 11: // tha luoi -_-
                    if (CatchPet(init, dest))
                    {
                        init.chr.addPet((ushort)dest.npc.npcid, 0);
                        dest.outBattle = true;
                        // Chua sua effect -.-
                        dest_anim = 0;
                        effect_code = 0x19;
                    }
                    break;
                case 12: // run
                    break;
                case 13: //item, later
                    if (hit == 1)
                    {
                        final_dmg = ItemData.itemList[c.skill].prop1_val;
                        dest_anim = 0;
                        effect_type = 0;
                        ushort prop1 = ItemData.itemList[c.skill].prop1;
                        if (prop1 == 25)
                        {
                            effect_code = 0x19;
                            dest.setHp(c.dmg);
                            dest.refreshHp();
                        }
                        else if (prop1 == 26)
                        {
                            effect_code = 0x1a;
                            dest.setSp(c.dmg);
                            dest.refreshSp();
                        }
                        if (ItemData.itemList[c.skill].prop2 != 0) nb_effect = 2;
                        if (ItemData.itemList[c.skill].type == 50) //HHD
                        {
                            dest.death = false;
                            if (c.dest_row < 2) countEnemy++; else countAlly++;
                        }
                    }
                    else
                    {
                        final_dmg = 0;
                        dest_anim = 1;
                        effect_code = 0x19;
                        effect_type = 0;
                    }
                    break;
                case 14: //thanh luu
                    nb_effect = 2;
                    final_dmg = c.dmg;
                    dest_anim = 1;
                    effect_code = 0x19;
                    effect_type = 0;
                    break;
                case 15: //debuff
                    final_dmg = 0;
                    effect_code = 0xdf;
                    if (hit == 1)
                    {
                        dest_anim = 0;
                        dest.debuff = (byte)(Math.Ceiling((double)c.skill_lvl / 2) + 1);
                        if (c.skill >= 10016 && c.skill <= 10019) //TG Nham quai
                        {
                            dest.debuff = 5;
                        }
                        dest.debuff_type = c.skill;
                        if (c.skill == 14021 || c.skill == 20014)
                            for (int i = nextcmd; i < cmdReg.Count; i++)
                                if (cmdReg[i].init_row == row && cmdReg[i].init_col == col)
                                    cmdReg.RemoveAt(i);
                        Console.WriteLine(row + " " + col + " get debuff " + c.skill);
                    }
                    else
                        dest_anim = 2;
                    break;
                case 16:
                    goto case 5;
                case 18:
                    goto case 5;
                case 19:
                    final_dmg = 0;
                    effect_code = 0xe1;
                    if (hit == 1)
                    {
                        dest_anim = 1;
                        dest.aura = (byte)(Math.Ceiling((double)c.skill_lvl / 2) + 1); //not really true, fix later
                        dest.aura_type = c.skill;
                        Console.WriteLine(row + " " + col + " get aura " + c.skill);
                    }
                    else
                        dest_anim = 0;
                    break;
                case 20: //special case for no anim dmg (reflect, poison, etc.)
                    final_dmg = c.dmg;
                    effect_code = 0x19;
                    Console.WriteLine("special dmg " + final_dmg);
                    dest_anim = 0;
                    dest.setHp(-c.dmg);
                    dest.refreshHp();
                    checkDeath(row, col, c);
                    break;
                default:
                    goto case 2;
            }

            byte[] p = new byte[5 + nb_effect * 4];
            p[0] = row; p[1] = col;
            p[2] = hit; //anim attack
            p[3] = dest_anim;
            p[4] = nb_effect;
            p[5] = effect_code;
            p[6] = (byte)final_dmg; p[7] = (byte)(final_dmg >> 8);
            p[8] = effect_type;      //effect : 1 : dmg, 0: heal, ...

            //subeffects
            if (nb_effect > 1)
            {
                byte[] subeffect_code = null;
                ushort[] dmg_subeffect = null;
                byte[] subeffect_type = null;
                if (nb_effect == 2)
                {
                    subeffect_code = new byte[1];
                    dmg_subeffect = new ushort[1];
                    subeffect_type = new byte[1];
                    if (c.type == 1)
                    {
                        ushort prop2 = ItemData.itemList[c.skill].prop2;
                        dmg_subeffect[0] = (ushort)ItemData.itemList[c.skill].prop2_val;
                        if (prop2 == 25)
                        {
                            subeffect_code[0] = 0x19;
                            dest.setHp(dmg_subeffect[0]);
                            dest.refreshHp();
                        }
                        else if (prop2 == 26)
                        {
                            subeffect_code[0] = 0x1a;
                            dest.setSp(dmg_subeffect[0]);
                            dest.refreshSp();
                        }
                        subeffect_type[0] = 0;
                    }
                    else if (effect == 14)
                    {
                        subeffect_code[0] = 0x1a;
                        dmg_subeffect[0] = (ushort)(c.dmg / 5);
                        subeffect_type[0] = 0;
                    }
                }
                if (nb_effect == 5)
                {
                    subeffect_code = new byte[] { 0xdd, 0xde, 0xdf, 0xe1 };
                    dmg_subeffect = new ushort[] { 0, 0, 0, 0 };
                    subeffect_type = new byte[] { 1, 1, 1, 1 };
                }

                for (int i = 1; i < nb_effect; i++)
                {
                    p[5 + i * 4] = subeffect_code[i - 1];
                    p[6 + i * 4] = (byte)dmg_subeffect[i - 1];
                    p[7 + i * 4] = (byte)(dmg_subeffect[i - 1] >> 8);
                    p[8 + i * 4] = subeffect_type[i - 1];      //effect : 1 : dmg, 0: heal, ...
                }
            }

            return p;
        }

        public byte calculateHit(byte row, byte col, ref BattleCommand c, byte effect, double elem_coef) //calculate miss or hit here
        {
            BattleParticipant init = position[c.init_row][c.init_col];
            BattleParticipant dest = position[row][col];

            switch (effect)
            {
                case 1:
                    if (dest.buff_type == 13003) return 0; //lan tranh
                    if (init.buff_type == 13005) position[c.init_row][c.init_col].buff = 1; //het an minh trong luot sau;
                    goto case 2;
                case 2:
                    if (dest.buff_type == 10010) return 0; //ket gioi                    
                    else if (dest.buff_type == 11002) c.dmg = (ushort)(c.dmg * 0.75); //bang tuong //need update
                    else if (dest.buff_type == 10031) c.dmg = (ushort)(c.dmg * 0.5); //chung trao //need update

                    if (dest.disable_type == 20026) c.dmg = (ushort)(c.dmg * 0.75); //bi bang phong tang def

                    if (init.buff_type == 13012) c.dmg = (ushort)(c.dmg * 1.25); //phong dai
                    else if (init.debuff_type == 13011) c.dmg = (ushort)(c.dmg * 0.75); //thu nho 

                    if (init.aura_type == 12025) c.dmg = (ushort)(c.dmg * 1.25); //cuong no
                    else if (init.aura_type == 14040) c.dmg = (ushort)(c.dmg * 1.5); //ba y
                    if (dest.def) c.dmg = (ushort)(c.dmg * 0.75);

                    //50% addition miss chance under golem summon effect
                    if ((init.debuff_type >= 10017) && (init.debuff_type <= 10019))
                        return (byte)(RandomGen.getByte(0, 100) >= (dest.getLvl() - init.getLvl() + 5) * 0.2 + 50 ? 1 : 0);
                    //miss 10% if equal lvl, 2% more each lvl
                    return (byte)(RandomGen.getByte(0, 100) >= (dest.getLvl() - init.getLvl() + 5) * 0.2 ? 1 : 0);
                case 3:
                    if (dest.death) return 0;
                    if (dest.disable_type != 0) return 0;
                    //disable skills always miss 20% plus 10% if equal lvl, 2% more each lvl                    
                    else return (byte)(RandomGen.getByte(0, 100) >= Math.Max((dest.getLvl() - init.getLvl() + 5) * 0.2, 0) + 20 ? 1 : 0);
                case 4:
                    if (dest.death) return 0;
                    if (dest.buff_type != 0) return 0;
                    else return 1; //buff skill always hit
                case 5:
                    if (c.skill == 11012 || c.skill == 11025 || c.skill == 11031)
                    {
                        position[row][col].purge_type = 3;
                        return 1;
                    }
                    if (c.skill == 11015 && dest.disable_type == 11014) //giai bang phong
                    {
                        position[row][col].purge_type = 4;
                        return 1;
                    }
                    if (c.skill == 14007 && dest.disable_type == 14008) //giai hon me
                    {
                        position[row][col].purge_type = 4;
                        return 1;
                    }
                    if (c.skill == 14014 && dest.debuff_type == 14015) //giai doc
                    {
                        position[row][col].purge_type = 6;
                        return 1;
                    }
                    if (c.skill == 14022 && dest.debuff_type == 14021) //giai hon loan
                    {
                        position[row][col].purge_type = 6;
                        return 1;
                    }
                    if (c.skill == 10009 && dest.buff_type == 10010) //giai ket gioi
                    {
                        position[row][col].purge_type = 5;
                        return 1;
                    }
                    return 0;
                case 6:
                    if (dest.death) return 0;
                    c.dmg = (ushort)Math.Min(dest.getMaxSp() - dest.getSp(), c.dmg);
                    dest.setSp(c.dmg);
                    dest.refreshSp();
                    return 1;
                case 7:
                    if (dest.death) return 0;
                    c.dmg = (ushort)Math.Min(dest.getMaxHp() - dest.getHp(), c.dmg);
                    dest.setHp(c.dmg);
                    dest.refreshHp();
                    return 1;
                case 8:
                    if (!dest.death) return 0;
                    c.dmg = (ushort)(dest.getMaxHp() * init.getSkillLvl(11013) * 0.1);
                    dest.setHp(c.dmg);
                    dest.refreshHp();
                    return 1;
                case 11:
                    return 1;
                case 13:
                    if (dest.death && ItemData.itemList[c.skill].type != 50)
                        return 0;
                    if (!dest.death && ItemData.itemList[c.skill].type == 50)
                        return 0;
                    else return 1;
                case 14:
                    if (dest.death) return 0;
                    c.dmg = (ushort)Math.Min(dest.getMaxHp() - dest.getHp(), c.dmg);
                    dest.setHp(c.dmg);
                    dest.refreshHp();
                    dest.setSp(c.dmg / 5);
                    dest.refreshSp();
                    return 1;
                case 15:
                    if (dest.death) return 0;
                    if (dest.debuff_type != 0) return 0;
                    //debuff skills always miss 15% plus 10% if equal lvl, 2% more each lvl                    
                    else return (byte)(RandomGen.getByte(0, 100) >= Math.Max((dest.getLvl() - init.getLvl() + 5) * 0.2, 0) + 15 ? 1 : 0);
                case 16:
                    if (c.skill == 10009 && dest.buff_type == 10010) //giai ket gioi
                    {
                        position[row][col].purge_type = 5;
                        return 1;
                    }
                    if (c.skill == 10014 && dest.buff_type == 10015) //giai kinh
                    {
                        position[row][col].purge_type = 5;
                        return 1;
                    }
                    return 0;
                case 18:
                    if (sameSide(c.init_row, row)) dest.purge_type = 2;
                    else dest.purge_type = 1;
                    return 1;
                case 19:
                    if (dest.death) return 0;
                    if (dest.aura_type != 0) return 0;
                    else return 1; //aura skill always hit
                default:
                    return 0;
            }

        }

        public bool checkCombo(int index)
        {
            if (index == cmdReg.Count) return false;
            if ((cmdReg[index].type != 0) || (cmdReg[index - 1].type != 0)) return false;
            if (SkillData.skillList[cmdReg[index - 1].skill].skill_type != 1 || SkillData.skillList[cmdReg[index].skill].skill_type != 1)
                return false;
            if (cmdReg[index - 1].dest_col != cmdReg[index].dest_col || cmdReg[index - 1].dest_row != cmdReg[index].dest_row)
                return false;
            if (position[cmdReg[index].init_row][cmdReg[index].init_col].disable > 0 || position[cmdReg[index].init_row][cmdReg[index].init_col].death)
                return false;
            return true; //combo 100% regardless of agi and level
        }

        public bool sameSide(byte row1, byte row2)
        {
            return (Math.Abs(row1 - row2) == 1 && row1 + row2 != 3);
        }

        public void battleBroadcast(byte[] msg)
        {
            for (int i = 0; i < 4; i += 3)
                for (int j = 0; j < 5; j++)
                    if (position[i][j].exist && position[i][j].type == 1)
                        position[i][j].chr.reply(msg);
        }

        public BattleParticipant getBpByClient(TSClient client)
        {
            for (int i = 0; i < 4; i += 3)
                for (int j = 0; j < 5; j++)
                {
                    if (position[i][j].exist && position[i][j].chr == client.getChar())
                        return position[i][j];
                }
            return null;
        }

        public void DoEquip(TSClient client)
        {
            BattleParticipant bp = getBpByClient(client);
            if (bp == null) return;
            // Send Battle Command
            battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, bp.row, bp.col }).send());
            cmdNeeded--;
            if (cmdNeeded == 0) execute();
        }

        public void DoEquipPet(TSClient client)
        {
            BattleParticipant bp = getBpByClient(client);
            if (bp == null) return;

            // Pet Row
            byte col = 0; byte row = 0;
            if (bp.row == 0) row = 1; else row = 2;
            col = bp.col;
            // Send Battle Command
            battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, row, col }).send());
            cmdNeeded--;
            if (cmdNeeded == 0) execute();
        }

        public void SetBattlePet(TSClient client, byte[] data)
        {
            ushort pet_npcid = PacketReader.read16(data, 2);
            TSCharacter player = client.getChar();

            byte col = 0; byte row = 0;
            if (client.getChar().setBattlePet(PacketReader.read16(data, 2)) && pet_npcid != player.pet_battle)
            {
                BattleParticipant bp = getBpByClient(client);
                if (bp == null) return;
                // Send Battle Command
                battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, bp.row, bp.col }).send());
                bp.alreadyCommand = true;
                cmdNeeded--;

                // Pet Row
                if (bp.row == 0) row = 1; else row = 2;
                col = bp.col;

                if (position[row][col].exist)
                    checkOutBattle(position[row][col]);

                position[row][col] = new BattleParticipant(this, row, col);
                position[row][col].petIn(player.pet[player.pet_battle]);
                countAlly++;

                // Refresh Pet here
                TSPet pet = player.pet[player.pet_battle];
                var p = new PacketCreator(0x0B, 0x05);
                p.addBytes(position[row][col].announce(5, countAlly).getData());
                battleBroadcast(p.send());

                client.reply(new PacketCreator(data).send());
            }

            if (cmdNeeded == 0) execute();
        }

        public void UnBattlePet(TSClient client, byte[] data)
        {
            BattleParticipant bp = getBpByClient(client);
            if (bp == null) return;

            battleBroadcast(new PacketCreator(new byte[] { 0x35, 5, bp.row, bp.col }).send());
            bp.alreadyCommand = true;
            cmdNeeded--;

            // Pet Position
            int row = bp.row == 0 ? 1 : 2;
            checkOutBattle(position[row][bp.col]);

            // Send unbattle pet
            if (client.getChar().unsetBattlePet())
                client.reply(new PacketCreator(data).send());

            if (cmdNeeded == 0) execute();
        }

        public void outBattle(TSClient c)
        {
            for (int i = 0; i < 5; i++)
                if (position[3][i].exist)
                    if (position[3][i].chr.client == c) //search for position of client
                    {
                        Console.WriteLine("3 " + i + " out of battle");
                        BattleParticipant charOut = position[3][i];
                        charOut.outBattle = true;
                        if (aTimer.Enabled)  //disconnect during 20s timer
                            checkOutBattle(charOut);

                        //same with pet
                        if (position[2][i].exist)
                        {
                            BattleParticipant petOut = position[2][i];
                            petOut.outBattle = true;
                            if (aTimer.Enabled)
                                checkOutBattle(petOut);
                        }
                    }
            if (aTimer.Enabled && cmdNeeded == 0) execute();
        }

        public void checkOutBattle(BattleParticipant bp)
        {
            bp.exist = false;
            bp.outBattle = false; //reset the value so that this won't get called again

            if (!bp.death) //if char still alive
            {
                if (bp.row >= 2) countAlly--;
                else countEnemy--;
                if (countEnemy == 0) finish = 1;
                else if (countAlly == 0) finish = 2;
                if (bp.disable > 0) //char alive but disabled
                    countDisabled--;
            }
            battleBroadcast(new PacketCreator(new byte[] { 0xb, 1, bp.row, bp.col }).send());

            if (aTimer.Enabled && !bp.alreadyCommand && !bp.death && bp.disable == 0) cmdNeeded--;  //not given BattleCommand yet
        }

        public bool CatchPet(BattleParticipant init, BattleParticipant dest)
        {
            if (dest.npc != null)
            {
                if (init.chr != null)
                {
                    if (init.chr.next_pet < 4)
                    {
                        if (init.chr.level + 5 >= dest.npc.level && NpcData.npcList[(ushort)dest.npc.npcid].notPet == 0)
                        {
                            double rate = (1 - (double)dest.getHp() / dest.getMaxHp()) * 100;
                            if (RandomGen.getInt(0, 100) < rate)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
