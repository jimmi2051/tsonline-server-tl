using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class MoveHandler
    {
        public MoveHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1: //normal movement
                    //System.Threading.Thread.Sleep(250);
                    ushort x = PacketReader.read16(data, 3);
                    ushort y = PacketReader.read16(data, 5);
                    client.getChar().orient = data[2];
                    client.map.movePlayer(client, x, y);
                    break;
                default:
                    Console.WriteLine("MoveHandler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
