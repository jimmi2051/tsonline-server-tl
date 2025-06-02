using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.DataTools;
using TS_Server.Client;

namespace TS_Server.Server
{
    public class TSWorld
    {
        private static TSWorld instance = null;
        public TSServer server;
        public Dictionary<ushort, TSMap> listMap;
        
        public TSWorld(TSServer s)
        {
            server = s;
            instance = this;
            listMap = new Dictionary<ushort, TSMap>();
        }

       
        public static TSWorld getInstance()
        {
            return instance;
        }

        public TSMap initMap(ushort mapid)
        {
            TSMap m = new TSMap(this, mapid);
            listMap.Add(mapid,m);
            return m;
        }

        public void warp(TSClient client, ushort warpid)
        {
            client.reply(new PacketCreator(new byte[] { 20, 0x07 }).send());
            client.reply(new PacketCreator(new byte[] { 0x29, 0x0E }).send());

            ushort start = client.map.mapid;
            if (WarpData.warpList.ContainsKey(start))
            {
                if (WarpData.warpList[start].ContainsKey(warpid))
                {
                    ushort[] dest = WarpData.warpList[start][warpid];

                    if (!listMap.ContainsKey(dest[0]))
                    {
                        listMap.Add(dest[0], new TSMap(this, dest[0]));
                    }

                    client.getChar().mapID = dest[0];
                    client.getChar().mapX = (ushort)(dest[1] - 100);
                    client.getChar().mapY = (ushort)(dest[2] - 0);
                    //client.map.removePlayer(client.accID);

                    listMap[dest[0]].addPlayerWarp(client, dest[1], dest[2]);
                    return;
                }
                else if (warpid == 2 & start == 10851)
                {
                   
                        if (!listMap.ContainsKey(12003))
                        {
                            listMap.Add(12003, new TSMap(this, 12003));
                        }
                        client.getChar().mapID = 12003;
                        client.getChar().mapX = 555;
                        client.getChar().mapY = 555;
                        listMap[12003].addPlayerWarp(client, 555, 555);              
                }
                else
                {
                  
                    Console.WriteLine("Warp data helper : warpid " + warpid + " not found");
                    EveData.loadCoor(start, 12000, warpid);
                }
            }
          
            else
            {
                Console.WriteLine("Warp data helper : mapid " + start + " warpid " + warpid + " not found");
                EveData.loadCoor(start, 12000, warpid);
            }
            client.AllowMove();
        }
    }
}
