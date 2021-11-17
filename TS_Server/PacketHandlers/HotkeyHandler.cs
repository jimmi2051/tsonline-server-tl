using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class HotkeyHandler
    {
        public HotkeyHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1: //hotkey manager
                    if (data[2] == 0)
                        client.getChar().hotkey[data[5] - 1] = 0;
                    else if (data[2] == 2)
                        client.getChar().hotkey[data[5] - 1] = (ushort)(data[3] + (data[4] << 8));
                    Console.WriteLine("hotkey " + +data[5] + " " + (data[3] + (data[4] << 8)));
                    break;
                default:
                    Console.WriteLine("Hotkey Handler : unknown subcode" + data[1]);                    
                    break;                    
            }
            
        }
    }
}
