using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Server;

namespace TS_Server.Client
{
    public class TSParty
    {
        public int leader_id;
        public int subleader_id;
        public List<TSCharacter> member;

        public TSParty(TSCharacter leader, TSCharacter mem)
        {
            leader_id = (int)leader.client.accID;

            member = new List<TSCharacter>();
            member.Add(leader);
            member.Add(mem);
        }

        public bool canJoin()
        {
            if (member.Count < 5)
                return true;
            else
                return false;
        }
        public bool isMember(TSCharacter c)
        {
            bool exist = false;
            foreach (TSCharacter chr in member)
            {
                if (c == chr)
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }

        public void SetSubleader(int accID, TSClient client)
        {
            if (!client.getChar().isTeamLeader())
                return;

            TSCharacter sub = TSServer.getInstance().getPlayerById(accID).getChar();
            this.subleader_id = accID;

            //if(sub != null && sub.isJoinedTeam() && isMember(sub))
            UpdateTeamSub(client);
        }

        public void unSetSubLeader(int accID, TSClient client)
        {
            TSCharacter sub = TSServer.getInstance().getPlayerById(accID).getChar();

            /*
            if (!client.getChar().isTeamLeader())
                return;

            if (this.subleader_id != accID)
                return;
            */

            if (sub != null && accID == sub.client.accID)
            {

                var p = new PacketCreator(0x0D);
                p.add8(8); p.add32((UInt32)sub.client.accID);
                client.getChar().replyToMap(p.send(), true);

                p = new PacketCreator(0x0D);
                p.add8(0x0C); p.add32((UInt32)sub.client.accID);
                client.getChar().replyToMap(p.send(), true);

                ///- Revoke It
                this.subleader_id = 0;
            }
        }

        public void LeaveTeam(TSCharacter mem)
        {
            if (mem != null)
            {
                // Update Team  
                if (subleader_id == mem.client.accID)
                {
                    unSetSubLeader((int)mem.client.accID, mem.client);
                }

                mem.sendUpdateTeam();
                var p = new PacketCreator(0x0D);
                p.add8(0x04); p.add32((uint)mem.client.accID);
                mem.replyToMap(p.send(), true);

                member.Remove(mem);
                mem.party = null;

                if (member.Count == 1)
                {
                    // Disband
                    TSCharacter leader = TSServer.getInstance().getPlayerById(leader_id).getChar();
                    leader.sendUpdateTeam();
                    Disband(leader);
                }
            }
        }
        public void Disband(TSCharacter leader)
        {
            if (leader.isTeamLeader())
            {
                // Unset Subleader
                //TSClient subleader = TSServer.getInstance().getPlayerById(subleader_id);
                //unSetSubLeader(subleader_id, subleader);

                var p = new PacketCreator(0x18, 0x05);
                p.add8(0x02); p.add16(0);
                leader.replyToMap(p.send(), true);

                p = new PacketCreator(0x18, 0x05);
                p.add8(0x62); p.add8(0x01); p.add8(0x00);
                leader.replyToMap(p.send(), true);

                p = new PacketCreator(0x18, 0x05);
                p.add8(0x91); p.add8(0x01); p.add8(0x00);
                leader.replyToMap(p.send(), true);

                p = new PacketCreator(0x0D, 0x04);
                p.add32(leader.client.accID);
                leader.replyToMap(p.send(), true);

                //member.Reverse();
                while (member.Count > 1)
                {
                    TSCharacter c = member[member.Count - 1];
                    LeaveTeam(c);
                }

                // member.Remove(leader);
                //leader_id = 0;
                leader.party = null;
            }
        }

        public void UpdateTeamSub(TSClient client)
        {
            uint sub_id = 0;
            if (subleader_id > 0)
            {
                TSCharacter sub = TSServer.getInstance().getPlayerById(subleader_id).getChar();

                if (sub != null)
                {
                    sub_id = (uint)sub.client.accID;

                    ///- Update to leader only
                    var p = new PacketCreator(0x0D);
                    p.add8(8); p.add32(sub_id);
                    client.reply(p.send());

                    ///- Update to member
                    p = new PacketCreator(0x0D);
                    p.add8(7); p.add32(sub_id);
                    client.getChar().replyToTeam(p.send());

                    ///- Update to member
                    p = new PacketCreator(0x0D);
                    p.add8(0x0B); p.add32(sub_id);
                    client.getChar().replyToTeam(p.send());
                }
            }
        }
    }
}
