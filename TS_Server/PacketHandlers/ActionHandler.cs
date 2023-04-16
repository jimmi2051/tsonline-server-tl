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
                    //client.ClickkNpc(data, client);
                    //if (client.idNpcTalking == 0)
                    //{
                    //    client.continueMoving();
                    //}
                    //client.TalkQuestNpc(data, client);
                    Console.WriteLine("Come here Collide");
                    break;
                case 4: // Click on Trigger
                    client.continueMoving();
                    break;
                case 6: // warp ok

                    client.TalkQuestNpc(client);
                    //client.continueMoving();
                    client.getChar().showOutfit();
                    break;
                case 8: //initiate warp
                    {

                        ushort start = client.map.mapid;
                        ushort warpid = PacketReader.read16(data, 2);
                        Console.WriteLine("DEBUG ++++ INIT WARP +++ " + warpid);
                        client.warpid = warpid;
                        if (WarpData.warpList.ContainsKey(start) && WarpData.warpList[start].ContainsKey(warpid) && EveData.listSteps.ContainsKey(client.map.mapid))
                        {
                            Dictionary<ushort, List<NewStep>> listStepsInMap = EveData.listSteps[client.map.mapid];
                            ushort[] dest = WarpData.warpList[start][warpid];
                            ushort end = dest[0];
                            ushort posX = dest[1];
                            ushort posY = dest[2];
                            if (listStepsInMap.ContainsKey(9919))
                            {
                                List<NewStep> newSteps = listStepsInMap[9919];
                                List<NewStep> matchSteps = newSteps.FindAll(item => item.childs.FindIndex(child => child.targetMapId == end & child.targetMapX == posX & child.targetMapY == posY) > -1);
                                if (matchSteps.Count > 0)
                                {
                                    List<NewStep> validSteps = new List<NewStep>();
                                    for (int i = 0; i < matchSteps.Count; i++)
                                    {
                                        Console.WriteLine("=======================================");
                                        Console.WriteLine("Quest ID > " + matchSteps[i].questId);
                                        Console.WriteLine("Idx > " + matchSteps[i].index);
                                        Console.WriteLine("Root Idx > " + matchSteps[i].rootIndex);
                                        Console.WriteLine("=======================================");
                                        NewStep checkingStep = matchSteps[i];

                                        ushort parentChecking = checkingStep.parentQuestId;
                                        if (parentChecking <= 0)
                                        {
                                            parentChecking = client.findParentQuestId(checkingStep.questId);
                                        }
                                        //Console.WriteLine("parent Quest >>> " + parentChecking);
                                        if (parentChecking > 0)
                                        {
                                            int isFinish = client.checkQuest(client, parentChecking);
                                            if (isFinish >= 1 && checkingStep.questReceived.Contains(parentChecking))
                                            {
                                                Console.WriteLine("SKIPPED Finished Q > Parent" + checkingStep.parentQuestId + " ROOT " + checkingStep.rootIndex + " INDEX " + checkingStep.index);
                                                continue;
                                            }
                                        }
                                        if (checkingStep.questId <= 0 && (checkingStep.optId > 0 || checkingStep.resBattle > 0))
                                        {
                                            Console.WriteLine("SKIPPED OPTs " + checkingStep.parentQuestId + " ROOT " + checkingStep.rootIndex + " INDEX " + checkingStep.index);
                                            continue;
                                        }
                                        if (checkingStep.childQuestId > 0)
                                        {
                                            int isInsertQ = client.checkQuest(client, checkingStep.childQuestId);
                                            if (isInsertQ > 0 && checkingStep.questReceived.Contains(checkingStep.childQuestId))
                                            {
                                                Console.WriteLine("SKIPPED Q have received > Parent" + checkingStep.parentQuestId + " ROOT " + checkingStep.rootIndex + " INDEX " + checkingStep.index);
                                                continue;
                                            }
                                        }
                                        if (checkingStep.requiredLevel > client.getChar().level)
                                        {
                                            Console.WriteLine("SKIPPED LEVEL" + checkingStep.index);
                                            continue;
                                        }
                                        if (checkingStep.requiredSlotPet && client.getChar().next_pet != 4)
                                        {
                                            Console.WriteLine("SKIPPED SLOT" + checkingStep.index);
                                            continue;
                                        }
                                        if (checkingStep.requiredItems.Count > 0)
                                        {
                                            bool isValidate = true;
                                            foreach (KeyValuePair<ushort, ushort> entry in checkingStep.requiredItems)
                                            {
                                                int idxItem = client.getChar().inventory.haveItem(entry.Key);
                                                if (idxItem == -1)
                                                {
                                                    isValidate = false;
                                                    break;
                                                }

                                            }
                                            if (!isValidate)
                                            {
                                                Console.WriteLine("SKIPPED ITEMS" + checkingStep.index);
                                                continue;
                                            }
                                        }
                                        if (checkingStep.requiredNpcs.Count > 0)
                                        {
                                            bool isFound = false;
                                            checkingStep.requiredNpcs.ForEach(npcId =>
                                            {
                                                for (int sl = 0; sl < client.getChar().next_pet; sl++)
                                                    if (client.getChar().pet[sl].NPCid == npcId)
                                                        isFound = true;
                                            });
                                            if (!isFound)
                                            {
                                                Console.WriteLine("SKIPPED NPCS" + checkingStep.index);
                                                continue;
                                            }
                                        }


                                        List<ushort> questRootInSteps = new List<ushort>();
                                        if (checkingStep.requiredQuests.Count > 0)
                                        {
                                            bool isValid = true;

                                            checkingStep.requiredQuests.ForEach(requiredQuest =>
                                            {
                                                //Console.WriteLine("[DEBUG REQUIRED] Root Index >>> " + checkingStep.rootIndex);
                                                //Console.WriteLine("[DEBUG REQUIRED] Index >>> " + checkingStep.index);
                                                //Console.WriteLine("[DEBUG REQUIRED] Q >>> " + requiredQuest.questId);
                                                //Console.WriteLine("[DEBUG REQUIRED] Step >>> " + requiredQuest.stepId);
                                                questRootInSteps.Add(client.findParentQuestId(requiredQuest.questId));
                                                if (requiredQuest.stepId > 0)
                                                {
                                                    int id = -1;
                                                    //haveRequiredNoZero = true;
                                                    // Case special
                                                    if (requiredQuest.bit1 == 1 && requiredQuest.bit2 == 2 && requiredQuest.bit3 == 1)
                                                    {
                                                        id = client.checkQuest(client, requiredQuest.questId, 2);
                                                    }
                                                    else
                                                    {
                                                        id = client.checkQuest(client, requiredQuest.questId, requiredQuest.stepId);
                                                    }

                                                    if (id == -1)
                                                    {
                                                        isValid = false;
                                                    }
                                                }
                                            });
                                            if (!isValid)
                                            {
                                                Console.WriteLine("SKIPPED requiredQuests..." + checkingStep.index);
                                                continue;
                                            }
                                        }
                                        if (questRootInSteps.Count > 0)
                                        {
                                            int totalFinish = client.checkQuest(client, questRootInSteps);

                                            if (totalFinish == questRootInSteps.Count && checkingStep.questReceived.Count(item => checkingStep.childQuestId == item) > 0 && checkingStep.parentQuestId == 0 && checkingStep.childQuestId == 0)
                                            {
                                                Console.WriteLine("SKIPPED all quest are finished...." + checkingStep.index);
                                                continue;
                                            }
                                        }



                                        int isDup = validSteps.FindIndex(step => step.questId == checkingStep.questId);
                                        if (isDup > -1)
                                        {
                                            if (checkingStep.isHaveReceiveQ)
                                            {
                                                Console.WriteLine("Remove this one due to have another Q " + checkingStep.stepId + " | Current: " + validSteps[isDup].stepId + " ROOT " + validSteps[isDup].rootIndex + " INDEX " + validSteps[isDup].index);
                                                validSteps.RemoveAt(isDup);
                                            }
                                        }
                                        validSteps.Add(checkingStep);
                                    }
                                    if (validSteps.Count > 0)
                                    {
                                        int _index = validSteps.FindIndex(tempstep => tempstep.questReceived.Count > 0);
                                        if (_index == -1)
                                        {
                                            _index = validSteps.FindIndex(tempstep => tempstep.requiredQuests.Count > 0);
                                        }
                                        if (_index == -1)
                                        {
                                            _index = 0;
                                        }
                                        Console.WriteLine("INDEX >>>> " + _index);

                                        NewStep step = validSteps[_index];
                                        client.currentStep = step;
                                        Console.WriteLine("Wrap Founded >> ROOT " + client.currentStep.rootIndex + " Index  " + +client.currentStep.index + " Package to send: " + client.currentStep.packageSend.Count());
                                        NewStep targett = step.childs.Find(child => child.targetMapId == end & child.targetMapX == posX & child.targetMapY == posY);
                                        client.warpidFake = targett.warpId;
                                        client.processStep(client);
                                        break;
                                    }
                                }

                            }
                        }
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
                        client.warpid = 0;
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
