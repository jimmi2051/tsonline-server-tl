using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class WelcomeHandler
    {
        public WelcomeHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    {
                        PacketCreator p = new PacketCreator(0x21, 1);
                        p.add8(data[2]);
                        client.map.BroadCast(client, data, true);
                        client.reply(p.send());
                        break;
                    }
                case 2:
                    {
                        PacketCreator p = new PacketCreator(0x21, 2);
                        p.add8(data[2]);
                        client.reply(p.send());
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
