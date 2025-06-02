using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class ModifyStatHandler
    {
        public ModifyStatHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    client.getChar().setStat(data[4], (int)PacketReader.read16(data, 5));
                    break;
                default:
                    Console.WriteLine("Modify Stat Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
