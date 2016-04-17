using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.Inventory
{
     public class MapleInventoryManipulator
    {
        public static bool addFromDrop(MapleClient c, IMapleItem item, bool show, string owner = null)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            MapleInventoryType type = ii.GetInventoryType(item.ItemId);
            if (!c.ChannelServer.AllowMoreThanOne && ii.IsPickupRestricted(item.ItemId) &&
                c.Character.haveItem(item.ItemId, 1, true, false))
            {
                c.Send(PacketCreator.GetInventoryFull());
                c.Send(PacketCreator.ShowItemUnavailable());
                return false;
            }
            short quantity = item.Quantity;
            if (quantity >= 4000 || quantity < 0)
            {
                AutobanManager.Instance.Autoban(c, $"XSource| PE Item: {quantity} x {item.ItemId}");
                return false;
            }
            if (type != MapleInventoryType.Equip)
            {
                short slotMax = ii.GetSlotMax(c, item.ItemId);
                List<IMapleItem> existing = c.Character.Inventorys[type.Value].ListById(item.ItemId);
                if (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                {
                    if (existing.Any())
                    {
                        for (int i = 0; i < existing.Count && quantity > 0; i++)
                        {
                            Item eItem = (Item)existing[i];
                            short oldQ = eItem.Quantity;
                            if (oldQ < slotMax && item.Owner == eItem.Owner)
                            {
                                short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                quantity -= (short)(newQ - oldQ);
                                eItem.Quantity = newQ;
                                c.Send(PacketCreator.UpdateInventorySlot(type, eItem, true));
                            }
                        }
                    }
                    // add new slots if there is still something left
                    while (quantity > 0 || ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId))
                    {
                        short newQ = Math.Min(quantity, slotMax);
                        quantity -= newQ;
                        Item nItem = new Item(item.ItemId, 0, newQ) { Owner = item.Owner };
                        byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                        if (newSlot == 128)
                        {
                            c.Send(PacketCreator.GetInventoryFull());
                            c.Send(PacketCreator.GetShowInventoryFull());
                            item.Quantity = (short)(quantity + newQ);
                            return false;
                        }
                        c.Send(PacketCreator.AddInventorySlot(type, nItem, true));
                    }
                }
                else
                {
                    // Throwing Stars and Bullets - Add all into one slot regardless of quantity.
                    Item nItem = new Item(item.ItemId, 0, quantity);
                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, nItem));
                    c.Send(PacketCreator.EnableActions());
                }
            }
            else
            {
                if (quantity == 1)
                {
                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(item);

                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, item, true));
                }
                else
                {
                    throw new Exception("Trying to create equip with non-one quantity");
                }
            }
            if (owner != null)
            {
                item.Owner = owner;
            }
            if (show)
            {
                c.Send(PacketCreator.GetShowItemGain(item.ItemId, item.Quantity));
            }
            return true;
        }

        public static bool addFromDrop(MapleClient c, Equip item, string logInfo)
        {
            return addFromDrop(c, item.Copy(), logInfo);
        }

        public static bool addFromDrop(MapleClient c, IMapleItem item, string logInfo = "", bool show = true)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            MapleInventoryType type = ii.GetInventoryType(item.ItemId);

            if (!c.ChannelServer.AllowMoreThanOne && ii.IsPickupRestricted(item.ItemId) && c.Character.haveItem(item.ItemId, 1, true, true))
            {
                c.Send(PacketCreator.GetInventoryFull());
                c.Send(PacketCreator.ShowItemUnavailable());
                return false;
            }

            short quantity = item.Quantity;
            if (type != MapleInventoryType.Equip)
            {
                short slotMax = ii.GetSlotMax(c, item.ItemId);
                List<IMapleItem> existing = c.Character.Inventorys[type.Value].ListById(item.ItemId);
                if (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                {
                    if (existing.Any())
                    { // first update all existing slots to slotMax

                        for (int i = 0; i < existing.Count && quantity > 0; i++)
                        {
                            Item eItem = (Item)existing[i];
                            short oldQ = eItem.Quantity;
                            if (oldQ < slotMax && item.Owner == eItem.Owner)
                            {
                                short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                quantity -= (short)(newQ - oldQ);
                                eItem.Quantity = newQ;
                                //eItem.log("Added " + (newQ - oldQ) + " items to stack, new quantity is " + newQ + " (" + logInfo + " )", false);
                                c.Send(PacketCreator.UpdateInventorySlot(type, eItem, true));
                            }
                        }
                    }
                    // add new slots if there is still something left
                    while (quantity > 0)
                    {
                        short newQ = Math.Min(quantity, slotMax);
                        quantity -= newQ;
                        Item nItem = new Item(item.ItemId, 0, newQ);
                        nItem.Owner = (item.Owner);
                        //nItem.log("Created while adding from drop. Quantity: " + newQ + " (" + logInfo + " )", false);
                        byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                        if (newSlot == 128)
                        {
                            c.Send(PacketCreator.GetInventoryFull());
                            c.Send(PacketCreator.GetShowInventoryFull());
                            item.Quantity = (short)(quantity + newQ);
                            return false;
                        }
                        c.Send(PacketCreator.AddInventorySlot(type, nItem, true));
                    }
                }
                else {
                    // Throwing Stars and Bullets - Add all into one slot regardless of quantity.
                    Item nItem = new Item(item.ItemId, 0, quantity);
                    //nItem.log("Created while adding by id. Quantity: " + quantity + " (" + logInfo + " )", false);
                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, nItem));
                    c.Send(PacketCreator.EnableActions());
                }
            }
            else {
                if (quantity == 1)
                {
                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(item);
                    //item.log("Adding from drop. (" + logInfo + " )", false);

                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, item, true));
                }
                else {
                    throw new Exception("Trying to create equip with non-one quantity");
                }
            }
            if (show)
            {
                c.Send(PacketCreator.GetShowItemGain(item.ItemId, item.Quantity));
            }
            return true;
        }

        public static bool checkSpace(MapleClient c, int itemid, int quantity, string owner)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            MapleInventoryType type = ii.GetInventoryType(itemid);

            if (type != MapleInventoryType.Equip)
            {
                short slotMax = ii.GetSlotMax(c, itemid);
                var existing = c.Character.Inventorys[type.Value].ListById(itemid);
                if (!ii.IsThrowingStar(itemid) && !ii.IsBullet(itemid))
                {
                    if (existing.Any())
                    { // first update all existing slots to slotMax
                        foreach (var eItem in existing)
                        {
                            short oldQ = eItem.Quantity;
                            if (oldQ < slotMax && owner == eItem.Owner)
                            {
                                short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                quantity -= newQ - oldQ;
                            }
                            if (quantity <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
                int numSlotsNeeded;
                if (slotMax > 0)
                { // add new slots if there is still something left
                    numSlotsNeeded = (int)Math.Ceiling((double)quantity / slotMax);
                }
                else if (ii.IsThrowingStar(itemid) || ii.IsBullet(itemid))
                {
                    numSlotsNeeded = 1;
                }
                else {
                    numSlotsNeeded = 1;
                    Console.WriteLine("SUCK ERROR - FIX ME! - 0 slotMax");
                }
                return !c.Character.Inventorys[type.Value].IsFull(numSlotsNeeded - 1);
            }
            return !c.Character.Inventorys[type.Value].IsFull();
        }

        //public static void removeAllById(MapleClient c, int itemId, bool checkEquipped)
        //{
        //    MapleInventoryType type = MapleItemInformationProvider.getInstance().getInventoryType(itemId);
        //    for (IMapleItem item : c.getPlayer().getInventory(type).listById(itemId))
        //    {
        //        if (item != null)
        //        {
        //            removeFromSlot(c, type, item.getPosition(), item.getQuantity(), true, false);
        //        }
        //    }
        //    if (checkEquipped)
        //    {
        //        IMapleItem ii = c.getPlayer().getInventory(type).findById(itemId);
        //        if (ii != null)
        //        {
        //            c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).removeItem(ii.getPosition());
        //            c.getPlayer().equipChanged();
        //        }
        //    }
        //}

        public static bool addById(MapleClient c, int itemId, short quantity, string logInfo, string owner = null, int petid = -1)
        {
            if (quantity < 0)
            {
                return false;
            }
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            MapleInventoryType type = ii.GetInventoryType(itemId);
            if (type!=MapleInventoryType.Equip)
            {
                short slotMax = ii.GetSlotMax(c, itemId);
                List<IMapleItem> existing = c.Character.Inventorys[type.Value].ListById(itemId);
                if (!ii.IsThrowingStar(itemId) && !ii.IsBullet(itemId))
                {
                    if (existing.Any())
                    { // first update all existing slots to slotMax

                        for (int i = 0; i < existing.Count && quantity > 0; i++)
                        {
                            Item eItem = (Item) existing[i];
                            short oldQ = eItem.Quantity;
                                if (oldQ < slotMax && (eItem.Owner==owner || owner == null))
                                {
                                    short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                    quantity -= (short)(newQ - oldQ);
                                    eItem.Quantity= newQ;
                                    //eItem.log("Added " + (newQ - oldQ) + " items to stack, new quantity is " + newQ + " (" + logInfo + " )", false);
                                    c.Send(PacketCreator.UpdateInventorySlot(type, eItem));
                                }
                        }
                    }
                    while (quantity > 0)
                    { // add new slots if there is still something left
                        short newQ = (short)Math.Min(quantity, slotMax);
                        if (newQ != 0)
                        {
                            quantity -= newQ;
                            Item nItem = new Item(itemId, 0, newQ);//, petid);
                            //nItem.log("Created while adding by id. Quantity: " + newQ + " (" + logInfo + ")", false);
                            byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                            if (newSlot == 128)
                            {
                                c.Send(PacketCreator.GetInventoryFull());
                                c.Send(PacketCreator.GetShowInventoryFull());
                                return false;
                            }
                            if (owner != null)
                            {
                                nItem.Owner = owner;
                            }
                            c.Send(PacketCreator.AddInventorySlot(type, nItem));
                            if ((ii.IsThrowingStar(itemId) || ii.IsBullet(itemId)) && quantity == 0)
                            {
                                break;
                            }
                        }
                        else {
                            c.Send(PacketCreator.EnableActions());
                            return false;
                        }
                    }
                }
                else { // Throwing Stars and Bullets - Add all into one slot regardless of quantity.
                    Item nItem = new Item(itemId, 0, quantity);
                    //nItem.log("Created while adding by id. Quantity: " + quantity + " (" + logInfo + " )", false);
                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(nItem);
                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, nItem));
                    c.Send(PacketCreator.EnableActions());
                }
            }
            else {
                if (quantity == 1)
                {
                    IMapleItem nEquip = ii.GetEquipById(itemId);
                    //nEquip.log("Created while adding by id. (" + logInfo + " )", false);
                    if (owner != null)
                    {
                        nEquip.Owner = owner;
                    }

                    byte newSlot = c.Character.Inventorys[type.Value].AddItem(nEquip);
                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, nEquip));
                }
                else {
                    throw new Exception("Trying to create equip with non-one quantity");
                }
            }
            return true;
        }

        public static void removeFromSlot(MapleClient c, MapleInventoryType type, byte slot, short quantity, bool fromDrop, bool consume = false)
        {
            if (quantity < 0)
            {
                return;
            }
            IMapleItem item = c.Character.Inventorys[type.Value].Inventory[slot];
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            bool allowZero = consume && (ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId));
            c.Character.Inventorys[type.Value].RemoveItem(slot, quantity, allowZero);
            if (item.Quantity == 0 && !allowZero)
            {
                c.Send(PacketCreator.ClearInventoryItem(type, item.Position, fromDrop));
            }
            else {
                if (!consume)
                {
                    //item.l(c.getPlayer().getName() + " removed " + quantity + ". " + item.getQuantity() + " left.", false);
                }
                c.Send(PacketCreator.UpdateInventorySlot(type, (Item)item, fromDrop));
            }
        }

        public static void removeById(MapleClient c, MapleInventoryType type, int itemId, int quantity, bool fromDrop, bool consume, bool v)
        {
            var items = c.Character.Inventorys[type.Value].ListById(itemId);
            int remremove = quantity;
            foreach (IMapleItem item in items)
            {
                if (remremove <= item.Quantity)
                {
                    removeFromSlot(c, type, item.Position, (short)remremove, fromDrop, consume);
                    remremove = 0;
                    break;
                }
                else {
                    remremove -= item.Quantity;
                    removeFromSlot(c, type, item.Position, item.Quantity, fromDrop, consume);
                }
            }
            if (remremove > 0)
            {
                throw new Exception("[h4x] Not enough items available (" + itemId + ", " + (quantity - remremove) + "/" + quantity + ")");
            }
        }

        public static void removeById(MapleClient c, MapleInventoryType type, int itemId, int quantity, bool fromDrop, bool consume)
        {
            if (quantity < 0)
            {
                return;
            }
            var items = c.Character.Inventorys[type.Value].ListById(itemId);
            int remremove = quantity;
            foreach (IMapleItem item in items)
            {
                if (remremove <= item.Quantity)
                {
                    removeFromSlot(c, type, item.Position, (short)remremove, fromDrop, consume);
                    remremove = 0;
                    break;
                }
                else {
                    remremove -= item.Quantity;
                    removeFromSlot(c, type, item.Position, item.Quantity, fromDrop, consume);
                }
            }
            if (remremove > 0)
            {
                throw new Exception("Not enough cheese available ( ItemID:" + itemId + ", Remove Amount:" + (quantity - remremove) + "| Current Amount:" + quantity + ")");
            }
        }

        //public static void move(MapleClient c, MapleInventoryType type, byte src, byte dst)
        //{
        //    if (src < 0 || dst < 0 || src > c.getPlayer().getInventory(type).getSlots() || dst > c.getPlayer().getInventory(type).getSlots())
        //    {
        //        return;
        //    }
        //    MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();
        //    IMapleItem source = c.getPlayer().getInventory(type).getItem(src);
        //    IMapleItem initialTarget = c.getPlayer().getInventory(type).getItem(dst);
        //    if (source == null)
        //    {
        //        return;
        //    }
        //    short olddstQ = -1;
        //    if (initialTarget != null)
        //    {
        //        olddstQ = initialTarget.getQuantity();
        //    }
        //    short oldsrcQ = source.getQuantity();
        //    short slotMax = ii.getSlotMax(c, source.getItemId());
        //    bool op = c.getPlayer().getInventory(type).move(src, dst, slotMax);
        //    if (!op)
        //    {
        //        c.getSession().write(MaplePacketCreator.enableActions());
        //        return;
        //    }
        //    if (!type.equals(MapleInventoryType.EQUIP) && initialTarget != null &&
        //            initialTarget.getItemId() == source.getItemId() && !ii.isThrowingStar(source.getItemId()) &&
        //            !ii.isBullet(source.getItemId()))
        //    {
        //        if ((olddstQ + oldsrcQ) > slotMax)
        //        {
        //            c.getSession().write(MaplePacketCreator.moveAndMergeWithRestInventoryItem(type, src, dst, (short)((olddstQ + oldsrcQ) - slotMax), slotMax));
        //        }
        //        else {
        //            c.getSession().write(MaplePacketCreator.moveAndMergeInventoryItem(type, src, dst, ((Item)c.getPlayer().getInventory(type).getItem(dst)).getQuantity()));
        //        }
        //    }
        //    else {
        //        c.getSession().write(MaplePacketCreator.moveInventoryItem(type, src, dst));
        //    }
        //}

        //public static void equip(MapleClient c, byte src, byte dst)
        //{
        //    MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();
        //    Equip source = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIP).getItem(src);
        //    Equip target = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem(dst);

        //    if (source == null)
        //    {
        //        return;
        //    }
        //    if (!c.getPlayer().isGM())
        //    {
        //        switch (source.getItemId())
        //        {
        //            case 1002140: // Wizet Invincible Hat
        //            case 1042003: // Wizet Plain Suit
        //            case 1062007: // Wizet Plain Suit Pants
        //            case 1322013: // Wizet Secret Agent Suitcase
        //                removeAllById(c, source.getItemId(), false);
        //                c.getPlayer().dropMessage(1, "无法佩带此物品");
        //                return;
        //        }
        //    }
        //    int reqLevel = ii.getReqLevel(source.getItemId());
        //    int reqStr = ii.getReqStr(source.getItemId());
        //    int reqDex = ii.getReqDex(source.getItemId());
        //    int reqInt = ii.getReqInt(source.getItemId());
        //    int reqLuk = ii.getReqLuk(source.getItemId());
        //    bool cashSlot = false;
        //    if (source.getItemId() == 1812006)
        //    {
        //        removeAllById(c, source.getItemId(), false);
        //        c.getPlayer().dropMessage(1, "物品已被封印");
        //        return;
        //    }
        //    if (dst < -99)
        //    {
        //        cashSlot = true;
        //    }
        //    if (!ii.isCash(source.getItemId()))
        //    {
        //        String type = ii.getType(source.getItemId());
        //        if ((type.equalsIgnoreCase("Cp") && dst != -1) ||
        //                (type.equalsIgnoreCase("Af") && dst != -2) ||
        //                (type.equalsIgnoreCase("Ay") && dst != -3) ||
        //                (type.equalsIgnoreCase("Ae") && dst != -4) ||
        //                ((type.equalsIgnoreCase("Ma") || type.equalsIgnoreCase("MaPn")) && dst != -5) ||
        //                (type.equalsIgnoreCase("Pn") && dst != -6) ||
        //                (type.equalsIgnoreCase("So") && dst != -7) ||
        //                (type.equalsIgnoreCase("Gv") && dst != -8) ||
        //                (type.equalsIgnoreCase("Sr") && dst != -9) ||
        //                (type.equalsIgnoreCase("Si") && dst != -10) ||
        //                ((type.equalsIgnoreCase("Wp") || type.equalsIgnoreCase("WpSi")) && dst != -11) ||
        //                (type.equalsIgnoreCase("Pe") && dst != -17))
        //        {
        //            c.getSession().write(MaplePacketCreator.enableActions());
        //            return;
        //        }
        //    }
        //    if ((ii.getName(source.getItemId()).contains("(Male)") && c.getPlayer().getGender() != 0) ||
        //            (ii.getName(source.getItemId()).contains("(Female)") && c.getPlayer().getGender() != 1) ||
        //            reqLevel > c.getPlayer().getLevel() ||
        //            reqStr > c.getPlayer().getTotalStr() ||
        //            reqDex > c.getPlayer().getTotalDex() ||
        //            reqInt > c.getPlayer().getTotalInt() ||
        //            reqLuk > c.getPlayer().getTotalLuk() ||
        //            (cashSlot && !ii.isCash(source.getItemId())))
        //    {
        //        c.getSession().write(MaplePacketCreator.enableActions());
        //        return;
        //    }

        //    if (dst == -6)
        //    { // unequip the overall
        //        IMapleItem top = c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-5);
        //        if (top != null && ii.isOverall(top.getItemId()))
        //        {
        //            if (c.getPlayer().getInventory(MapleInventoryType.EQUIP).isFull())
        //            {
        //                c.getSession().write(MaplePacketCreator.getInventoryFull());
        //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
        //                return;
        //            }
        //            unequip(c, (byte)-5, c.getPlayer().getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
        //        }
        //    }
        //    else if (dst == -5)
        //    { // unequip the bottom and top
        //        IMapleItem top = c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-5);
        //        IMapleItem bottom = c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-6);
        //        if (top != null && ii.isOverall(source.getItemId()))
        //        {
        //            if (c.getPlayer().getInventory(MapleInventoryType.EQUIP).isFull(bottom != null && ii.isOverall(source.getItemId()) ? 1 : 0))
        //            {
        //                c.getSession().write(MaplePacketCreator.getInventoryFull());
        //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
        //                return;
        //            }
        //            unequip(c, (byte)-5, c.getPlayer().getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
        //        }
        //        if (bottom != null && ii.isOverall(source.getItemId()))
        //        {
        //            if (c.getPlayer().getInventory(MapleInventoryType.EQUIP).isFull())
        //            {
        //                c.getSession().write(MaplePacketCreator.getInventoryFull());
        //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
        //                return;
        //            }
        //            unequip(c, (byte)-6, c.getPlayer().getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
        //        }
        //    }
        //    else if (dst == -10)
        //    { // check if weapon is two-handed
        //        IMapleItem weapon = c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-11);
        //        if (weapon != null && ii.isTwoHanded(weapon.getItemId()))
        //        {
        //            if (c.getPlayer().getInventory(MapleInventoryType.EQUIP).isFull())
        //            {
        //                c.getSession().write(MaplePacketCreator.getInventoryFull());
        //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
        //                return;
        //            }
        //            unequip(c, (byte)-11, c.getPlayer().getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
        //        }
        //    }
        //    else if (dst == -11)
        //    {
        //        IMapleItem shield = c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-10);
        //        if (shield != null && ii.isTwoHanded(source.getItemId()))
        //        {
        //            if (c.getPlayer().getInventory(MapleInventoryType.EQUIP).isFull())
        //            {
        //                c.getSession().write(MaplePacketCreator.getInventoryFull());
        //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
        //                return;
        //            }
        //            unequip(c, (byte)-10, c.getPlayer().getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
        //        }
        //    }
        //    else if (dst == -18)
        //    {
        //        if (c.getPlayer().getMount() != null)
        //        {
        //            c.getPlayer().getMount().setItemId(source.getItemId());
        //        }
        //    }
        //    source = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIP).getItem(src);
        //    target = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem(dst);
        //    c.getPlayer().getInventory(MapleInventoryType.EQUIP).removeSlot(src);
        //    if (target != null)
        //    {
        //        c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).removeSlot(dst);
        //    }
        //    source.setPosition(dst);
        //    c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).addFromDB(source);
        //    if (target != null)
        //    {
        //        target.setPosition(src);
        //        c.getPlayer().getInventory(MapleInventoryType.EQUIP).addFromDB(target);
        //    }
        //    if (c.getPlayer().getBuffedValue(MapleBuffStat.BOOSTER) != null && ii.isWeapon(source.getItemId()))
        //    {
        //        c.getPlayer().cancelBuffStats(MapleBuffStat.BOOSTER);
        //    }
        //    c.getSession().write(MaplePacketCreator.moveInventoryItem(MapleInventoryType.EQUIP, src, dst, (byte)2));
        //    c.getPlayer().equipChanged();
        //    //c.getSession().write(MaplePacketCreator.upChrLook());
        //}

        //public static void unequip(MapleClient c, byte src, byte dst)
        //{
        //    Equip source = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem(src);
        //    Equip target = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIP).getItem(dst);
        //    if (dst < 0)
        //    {
        //        log.warn("Unequipping to negative slot. ({}: {}->{})", new Object[] { c.getPlayer().getName(), src, dst });
        //    }
        //    if (source == null)
        //    {
        //        return;
        //    }
        //    if (target != null && src <= 0)
        //    { // do not allow switching with equip
        //        c.getSession().write(MaplePacketCreator.getInventoryFull());
        //        return;
        //    }
        //    c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).removeSlot(src);
        //    if (target != null)
        //    {
        //        c.getPlayer().getInventory(MapleInventoryType.EQUIP).removeSlot(dst);
        //    }
        //    source.setPosition(dst);
        //    c.getPlayer().getInventory(MapleInventoryType.EQUIP).addFromDB(source);
        //    if (target != null)
        //    {
        //        target.setPosition(src);
        //        c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).addFromDB(target);
        //    }
        //    c.getSession().write(MaplePacketCreator.moveInventoryItem(MapleInventoryType.EQUIP, src, dst, (byte)1));
        //    c.getPlayer().equipChanged();
        //}

        //public static void drop(MapleClient c, MapleInventoryType type, byte src, short quantity)
        //{
        //    MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();

        //    if (src < 0)
        //    {
        //        type = MapleInventoryType.EQUIPPED;
        //    }
        //    IMapleItem source = c.getPlayer().getInventory(type).getItem(src);
        //    if (quantity > ii.getSlotMax(c, source.getItemId()))
        //    {
        //        try
        //        {
        //            c.getChannelServer().getWorldInterface().broadcastGMMessage(c.getPlayer().getName(), MaplePacketCreator.serverNotice(0, c.getPlayer().getName() + " is dropping more than slotMax.").getBytes());
        //        }
        //        catch (Throwable u)
        //        {
        //        }
        //    }
        //    if (quantity < 0 || quantity == 0 && !ii.isThrowingStar(source.getItemId()) && !ii.isBullet(source.getItemId()))
        //    {
        //        String message = "Dropping " + quantity + " " + (source == null ? "?" : source.getItemId()) + " (" +
        //                type.name() + "/" + src + ")";
        //        log.info(MapleClient.getLogMessage(c, message));
        //        c.getSession().close(); // disconnect the client as is inventory is inconsistent with the serverside inventory
        //        return;
        //    }
        //    Point dropPos = new Point(c.getPlayer().getPosition());
        //    if (quantity < source.getQuantity() && !ii.isThrowingStar(source.getItemId()) && !ii.isBullet(source.getItemId()))
        //    {
        //        IMapleItem target = source.copy();
        //        target.setQuantity(quantity);
        //        target.log(c.getPlayer().getName() + " dropped part of a stack at " + dropPos.toString() + " on map " + c.getPlayer().getMapId() + ". Quantity of this (new) instance is now " + quantity, false);
        //        source.setQuantity((short)(source.getQuantity() - quantity));
        //        source.log(c.getPlayer().getName() + " dropped part of a stack at " + dropPos.toString() + " on map " + c.getPlayer().getMapId() + ". Quantity of this (leftover) instance is now " + source.getQuantity(), false);
        //        c.getSession().write(MaplePacketCreator.dropInventoryItemUpdate(type, source));
        //        bool weddingRing = source.getItemId() == 1112804;
        //        bool LiRing = source.getItemId() == 1112405;
        //        if (weddingRing)
        //        {
        //            c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //        }
        //        else if (LiRing)
        //        {
        //            c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //        }
        //        else if (c.getPlayer().getMap().getEverlast())
        //        {
        //            if (!c.getChannelServer().allowUndroppablesDrop() && (ii.isDropRestricted(target.getItemId())))
        //            {
        //                c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //            }
        //            else {
        //                if (LiRing)
        //                {
        //                    c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //                }
        //                else {
        //                    c.getPlayer().getMap().spawnItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos, true, false);
        //                }
        //            }
        //        }
        //        else {
        //            if (!c.getChannelServer().allowUndroppablesDrop() && (ii.isDropRestricted(target.getItemId())))
        //            {
        //                c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //            }
        //            else {
        //                if (LiRing)
        //                {
        //                    c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos);
        //                }
        //                else {
        //                    c.getPlayer().getMap().spawnItemDrop(c.getPlayer(), c.getPlayer(), target, dropPos, true, false);
        //                }
        //            }
        //        }
        //    }
        //    else {
        //        source.log(c.getPlayer().getName() + " dropped this (with full quantity) at " + dropPos.toString() + " on map " + c.getPlayer().getMapId(), false);
        //        c.getPlayer().getInventory(type).removeSlot(src);
        //        c.getSession().write(MaplePacketCreator.dropInventoryItem((src < 0 ? MapleInventoryType.EQUIP : type), src));
        //        bool LiRing = source.getItemId() == 1112405;
        //        if (src < 0)
        //        {
        //            c.getPlayer().equipChanged();
        //        }
        //        if (c.getPlayer().getMap().getEverlast())
        //        {
        //            if (!c.getChannelServer().allowUndroppablesDrop() && ii.isDropRestricted(source.getItemId()))
        //            {
        //                c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos);
        //            }
        //            else {
        //                c.getPlayer().getMap().spawnItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos, true, false);
        //                if (LiRing)
        //                {
        //                    c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos);
        //                }
        //                else {
        //                    c.getPlayer().getMap().spawnItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos, true, true);
        //                }
        //            }
        //        }
        //        else {
        //            if (!c.getChannelServer().allowUndroppablesDrop() && ii.isDropRestricted(source.getItemId()))
        //            {
        //                c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos);
        //            }
        //            else {
        //                if (LiRing)
        //                {
        //                    c.getPlayer().getMap().disappearingItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos);
        //                }
        //                else {
        //                    c.getPlayer().getMap().spawnItemDrop(c.getPlayer(), c.getPlayer(), source, dropPos, true, true);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
