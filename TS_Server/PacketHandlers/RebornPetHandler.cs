using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    public class RebornPetHandler
    {
        public RebornPetHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    client.getChar().rebornPet(1,data[2]);
                    break;
                case 3:
                    client.getChar().rebornPet(2, data[2]);
                    break;
                default:
                    Console.WriteLine("Pet Reborn Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
