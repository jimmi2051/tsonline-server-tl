using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TS_Server.PacketHandlers;
using TS_Server.Client;

namespace TS_Server
{
    class PacketProcessor
    {
        public Socket socket;
        public TSClient client;
        public byte[] DataBuff = new byte[1024];
        public int byte_remain = 512;

        public PacketProcessor(Socket s, TSClient c)
        {
            socket = s;
            client = c;
        }

        public int gatherPacket()
        {
            int n = 0; // offset
            int packet_len;
            while (true)
            {
                //Console.WriteLine("DataBuff " + BitConverter.ToString(DataBuff));
                if (DataBuff[n] != 0x59 || DataBuff[n + 1] != 0xE9)
                    break;
                packet_len = (UInt16)((DataBuff[n + 2] ^ 0xAD) + (DataBuff[n + 3] ^ 0xAD) * 255);
                byte[] data = new byte[packet_len];

                if (n + packet_len + 4 >= DataBuff.Length)
                    break;

                for (int i = 4; i < packet_len + 4; i++)
                    data[i - 4] = (byte)(DataBuff[n + i] ^ 0xAD);
                Console.WriteLine("Recv " + BitConverter.ToString(data));

                processPacket(data);

                n = n + packet_len + 4;
                if (n + 4 >= DataBuff.Length)
                    break;
            }
            if (n == 0)
                return 1;
            return 0;
        }

        public int processPacket(byte[] data)
        {
            byte cmd = data[0];
            Console.WriteLine("cmd:" + cmd);
            switch (cmd)
            {
                case 0:
                    //Console.WriteLine("Request Login");
                    new RequestLogin(client);
                    break;
                case 1:
                    //Console.WriteLine("Authentication");
                    new Authentication(client,data);
                    break;
                case 2:
                    new ChatHandler(client, data);
                    break;
                case 5: //some weird packet 05 06 0000000000000000000 of client appears that flood the buffer, have to fix later
                    new ActionHandler(client, data);
                    //client.continueMoving();
                    break;
                case 6:
                    new MoveHandler(client, data);
                    break;
                case 8:
                    new ModifyStatHandler(client, data);
                    break;
                case 9:
                    //Console.WriteLine("Char creation in process");
                    new CreateChar(client, data);
                    break;
                case 0x0b: // battle fuck yeah
                    new BattleHandler(client, data);
                    break;
                case 0x0c:
                    //Console.WriteLine("Relocation");
                    new RelocateHandler(client, data);
                    break;
                case 0xd: //Group relate
                    new GroupHandler(client, data);
                    break;
                case 0x0f: //pet manip
                    new PetManipHandler(client, data);
                    break;
                case 0x13: //party relate (19)
                    new PartyHandler(client, data);
                    break;
                case 0x14:
                    //Console.WriteLine("Player Action");
                    new ActionHandler(client, data);
                    break;
                case 0x17:
                    new ItemHandler(client, data);
                    break;
                // Buy item in shop npc
                case 0x1b:
                    Console.WriteLine("Here >>> BUY ITEM");
                    client.continueMoving();
                    break;
                case 0x1c:
                    new ModifySkillHandler(client, data);
                    break;
                case 0x1e:
                    new StorageHandler(client, data);
                    break;
                case 0x20:
                    new ExpressHandler(client, data);
                    break;
                case 0x21:
                    new WelcomeHandler(client, data);
                    break;
                case 0x22:
                    Console.WriteLine("here >>>> ");
                    client.continueMoving();
                    break;
                case 0x28:
                    new HotkeyHandler(client, data);
                    break;
                case 0x2c:
                    new RebornPetHandler(client, data);
                    break;
                case 0x32: //battle command
                    new BattleCommandHandler(client, data);
                    break;
                case 14: //send mail
                    // sub_op_code == 1 : send to GM
                    break;
                default:
                    Console.WriteLine("Unknown cmd:" + cmd);
                    break;
            }
            return 0;
        }
    }
}
