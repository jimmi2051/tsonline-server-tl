using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Server.Server.BattleClasses
{
    public class BattleCommand
    {
        public byte init_row;
        public byte init_col;
        public byte dest_row;
        public byte dest_col;
        public byte type;
        public ushort skill;
        public byte skill_lvl;
        public ushort dmg; //just do it simple for now
        public int priority;

        public BattleCommand(byte i_row, byte i_col, byte d_row, byte d_col, byte t)
        {
            init_row = i_row;
            init_col = i_col;
            dest_row = d_row;
            dest_col = d_col;
            type = t;
        }

        public BattleCommand(byte i_row, byte i_col, byte d_row, byte d_col, ushort sk, ushort damage)
        {
            init_row = i_row;
            init_col = i_col;
            dest_row = d_row;
            dest_col = d_col;
            type = 0;
            skill = sk;
            dmg = damage;
        }
    }
}
