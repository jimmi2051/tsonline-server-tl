using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class Authentication
    {
        private uint accID;
        public Authentication(TSClient client, byte[] data)
        {
            byte pw_len = data[1];
            accID = PacketReader.read32(data, 2);
            uint patchVer = PacketReader.read32(data, 6);
            string passwd = PacketReader.readString(data, 10, pw_len);

            Console.WriteLine("User: " + accID + " Pass: " + passwd + " Patch: " + patchVer);

            int ret = client.checkLogin(accID, passwd);
            PacketCreator p = new PacketCreator();

            switch (ret)
            {
                case 0:
                    client.getChar().loginChar();
                    break;
                case 1: //wrong info
                    p.addByte(1); p.addByte(6);
                    client.reply(p.send());
                    break;
                case 2: //acc online
                    p.addByte(0); p.addByte(0x13);
                    client.reply(p.send());
                    break;
                case 3: //create char
                    if (!client.creating)
                    {
                        p.addByte(1); p.addByte(3); p.addByte(0);
                        client.reply(p.send());
                        client.creating = true;
                    }
                    break;
                default: //ok
                    break;
            }
        }           
    }
}
