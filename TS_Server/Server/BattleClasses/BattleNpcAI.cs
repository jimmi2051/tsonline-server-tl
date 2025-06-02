using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.DataTools;

namespace TS_Server.Server.BattleClasses
{
    public class BattleNpcAI
    {
        public ushort npcid;
        public ushort hpmax, spmax, hp, sp, atk, mag, def, agi;
        public ushort[] skill;
        public byte level, elem, reborn;
        public int disable;
        public ushort count;
        public ushort drop;
        public List<Tuple<byte, byte>> attacker;
        public List<Tuple<byte, byte>> killer;

        public BattleNpcAI(TSBattleNPC b, ushort cnt, ushort id)
        {
            npcid = id;
            NpcInfo npcinfo = NpcData.npcList[id];
            hpmax = (ushort)npcinfo.hpmax;
            spmax = (ushort)npcinfo.spmax;
            hp = (ushort)npcinfo.hpmax;
            sp = (ushort)npcinfo.spmax;
            level = npcinfo.level;
            elem = npcinfo.element;
            reborn = npcinfo.reborn;
            mag = npcinfo.mag;
            atk = npcinfo.atk;
            def = npcinfo.def;
            agi = npcinfo.agi;
            if (npcinfo.skill4 != 0)
            {
                skill = new ushort[4];
                skill[3] = npcinfo.skill4;
            }
            else skill = new ushort[3];
            skill[0] = npcinfo.skill1;
            skill[1] = npcinfo.skill2;
            skill[2] = npcinfo.skill3;
            count = cnt;

            attacker = new List<Tuple<byte, byte>>();
            killer = new List<Tuple<byte, byte>>();
        }

        public ushort generateDrop()
        {
            byte NO_DROP = 70; //chance of no drop
            byte DROP_NORMAL = 0; //chance of drop normal items, the rest is rare drop

            byte rand = RandomGen.getByte(0, 100);

            if (rand < NO_DROP) return 0;
            rand -= NO_DROP;

            if (rand < DROP_NORMAL)
            {
                int i = NpcData.drop[(ushort)npcid].Count;
                if (i == 0) return 0;
                else return NpcData.drop[(ushort)npcid][rand % i];
            }

            int j = NpcData.rareDrop[(ushort)npcid].Count;
            if (j == 0) return 0;
            else return NpcData.rareDrop[(ushort)npcid][rand % j];
        }
    }
}
