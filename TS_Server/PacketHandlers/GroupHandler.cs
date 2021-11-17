using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class GroupHandler
    {
        public GroupHandler(TSClient client, byte[] data)
        {
            int subleaderId = 0;
            int teamleaderId = 0;
            int groupCommandType = data[1];
            switch (groupCommandType)
            {
                // Member request
                case 1:
                    {
                        TSCharacter requester = client.getChar();
                        uint target_id = PacketReader.read32(data, 2);
                        TSCharacter target = TSServer.getInstance().getPlayerById((int)target_id).getChar();

                        // current player is in battle
                        // Requester not join team
                        if (!requester.isJoinedTeam())
                        {
                            if (target != null)
                            {
                                var p = new PacketCreator(0x0D);
                                p.add8(0x01);
                                p.add32((uint)client.accID);
                                target.reply(p.send());
                            }
                        }
                    }
                    break;
                case 2:
                    break;
                // Leader response
                case 8:
                case 3:
                    {
                        byte response = data[2];
                        uint memberId = PacketReader.read32(data, 3);
                        TSCharacter member = TSServer.getInstance().getPlayerById((int)memberId).getChar();

                        // Check joined team or in battle
                        ///- Notify response to requester
                        //if (!member.isJoinedTeam() || !member.isTeamLeader())

                        var p = new PacketCreator(0x0D);
                        if (groupCommandType == 0x03)
                            p.add8((byte)groupCommandType);
                        else
                            p.add8((byte)0x0A);
                        p.add8(response);

                        if (response == 0x02)
                        {
                            p.add32((uint)client.accID);
                            member.reply(p.send());
                        }
                        else
                        {
                            p.add32((uint)memberId);
                            client.reply(p.send());
                        }

                        switch (response)
                        {
                            case 0x01: // accepted
                                {
                                    uint leader_id = 0;
                                    uint member_id = 0;
                                    TSParty party = null;
                                    TSCharacter player = client.getChar();

                                    // Requester is leader
                                    if (member.party != null && member.isTeamLeader())
                                    {
                                        if (!member.party.canJoin())
                                        {
                                            // team is full
                                        }
                                        else
                                        {
                                            member.party.member.Add(player);
                                            player.party = member.party;
                                        }
                                        party = member.party;
                                        leader_id = member.client.accID;
                                        member_id = player.client.accID;

                                        //if (player.isJoinedTeam())
                                        //member.sendUpdateTeam();
                                    }
                                    else
                                    {
                                        if (player.party == null)
                                        {
                                            party = new TSParty(player, member);
                                            member.party = party;
                                            player.party = party;
                                        }
                                        else
                                        {
                                            party = player.party;
                                            player.party.member.Add(member);
                                            member.party = player.party;
                                        }
                                        leader_id = player.client.accID;
                                        member_id = member.client.accID;

                                        //player.sendUpdateTeam();
                                    }

                                    // Notify to everyone
                                    p = new PacketCreator(0x0D);
                                    p.add8(0x05);
                                    p.add32(leader_id);
                                    p.add32(member_id);
                                    member.replyToMap(p.send(), true);

                                    // Refresh
                                    foreach (TSCharacter c in member.party.member)
                                    {
                                        c.refreshTeam();
                                    }

                                    break;
                                }
                            case 0x02: // rejected
                            case 0x03: // no response
                            default:
                                break;
                        }
                    }
                    break;
                // Member leave
                case 4:
                    {
                        teamleaderId = (int)PacketReader.read32(data, 2);
                        if (client.getChar().isTeamLeader())
                        {
                            // Dimiss Team
                            client.getChar().party.Disband(client.getChar());
                        }
                        else
                        {
                            // Remove 
                            client.getChar().party.LeaveTeam(client.getChar());
                        }
                    }
                    break;
                // Set subleader
                case 5:
                    subleaderId = (int)PacketReader.read32(data, 2);
                    client.getChar().party.SetSubleader(subleaderId, client);
                    break;
                // Unset subleader
                case 6:
                    subleaderId = (int)PacketReader.read32(data, 2);
                    client.getChar().party.unSetSubLeader(subleaderId, client);
                    break;
                // Ask player to join and be team leader
                case 7:
                    {
                        UInt32 target_id = PacketReader.read32(data, 2);
                        TSCharacter requester = client.getChar();
                        TSCharacter target = TSServer.getInstance().getPlayerById((ushort)target_id).getChar();

                        if (requester.isTeamLeader())
                        {
                            if (target != null)
                            {
                                var p = new PacketCreator(0x0D);
                                p.add8(0x09);
                                p.add32((uint)client.accID);
                                target.reply(p.send());
                            }
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Group Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
