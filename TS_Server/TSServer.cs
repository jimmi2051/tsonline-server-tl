using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Server;
using TS_Server.DataTools;
using TS_Server.Client;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TS_Server
{
    public class TSServer
    {
        private static TSServer instance = null;
        private ServerHandler handler = null;
        private TSWorld world;
        private Dictionary<uint, TSClient> listPlayers;


        public static TSServer getInstance()
        {
            if (instance == null)
            {
                instance = new TSServer();
            }
            return instance;
        }
   
        public void run()
        {
            Console.WriteLine("Loading item data ...");
            ItemData.LoadItems();
            Console.WriteLine("Loaded " + ItemData.itemList.Count + " items");
            ItemData.writeToFile("item.txt");

            Console.WriteLine("Loading NPC data ...");
            NpcData.LoadNpcs();
            Console.WriteLine("Loaded " + NpcData.npcList.Count + " NPCs");
            NpcData.writeToFile("npc.txt");



            Console.WriteLine("Loading Eve data ...");
            Console.WriteLine("Loaded " + EveData.eveList.Count + " EVEs");
            EveData.loadHeaders();



            Console.WriteLine("Loading Warp data ...");
            WarpData.loadTxt("warps.txt");
            //WarpData.loadWarpEx();
            //WarpData.loadWarpDoDo();
            EveData.loadAllWarp();
            Console.WriteLine("Loaded " + WarpData.warpCount + " Warp Gates");
            WarpData.writeToFile("newWarp.txt");

            Console.WriteLine("Loading Skill data ...");
            SkillData.LoadSkills();
            Console.WriteLine("Loaded " + SkillData.skillList.Count + " Skills");
            SkillData.writeToFile("skill.txt");

            Console.WriteLine("Loading Scene data ...");
            SceneData.LoadScenes();
            Console.WriteLine("Loaded " + SceneData.sceneList.Count + " Scenes");
            SceneData.writeToFile("scene.txt");

            Console.WriteLine("Loading Talk data ...");
            TalkData.LoadTalks();
            Console.WriteLine("Loaded " + TalkData.talkList.Count + " Dialogs");
            TalkData.writeToFile("talk.txt");

            Console.WriteLine("Loading Battle data ...");
            BattleData.loadBattle("battle.txt");

            FileStream fss = new FileStream("quests.json", FileMode.Create, FileAccess.Write);
            StreamWriter ss = new StreamWriter(fss);
            Console.WriteLine("Loading Quest data ...");
            var jsonString = JObject.FromObject(EveData.listSteps[12000]).ToString();
            ss.Write(jsonString);
            ss.Write("\n");
            ss.Close();
            fss.Close();

            FileStream fs = new FileStream("itemOnMap.json", FileMode.Create, FileAccess.Write);
            StreamWriter s = new StreamWriter(fs);
            Console.WriteLine("Loading map wrap data ...");
            var jsonString2 = JArray.FromObject(EveData.listItemOnMap[12002]).ToString();
            s.Write(jsonString2);
            s.Write("\n");
            s.Close();
            fs.Close();


            handler = new ServerHandler(6414);

            listPlayers = new Dictionary<uint, TSClient>();
            world = new TSWorld(this);

            Console.WriteLine("Server is started...");
        }

        static void Main(string[] args)
        {
            TSServer.getInstance().run();
            Console.ReadLine();
        }

        public TSWorld getWorld()
        {
            return world;
        }

        public void addPlayer(TSClient c)
        {
            listPlayers.Add(c.accID, c);
            TSMap m;
            if (!world.listMap.ContainsKey(c.getChar().mapID))
                m = world.initMap(c.getChar().mapID);
            else
                m = world.listMap[c.getChar().mapID];
            m.addPlayer(c);
        }

        public void removePlayer(uint id)
        {
            listPlayers.Remove(id);
        }

        public TSClient getPlayerById(int id)
        {
            if (listPlayers.ContainsKey((UInt32)id))
                return listPlayers[(UInt32)id];
            return null;
        }
    }
}
