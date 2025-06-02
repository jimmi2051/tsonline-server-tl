using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class ModifySkillHandler
    {
        public ModifySkillHandler(TSClient client, byte[] data)
        {
            int i = 2;
            switch (data[1])
            {
                case 1: //char       
                    while (i < data.Length)
                    {
                        client.getChar().setSkill(PacketReader.read16(data, i), data[i + 2]);
                        i += 3;
                    }
                    break;
                case 2: //pet                   
                    i++;
                    while (i < data.Length)
                    {
                        client.getChar().pet[data[2] - 1].setSkill(PacketReader.read16(data, i), data[i + 2]);
                        i += 3;
                    }
                    break;
                case 5: //skill TS
                    client.getChar().setSkillRb2(data);
                    break;
                default:
                    Console.WriteLine("Modify Stat Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
