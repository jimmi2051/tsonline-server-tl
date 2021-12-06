using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Server.Client;
using TS_Server.DataTools;

namespace TS_Server.PacketHandlers
{
    class ItemHandler
    {
        public ItemHandler(TSClient client, byte[] data)
        {
            TSCharacter chr = client.getChar();
            switch (data[1])
            {
                case 3: //drop item
                    chr.inventory.dropItem(data[2], data[3]);
                    break;
                case 0x11: //equip item pet
                    chr.inventory.items[data[3] - 1].equip.equipOnPet(chr.pet[data[2] - 1]);
                    if (client.battle != null)
                    {
                        client.battle.DoEquipPet(client);
                    }
                    break;
                case 0x12: //unequip pet
                    chr.pet[data[2] - 1].equipment[data[3] - 1].unEquipPet(data[4]);
                    if (client.battle != null)
                    {
                        client.battle.DoEquipPet(client);
                    }
                    break;
                case 0xa: //move-split item
                    chr.inventory.moveItem(data[2], data[3], data[4]);
                    break;
                case 0x0b: //equip item char
                    chr.inventory.items[data[2] - 1].equip.equipOnChar();
                    if (client.battle != null)
                    {
                        client.battle.DoEquip(client);
                    }
                    break;
                case 0x0c: //unequip char                
                    chr.equipment[data[2] - 1].unEquipChar(data[3]);
                    if (client.battle != null)
                    {
                        client.battle.DoEquip(client);
                    }
                    break;
                case 0x0f: //use item
                    if (data[4] == 0)
                        chr.inventory.items[data[2] - 1].useItemChar(data[3], chr);
                    else
                        chr.inventory.items[data[2] - 1].useItemPet(data[3], chr.pet[data[4] - 1]);
                    break;
                case 2: // Click item on map
                    ushort mapId = client.map.mapid;
                    ushort idItemOnMap = data[2];
                    ushort idItem = EveData.listItemOnMap[mapId].Find(item => item.idItemOnMap == idItemOnMap).idItem;
                    chr.inventory.addItem(idItem, 1, true);
                    break;
                case 0x24:
                    new BagHandle(client, data);
                    break;
                case 0x25:
                    new BagHandle(client, data);
                    break;
                default:
                    Console.WriteLine("Item Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
