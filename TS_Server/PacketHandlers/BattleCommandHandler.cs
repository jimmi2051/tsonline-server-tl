using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class BattleCommandHandler
    {
        public BattleCommandHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    client.battle.registerCommand(client, data, 0);
                    break;
                case 2: //use item
                    client.battle.registerCommand(client, data, 1);
                    break;
            }
        }
    }
}
