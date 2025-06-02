using System;
using TS_Server.DataTools;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class PetManipHandler
    {
        public PetManipHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 2: // remove pet
                    client.getChar().removePet(data[2]);
                    break;
                case 4: //riding yay
                    ushort horseid = PacketReader.read16(data,2);
                    client.getChar().rideHorse(true, horseid);
                    break;

                case 5: // Xuong ngua
                    client.getChar().rideHorse(false);
                    break;
                case 6://change name
                    var petIndex = data[2];
                    client.getChar().changePetName(petIndex, PacketReader.toByteArray(data,3,data.Length - 3));
                    break;
                default:
                    Console.WriteLine("Pet Manip Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
