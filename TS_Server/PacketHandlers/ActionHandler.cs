using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Server;
using TS_Server.DataTools;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class ActionHandler
    {
        public ActionHandler(TSClient client, byte[] data)
        {
            
            switch (data[1])
            {
                case 1: // Click on NPC
                    //new TSBattleNPC(client, 3, 0xffff, getRandomBattle(client));
                    client.ClickkNpc(data, client);
                    //client.continueMoving();
                    break;
                case 2: // Collide With NPC
                    client.continueMoving();
                    Console.WriteLine("come here ne kuuu");
                    break;
                case 4: // Click on Trigger
                    client.continueMoving();
                    break;
                case 6: // warp ok
                    Console.WriteLine("Finished quest");
                    client.TalkQuestNpc(data, client);
                    //client.continueMoving();
                    client.getChar().showOutfit();
                    break;
                case 8: //initiate warp
                    {
                        Console.WriteLine("WRAPPP HERE quest");
                        ///- Send Enter Door action response
                        var p = new PacketCreator(20);
                        p.add8(0x07); client.reply(p.send());

                        p = new PacketCreator(0x29);
                        p.add8(0x0E); client.reply(p.send());

                        TSCharacter player = client.getChar();
                        if (player.party != null && player.isTeamLeader())
                        {
                            foreach (TSCharacter c in player.party.member)
                            {
                                c.client.warpPrepare = PacketReader.read16(data, 2);
                                TSWorld.getInstance().warp(c.client, c.client.warpPrepare);
                            }
                        }
                        else
                        {
                            client.warpPrepare = PacketReader.read16(data, 2);
                            TSWorld.getInstance().warp(client, client.warpPrepare);
                        }

                        ///- Force team update to set if team leader
                        if (client.getChar().isTeamLeader())
                        {
                            client.getChar().sendUpdateTeam();
                            ///- Update sub-leader
                            client.getChar().party.UpdateTeamSub(client);
                        }
                    }
                    break;
                case 9:
                    client.selectMenu = (ushort)data[2];
                    break;
                default:
                    Console.WriteLine("Action Handler : unknown subcode" + data[1]);
                    client.continueMoving();
                    break;
            }
        }

        public ushort[] getRandomBattle(TSClient client)
        {
            ushort[] ret = new ushort[10];
            List<byte> exclude = new List<byte>(new byte[] { 11, 12, 14, 15, 17, 20, 21, 22, 23, 24, 25, 28, 30, 35, 51, 52, 53 });
            int pos = 0;
            int maxlvl = Math.Min(client.getChar().level + 5, 200);
            int minlvl = Math.Min(Math.Max(client.getChar().level - 5, 1), 195);
            ushort id;
            NpcInfo npc;

            while (pos < 10)
            {
                id = RandomGen.getUShort(10001, 61021);
                if (NpcData.npcList.ContainsKey(id))
                    if (!exclude.Contains(NpcData.npcList[id].type))
                    {
                        npc = NpcData.npcList[id];
                        if (npc.level <= maxlvl && (npc.level >= minlvl || npc.hpmax >= client.getChar().level * 70))
                        {
                            ret[pos] = npc.id;
                            pos++;
                            if (pos >= 10) break;
                        }
                    }
            }

            return ret;
        }
    }
}
