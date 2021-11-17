using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;

namespace TS_Server.PacketHandlers
{
    class ExpressHandler
    {
        public ExpressHandler(TSClient client, byte[] data)
        {
            byte expressType;
            byte expressCode;
            try
            {
                expressType = data[1];
                expressCode = data[2];
                byte[] expr = client.getChar().setExpress(expressType, expressCode);
                client.getChar().replyToMap(expr, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
