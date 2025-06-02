using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TS_Server.DataTools
{
    public static class BattleData
    {
        public static Dictionary<ushort, BattleInfo> battleList = new Dictionary<ushort,BattleInfo>();
        public static int battleCount = 0;
        public static void loadBattle(string input)
        {
            FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read);
            StreamReader s = new StreamReader(fs);
            ushort id, ground;
            ushort[] npcid;
            s.ReadLine();
            while (!s.EndOfStream)
            {
                string str = s.ReadLine();
                string[] data = Regex.Split(str, @"\t+");
                id = ushort.Parse(data[0]);
                ground = ushort.Parse(data[1]);
                npcid = new ushort[10];
                for (int i = 0; i < 10; i++)
			    {
                    npcid[i] = ushort.Parse(data[2 + i]);
			    }
                battleList.Add(id, new BattleInfo(ground, npcid));
                battleCount++;
            }
            s.Close(); fs.Close();
        }
    }

    public class BattleInfo
    {
        ushort ground;
        ushort[] npcid;

        public BattleInfo(ushort ground, ushort[] npcid)
        {
            this.ground = ground;
            this.npcid = npcid;
        }

        public ushort getGround()
        {
            return ground;
        }

        public ushort[] getNpcId()
        {
            return npcid;
        }
    }
}
