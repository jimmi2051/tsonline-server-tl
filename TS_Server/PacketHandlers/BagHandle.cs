using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TS_Server.Client;
namespace TS_Server.PacketHandlers
{
    class BagHandle
    {
        public BagHandle(TSClient client, byte[] data)
        {
            TSCharacter chr = client.getChar();
            switch (data[1])
            {
                case 0x24: //Item In
                    int countbag=25;
                    PacketCreator psendbag;
                    for (int i = 0; i < 25; i++)
                    {
                        if (chr.bag.items[i] != null)
                            countbag--;

                    }

                        byte[] slotinsto = new byte[data.Length - 2];
                    
                        for (int i = 2; i < data.Length; i++)
                        {
                            slotinsto[i - 2] = data[i];
                        }
                        for (int i = 0; i < slotinsto.Length; i++)
                        {
                            if (i + 1 <= countbag)
                            {
                                //clear chonsen
                                psendbag = new PacketCreator(0x17, 0x32);
                                client.reply(psendbag.send());
                                psendbag = new PacketCreator(0x17, 0x9);
                                psendbag.add16(data[i + 2]);
                                client.reply(psendbag.send());
                                //clear item in inv

                                psendbag = new PacketCreator(0x1e, 2);
                                psendbag.addByte(data[i + 2]);
                                psendbag.send();
                                psendbag = new PacketCreator(0x17, 9);
                                psendbag.addByte(data[i + 2]);
                                psendbag.add16(chr.inventory.items[data[i + 2] - 1].quantity);
                                client.reply(psendbag.send());


                                ///////////additem in bag
                                psendbag = new PacketCreator(0x17, 0x30);
                                psendbag.add16(chr.inventory.items[data[i + 2] - 1].Itemid);
                                psendbag.add16(chr.inventory.items[data[i + 2] - 1].quantity);
                                psendbag.addByte(0);
                                psendbag.addByte(0);
                                psendbag.addByte(0);
                                psendbag.addByte(0);//de` khang cay tinh
                                psendbag.addByte(0);
                                psendbag.addByte(0);
                                psendbag.addByte(0);
                                psendbag.addByte(0);
                                client.reply(psendbag.send());




                                //process bag and inv
                                chr.bag.addItem(chr.inventory.items[data[i + 2] - 1].Itemid, chr.inventory.items[data[i + 2] - 1].quantity, false);
                                chr.inventory.destroyItem(data[i + 2]);
                            }
                            else
                            {
                               
                                //clear chonsen
                                psendbag = new PacketCreator(0x17, 0x32);
                                client.reply(psendbag.send());
                                psendbag = new PacketCreator(0x17, 0x9);
                                psendbag.add16(data[i + 2]);
                                client.reply(psendbag.send());
                                break;
                            }
                        }
                    break;
                case 0x25: //Item Out
                    PacketCreator getitem;
                       int countinv=25;
                    for (int i = 0; i < 25; i++)
                    {
                        if (chr.inventory.items[i] != null)
                            countinv--;
                    }
                        byte[] slotinbag = new byte[data.Length - 2];

                        for (int i = 2; i < data.Length; i++)
                        {
                            slotinbag[i - 2] = data[i];
                        }
                        for (int i = 0; i < slotinbag.Length; i++)
                        {
                            if (i + 1 <= countinv)
                            {
                                //clear chonsen
                                getitem = new PacketCreator(0x17, 0x32);
                                client.reply(getitem.send());


                                //clear item in bag
                                getitem = new PacketCreator(0x17, 0x31);
                                getitem.add16(data[i + 2]);
                                client.reply(getitem.send());


                                //add item in inv
    
                                getitem = new PacketCreator(0x17, 0x06);
                                getitem.add16(chr.bag.items[data[i + 2] - 1].Itemid);
                                getitem.add16(chr.bag.items[data[i + 2] - 1].quantity);
                                Console.WriteLine("id=" + chr.bag.items[data[i + 2] - 1].Itemid + " qt=" + chr.bag.items[data[i + 2] - 1].quantity);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                getitem.addByte(0);
                                client.reply(getitem.send());

                                chr.inventory.addItem(chr.bag.items[data[i + 2] - 1].Itemid, chr.bag.items[data[i + 2] - 1].quantity, false);
                                chr.bag.destroyItem(data[i + 2]);
                            }
                            else
                            {
                                //clear chonsen
                                getitem = new PacketCreator(0x17, 0x32);
                                client.reply(getitem.send());
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
