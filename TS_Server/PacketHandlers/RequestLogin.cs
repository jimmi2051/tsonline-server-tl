using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class RequestLogin
    {
        public RequestLogin(TSClient client)
        {
            if (client.isOnline())
                return;

            PacketCreator p1 = new PacketCreator();
            p1.addByte(1);
            p1.addByte(9);
            p1.addByte(1);
            client.reply(p1.send());

        }
    }
}
