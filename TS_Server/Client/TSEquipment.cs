using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TS_Server.DataTools;

namespace TS_Server.Client
{
    public class TSEquipment:TSItem
    {
        public byte elem_type;
        public int elem_val = 0;
        public uint exp = 0;
        public byte level = 0;
        public TSCharacter char_owner = null;
        public TSPet pet_owner = null;
        
        public TSEquipment(TSItemContainer ic, ushort id, byte sl, byte qty)
        {
            container = ic;
            Itemid = id;
            quantity = 1;
            slot = sl;
            equip = this;
            elem_type = ItemData.itemList[Itemid].elem_type;
            if (elem_type != 0)
                elem_val = ItemData.itemList[Itemid].elem_val;
        }

        public void equipOnChar()
        {
            int s = ItemData.itemList[Itemid].equippos;
            TSCharacter owner = container.owner;
            if (owner.level < ItemData.itemList[Itemid].level || s == 0)
                return;
            if (owner.equipment[s - 1] != null) //exchange equip
            {
                TSEquipment old_equip = owner.equipment[s - 1];
                container.items[slot - 1] = old_equip;
                old_equip.slot = slot;
                old_equip.container = container;
                old_equip.char_owner = null;
                owner.addEquipBonus(ItemData.itemList[old_equip.Itemid].prop1, ItemData.itemList[old_equip.Itemid].prop1_val,1);
                owner.addEquipBonus(ItemData.itemList[old_equip.Itemid].prop2, ItemData.itemList[old_equip.Itemid].prop2_val,1);
            }
            else
            {
                container.items[slot - 1] = null;
                owner.nb_equips++;
                container.nextSlot();
            }
            owner.equipment[s - 1] = this;
            byte old_slot = slot;
            char_owner = owner;
            slot = ItemData.itemList[Itemid].equippos;

            if (Itemid >= 23086 && Itemid <= 23089)
            {
                owner.addSummonSkill();
            }

            owner.addEquipBonus(ItemData.itemList[Itemid].prop1, ItemData.itemList[Itemid].prop1_val, 0);
            owner.addEquipBonus(ItemData.itemList[Itemid].prop2, ItemData.itemList[Itemid].prop2_val, 0);

            owner.refreshBonus();
            owner.reply(new PacketCreator(new byte[] { 0x17, 0x11, old_slot}).send());
            container = null;
        }

        public void unEquipChar(byte inventslot)
        {
            if (container != null) return;
            if (char_owner.inventory.next_item >= 25) return;
            if (char_owner.inventory.next_item + 1 != inventslot)
            {
                Console.WriteLine("WARINING : Inventory data out of sync");
                return;
            }
            container = char_owner.inventory;
            char_owner.equipment[slot - 1] = null;
            char_owner.nb_equips--;
            slot = (byte)(container.next_item + 1);
            container.items[slot - 1] = this;
            container.nextSlot();
            if (Itemid >= 23086 && Itemid <= 23089)
            {
                char_owner.removeSummonSkill();
            }

            char_owner.addEquipBonus(ItemData.itemList[Itemid].prop1, ItemData.itemList[Itemid].prop1_val, 1);
            char_owner.addEquipBonus(ItemData.itemList[Itemid].prop2, ItemData.itemList[Itemid].prop2_val, 1);

            char_owner.refreshBonus();
            char_owner.reply(new PacketCreator(new byte[] { 0x17, 0x10, ItemData.itemList[Itemid].equippos, slot }).send());
            char_owner = null;

        }

        public void equipOnPet(TSPet p)
        {
            int s = ItemData.itemList[Itemid].equippos;
            if (p.level < ItemData.itemList[Itemid].level || s == 0)
                return;
            if (p.equipment[s - 1] != null)
            {
                TSEquipment old_equip = p.equipment[s - 1];
                container.items[slot - 1] = old_equip;
                old_equip.slot = slot;
                old_equip.container = container;
                old_equip.pet_owner = null;
                p.addEquipBonus(ItemData.itemList[old_equip.Itemid].prop1, ItemData.itemList[old_equip.Itemid].prop1_val, 1);
                p.addEquipBonus(ItemData.itemList[old_equip.Itemid].prop2, ItemData.itemList[old_equip.Itemid].prop2_val, 1);
            }
            else
            {
                container.items[slot - 1] = null;
                container.nextSlot();
            }
            p.equipment[s - 1] = this;
            byte old_slot = slot;
            slot = ItemData.itemList[Itemid].equippos;
            pet_owner = p;
            p.addEquipBonus(ItemData.itemList[Itemid].prop1, ItemData.itemList[Itemid].prop1_val, 0);
            p.addEquipBonus(ItemData.itemList[Itemid].prop2, ItemData.itemList[Itemid].prop2_val, 0);

            p.refreshBonus();
            container.owner.reply(new PacketCreator(new byte[] { 0x17, 0x17, p.slot, old_slot }).send());
            container = null;
        }

        public void unEquipPet(byte inventslot)
        {
            if (container != null) return;
            if (pet_owner.owner.inventory.next_item >= 25) return;
            if (pet_owner.owner.inventory.next_item + 1 != inventslot)
            {
                Console.WriteLine("WARINING : Inventory data out of sync");
                return;
            }
            container = pet_owner.owner.inventory;
            pet_owner.equipment[slot - 1] = null;
            slot = (byte)(container.next_item + 1);            
            container.items[slot - 1] = this;
            container.nextSlot();

            pet_owner.addEquipBonus(ItemData.itemList[Itemid].prop1, ItemData.itemList[Itemid].prop1_val, 1);
            pet_owner.addEquipBonus(ItemData.itemList[Itemid].prop2, ItemData.itemList[Itemid].prop2_val, 1);

            pet_owner.refreshBonus();
            container.owner.reply(new PacketCreator(new byte[] { 0x17, 0x16, pet_owner.slot, ItemData.itemList[Itemid].equippos, slot }).send());
            pet_owner = null;
        }

        public void generateEquipBinary(ref byte[] data, ref int pos)
        {
            data[pos] = slot;
            data[pos + 1] = (byte)Itemid; data[pos + 2] = (byte)(Itemid >> 8);
            data[pos + 3] = duration;
            data[pos + 4] = elem_type;
            data[pos + 5] = (byte)elem_val; data[pos + 6] = (byte)(elem_val >> 8);
            pos += 7;            
        }
    }
}
