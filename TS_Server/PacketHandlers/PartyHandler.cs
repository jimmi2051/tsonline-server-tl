using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class PartyHandler
    {
        public PartyHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    if (client.battle != null)
                    {
                        client.battle.SetBattlePet(client, data);
                    }
                    else
                    {
                        if (client.getChar().setBattlePet(PacketReader.read16(data, 2)))
                            client.reply(new PacketCreator(data).send());
                    }
                    break;
                case 2:
                    // modified by zFan
                    // In battle
                    if (client.battle != null)
                    {
                        client.battle.UnBattlePet(client, data);
                    }
                    else
                    {
                        if (client.getChar().unsetBattlePet())
                            client.reply(new PacketCreator(data).send());
                    }
                    break;
                default:
                    Console.WriteLine("Party Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
