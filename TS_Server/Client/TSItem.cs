using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TS_Server.DataTools;

namespace TS_Server.Client
{
    public class TSItem
    {
        public TSItemContainer container;
        public byte slot;
        public ushort Itemid;
        public TSEquipment equip = null;
        public byte quantity;
        public byte duration;

        public TSItem()
        {
        }

        public TSItem(TSItemContainer ic, ushort id, byte sl, byte qty)
        {
            container = ic;
            Itemid = id;
            quantity = qty;
            duration = 0;
            slot = sl;
        }

        public void useItemChar(byte qty, TSCharacter c)
        {
            byte q = qty >= quantity ? quantity : qty;

            Console.WriteLine("Unknown prop1: " + ItemData.itemList[Itemid].prop1 + " prop2: " + ItemData.itemList[Itemid].prop2);

            switch (ItemData.itemList[Itemid].prop1)
            {
                case 25:
                    c.setHp(ItemData.itemList[Itemid].prop1_val * q);
                    c.refresh(c.hp, 0x19);
                    break;
                case 26:
                    c.setSp(ItemData.itemList[Itemid].prop1_val * q);
                    c.refresh(c.sp, 0x1a);
                    break;
                default:
                    break;
            }

            switch (ItemData.itemList[Itemid].prop2)
            {
                case 25:
                    c.setHp(ItemData.itemList[Itemid].prop2_val * q);
                    c.refresh(c.hp, 0x19);
                    break;
                case 26:
                    c.setSp(ItemData.itemList[Itemid].prop2_val * q);
                    c.refresh(c.sp, 0x1a);
                    break;
                default:
                    break;
            }
            switch (ItemData.itemList[Itemid].unk3)
            {
                case 5: // ตุ๊กตา
                    ushort npcid = NpcData.npcList.Where(n => n.Value.drop4 == Itemid).Select(n => n.Key).FirstOrDefault();
                    if (npcid == 0)
                    {
                        npcid = NpcData.npcList.Where(n => n.Value.drop5 == Itemid).Select(n => n.Key).FirstOrDefault();
                    }
                    container.owner.outfitId = npcid;
                    container.owner.showOutfit();
                    break;
                default:
                    break;
            }
            container.owner.reply(new PacketCreator(new byte[] { 0x17, 9, slot, q }).send());
            container.owner.reply(new PacketCreator(0x17, 0xf).send());
            quantity -= q;
            checkQuantity();
        }

        public void useItemPet(byte qty, TSPet p)
        {
            byte q = qty >= quantity ? quantity : qty;

            switch (ItemData.itemList[Itemid].prop1)
            {
                case 25:
                    p.setHp(ItemData.itemList[Itemid].prop1_val);
                    p.refresh(p.hp, 0x19);
                    break;
                case 26:
                    p.setSp(ItemData.itemList[Itemid].prop1_val);
                    p.refresh(p.sp, 0x1a);
                    break;
                case 64:
                    p.setFai(ItemData.itemList[Itemid].prop1_val);
                    break;
                default:
                    break;
            }

            switch (ItemData.itemList[Itemid].prop2)
            {
                case 25:
                    p.setHp(ItemData.itemList[Itemid].prop2_val);
                    p.refresh(p.hp, 0x19);
                    break;
                case 26:
                    p.setSp(ItemData.itemList[Itemid].prop2_val);
                    p.refresh(p.sp, 0x1a);
                    break;
                default:
                    break;
            }
            container.owner.reply(new PacketCreator(new byte[] { 0x17, 9, slot, q }).send());
            container.owner.reply(new PacketCreator(0x17, 0xf).send());
            quantity -= q;
            checkQuantity();
        }

        public void checkQuantity()
        {
            if (quantity == 0)
                container.destroyItem(slot);
        }

        public void sendNewItem(byte qty)
        {
            PacketCreator p = new PacketCreator(0x17, 6);
            p.add16(Itemid);
            p.add8(qty);
            p.addZero(9);
            //p.addByte(0x64);
            //p.addZero(5);
            container.owner.reply(p.send());
        }

        public void generateItemBinary(ref byte[] data, ref int pos)
        {
            data[pos] = slot;
            data[pos + 1] = (byte)Itemid; data[pos + 2] = (byte)(Itemid >> 8);
            data[pos + 3] = quantity;
            pos += 4;
        }

    }
}
