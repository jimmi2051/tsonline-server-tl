using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.DataTools;

namespace TS_Server.Client
{
    public class TSItemContainer
    {
        public TSItem[] items;
        public TSCharacter owner;
        public int next_item;
        public int capacity;

        public TSItemContainer(TSCharacter c, byte cap)
        {
            owner = c;
            capacity = cap;
            items = new TSItem[capacity];
            next_item = 0;
        }

        public void loadContainer(byte[] data)
        {
            int pos = 0;
            ushort itemid;
            while (pos < data.Length)
            {
                if (data[pos] != 0)
                {
                    itemid = (ushort)(data[pos + 1] + (data[pos + 2] << 8));
                    if (ItemData.itemList[itemid].equippos == 0)
                    {
                        items[data[pos] - 1] = new TSItem(this, itemid, data[pos], data[pos + 3]);
                        pos += 4;
                    }
                    else
                    {
                        items[data[pos] - 1] = new TSEquipment(this, itemid, data[pos], 1);
                        items[data[pos] - 1].equip.duration = data[pos + 3];
                        items[data[pos] - 1].equip.elem_type = data[pos + 4];
                        items[data[pos] - 1].equip.elem_val = data[pos + 5] + (data[pos + 6] << 8);
                        pos += 7;
                    }
                }
                else
                    break;
            }
            nextSlot();         
        }

        public byte[] saveContainer()
        {
            var data = new byte[capacity*10];
            int pos = 0;
                for (int i = 0; i < capacity; i++)
                    if (items[i] != null)
                        if (items[i].equip == null)
                            items[i].generateItemBinary(ref data, ref pos);
                        else
                            items[i].equip.generateEquipBinary(ref data, ref pos);
            return data;
        }

        public void sendItems(byte opcode, byte subcode) //0x17, 5 for invent, 0x1e, 0x1 for storage, 0x017, 0x2f for bag
        {
                var p = new PacketCreator(opcode, subcode);
                for (int i = 0; i < items.Length; i++)
                    if (items[i] != null)
                    {
                        p.addByte((byte)(i + 1));
                        p.add16(items[i].Itemid);
                        p.addByte(items[i].quantity);
                        p.addByte(items[i].duration); // p.addZero(7);
                        p.add8(0);
                        p.add8(0);
                        p.add8(0); // de khang สภาพ
                        p.add8(0x64); //bat chuoc DVT, cha hieu la gi ลอกเลียนแบบ DVT ไม่เข้าใจว่าจะเป็นอะไร
                        p.add8(0); // elem ++
                        p.add8(0); // elem ++
                        p.add8(0); // elem ++
                    }
                owner.reply(p.send());
        }
        
        public void addItem(ushort itemid, ushort qtty, bool newItem)
        {
            byte qty = 0;
            qtty = (qtty > 1250) ? (ushort)(1250) : qtty;

            if (qtty <= 50)
                qty = (byte)qtty;
            else
            {
                addItem(itemid, 50, newItem);
                qtty -= 50;
                if (qtty == 0)
                    return;
                addItem(itemid, qtty, newItem);
            }
            int i = itemAddable(itemid, qty);

            if (i == capacity) //add to new empty slot เพิ่มช่อง
            {
                if (ItemData.itemList[itemid].equippos != 0)
                    items[next_item] = new TSEquipment(this, itemid, (byte)(next_item + 1), 1);
                else
                    items[next_item] = new TSItem(this, itemid, (byte)(next_item + 1), qty);
                Console.WriteLine("item " + itemid + " added in slot " + (next_item + 1) + " x " + qty + "pcs");

                if (newItem) items[next_item].sendNewItem(qty);
                nextSlot();
            }

            else if (i >= 0 && i < capacity) //stack to existed slot
            {
                items[i].quantity += qty;
                Console.WriteLine("item " + itemid + " stacked in slot " + (i + 1) + "nextitem=" + next_item);
                if (newItem) items[i].sendNewItem(qty);
            }
        }

        public void moveItem(byte init_slot, byte qty, byte dest_slot) //pain in the assssss
        {
            TSItem init = items[init_slot - 1];
            TSItem dest = items[dest_slot - 1];
            if (init == null) return;
            if (dest == null) // empty slot
                if (ItemData.itemList[init.Itemid].equippos != 0 || qty >= init.quantity) // move entire slot
                {
                    items[init_slot - 1] = null;
                    items[dest_slot - 1] = init;
                    init.slot = dest_slot;
                }
                else //split to new slot
                {
                    items[dest_slot - 1] = new TSItem(this, init.Itemid, dest_slot, qty);
                    init.quantity -= qty;
                }
            else if (init.Itemid != dest.Itemid || ItemData.itemList[init.Itemid].equippos != 0
                || ItemData.itemList[dest.Itemid].equippos != 0) //not empty slot with different item
                return;
            else //not empty slot with same item
            {                
                byte q = qty <= 50 - dest.quantity ? qty : (byte)(50 - dest.quantity); // destination slot maximum 50
                init.quantity -= q;
                dest.quantity += q;
                init.checkQuantity();
            }

            owner.reply(new PacketCreator(new byte[] { 0x17, 0xa, init_slot, qty, dest_slot }).send());
            nextSlot();
        }

        public void dropItem(byte slot, byte qty)
        {
            if (items[slot - 1].quantity > qty)
                items[slot - 1].quantity -= qty;
            else
            {
                items[slot - 1] = null;
                nextSlot();
            }
            owner.reply(new PacketCreator(new byte[] { 0x17, 9, slot, qty }).send());
        }

        public int itemAddable(ushort itemid, byte qty) //return the (slot-1) where new item is stackable, return capacity if need new empty slot
        {
            if (!ItemData.itemList.ContainsKey(itemid))
                return 0;
            if (ItemData.itemList[itemid].equippos == 0)
            {
                int i = haveItem(itemid);
                if (i >= 0 && qty + items[i].quantity <= 50)
                    return i;
            }
            if (next_item >= capacity)
                return -1;
            return capacity;
        }

        public int haveItem(ushort itemid)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null)
                    if (items[i].Itemid == itemid && items[i].quantity < 50)
                        return i;
            return -1;
        }

        public byte getItemById(ushort itemid)
        {
            for (byte i = 0; i < items.Length; i++)
                if (items[i] != null)
                    if (items[i].Itemid == itemid)
                        return i;
            return (byte)items.Length;
        }

        public void destroyItem(byte slot)
        {
            items[slot - 1] = null;
            nextSlot();
        }

        public void nextSlot()
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i] == null)
                {
                    next_item = i;
                    Console.WriteLine("Next item : " + i);
                    return;
                }
        }
    }
}
