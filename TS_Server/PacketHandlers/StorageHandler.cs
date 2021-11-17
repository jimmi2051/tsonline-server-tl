using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TS_Server.Client;
namespace TS_Server.PacketHandlers
{
    class StorageHandler
    {
        public StorageHandler(TSClient client, byte[] data)
        {
            TSCharacter chr = client.getChar();
            switch (data[1])
            {
                case 1: //Item Out
                    PacketCreator getitem;
                           int countinv=25;
                    for (int i = 0; i < 25; i++)
                    {
                        if (chr.inventory.items[i] != null)
                            countinv--;
                    }
               byte[] slotinsto1 = new byte[data.Length - 2];

                        for (int i = 2; i < data.Length; i++)
                        {
                            slotinsto1[i - 2] = data[i];
                        }
                        for (int i = 0; i < slotinsto1.Length; i++)
                        {
                            if (i + 1 <= countinv)
                            {
                                //clear item in sto
                             
                                getitem = new PacketCreator(0x1e, 0x01);
                                getitem.addByte(data[i + 2]);//slot 
                                getitem.send();

                                getitem = new PacketCreator(0x1e, 0x05);
                                getitem.addByte(data[i + 2]);
                                getitem.addByte(50);
                                client.reply(getitem.send());

                                //additem to inv
                                getitem = new PacketCreator(0x17, 0x06);
                                getitem.add16(chr.storage.items[data[i + 2] - 1].Itemid);
                                getitem.add16(chr.storage.items[data[i + 2] - 1].quantity);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                client.reply(getitem.send());


                                //process sto and inv
                                chr.inventory.addItem(chr.storage.items[data[i + 2] - 1].Itemid, chr.storage.items[data[i + 2] - 1].quantity, false);
                                chr.storage.destroyItem(data[i + 2]);
                            }
                            else
                            {
                                getitem = new PacketCreator(0x1e, 0x01);
                                getitem.addByte(data[i + 2]);//slot 
                                getitem.send();
                                break;
                            }

                        }
                    break;
                case 2: //Item In
                    PacketCreator sendthungdo;
                          int countsto=50;
                    for (int i = 0; i < 50; i++)
                    {
                        if (chr.storage.items[i] != null)
                            countsto--;

                    }
                        byte[] slotinsto = new byte[data.Length - 2];
                   
                        for (int i = 2; i < data.Length; i++)
                        {
                            slotinsto[i - 2] = data[i];
                        }
                        for (int i = 0; i < slotinsto.Length; i++)
                        {
                            if (i + 1 <= countsto)
                            {
                                //clear chosen
                         
                                sendthungdo = new PacketCreator(0x1e, 2);
                                sendthungdo.addByte(data[i + 2]);
                                sendthungdo.send();
                                //clear item in inv
                           
                                sendthungdo = new PacketCreator(0x17, 9);
                                sendthungdo.addByte(data[i + 2]);//slot
                                sendthungdo.add16(50);//so luong
                                client.reply(sendthungdo.send());


                                //additem in sto
                                sendthungdo = new PacketCreator(0x1e, 2);
                                sendthungdo.add16(chr.inventory.items[data[i + 2] - 1].Itemid);
                                sendthungdo.add16(chr.inventory.items[data[i + 2] - 1].quantity);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                sendthungdo.addByte(0);
                                client.reply(sendthungdo.send());

                                //process sto and inv
                                chr.storage.addItem(chr.inventory.items[data[i + 2] - 1].Itemid, chr.inventory.items[data[i + 2] - 1].quantity, false);
                                chr.inventory.destroyItem(data[i + 2]);
                            }
                            else
                            {
                                sendthungdo = new PacketCreator(0x1e, 2);
                                sendthungdo.addByte(data[i + 2]);
                                sendthungdo.send();
                                break;
                            }
                          
                        }


                    break;
                default:
                    break;
            }
        }
    }
    
}
