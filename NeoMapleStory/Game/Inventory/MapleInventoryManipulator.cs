using System;
using System.Linq;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using NeoMapleStory.Game.Buff;
using System.Drawing;

namespace NeoMapleStory.Game.Inventory
{
    public class MapleInventoryManipulator
    {
        public static bool AddFromDrop(MapleClient c, IMapleItem item, bool show, string loginfo = "")
        {
            var ii = MapleItemInformationProvider.Instance;
            var type = ii.GetInventoryType(item.ItemId);
            if (!c.ChannelServer.AllowMoreThanOne && ii.IsPickupRestricted(item.ItemId) &&
                c.Player.HaveItem(item.ItemId, 1, true, false))
            {
                c.Send(PacketCreator.GetInventoryFull());
                c.Send(PacketCreator.ShowItemUnavailable());
                return false;
            }
            var quantity = item.Quantity;
            if (quantity >= 4000 || quantity < 0)
            {
                AutobanManager.Instance.Autoban(c, $"XSource| PE Item: {quantity} x {item.ItemId}");
                return false;
            }
            if (type != MapleInventoryType.Equip)
            {
                var slotMax = ii.GetSlotMax(c, item.ItemId);
                var existing = c.Player.Inventorys[type.Value].ListById(item.ItemId);
                if (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                {
                    if (existing.Any())
                    {
                        var i = existing.GetEnumerator();
                        while (quantity > 0)
                        {
                            if (i.MoveNext())
                            {
                                var eItem = (Item)i.Current;
                                if (eItem != null)
                                {
                                    var oldQ = eItem.Quantity;
                                    if (oldQ < slotMax && item.Owner == eItem.Owner)
                                    {
                                        var newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                        quantity -= (short)(newQ - oldQ);
                                        eItem.Quantity = newQ;
                                        c.Send(PacketCreator.UpdateInventorySlot(type, eItem, true));
                                    }
                                }
                            }
                            else
                                break;
                        }
                    }
                    // add new slots if there is still something left
                    while (quantity > 0 || ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId))
                    {
                        var newQ = Math.Min(quantity, slotMax);
                        quantity -= newQ;
                        var nItem = new Item(item.ItemId, 0, newQ) { Owner = item.Owner };
                        var newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
                        if (newSlot == 0xFF)
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
                    var nItem = new Item(item.ItemId, 0, quantity);
                    var newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
                    if (newSlot == 0xFF)
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
                    var newSlot = c.Player.Inventorys[type.Value].AddItem(item);

                    if (newSlot == 0xFF)
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
            if (show)
            {
                c.Send(PacketCreator.GetShowItemGain(item.ItemId, item.Quantity));
            }
            return true;
        }

        //public static bool AddFromDrop(MapleClient c, IMapleItem item, string logInfo , bool show)
        //{
        //    MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
        //    MapleInventoryType type = ii.GetInventoryType(item.ItemId);

        //    if (!c.ChannelServer.AllowMoreThanOne && ii.IsPickupRestricted(item.ItemId) && c.Player.HaveItem(item.ItemId, 1, true, true))
        //    {
        //        c.Send(PacketCreator.GetInventoryFull());
        //        c.Send(PacketCreator.ShowItemUnavailable());
        //        return false;
        //    }

        //    short quantity = item.Quantity;
        //    if (type != MapleInventoryType.Equip)
        //    {
        //        short slotMax = ii.GetSlotMax(c, item.ItemId);
        //        List<IMapleItem> existing = c.Player.Inventorys[type.Value].ListById(item.ItemId);
        //        if (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
        //        {
        //            if (existing.Any())
        //            { 
        //                // first update all existing slots to slotMax
        //                var i = existing.GetEnumerator();
        //                while( quantity > 0)
        //                {
        //                    if (i.MoveNext())
        //                    {
        //                        Item eItem = (Item) i.Current;
        //                        if (eItem != null)
        //                        {
        //                            short oldQ = eItem.Quantity;
        //                            if (oldQ < slotMax && item.Owner == eItem.Owner)
        //                            {
        //                                short newQ = (short) Math.Min(oldQ + quantity, slotMax);
        //                                quantity -= (short) (newQ - oldQ);
        //                                eItem.Quantity = newQ;
        //                                //eItem.log("Added " + (newQ - oldQ) + " items to stack, new quantity is " + newQ + " (" + logInfo + " )", false);
        //                                c.Send(PacketCreator.UpdateInventorySlot(type, eItem, true));
        //                            }
        //                        }
        //                    }
        //                    else
        //                        break;
        //                }
        //            }
        //            // add new slots if there is still something left
        //            while (quantity > 0)
        //            {
        //                short newQ = Math.Min(quantity, slotMax);
        //                quantity -= newQ;
        //                Item nItem = new Item(item.ItemId, 0, newQ) {Owner = (item.Owner)};
        //                //nItem.log("Created while adding from drop. Quantity: " + newQ + " (" + logInfo + " )", false);
        //                byte newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
        //                if (newSlot == 0xFF)
        //                {
        //                    c.Send(PacketCreator.GetInventoryFull());
        //                    c.Send(PacketCreator.GetShowInventoryFull());
        //                    item.Quantity = (short)(quantity + newQ);
        //                    return false;
        //                }
        //                c.Send(PacketCreator.AddInventorySlot(type, nItem, true));
        //            }
        //        }
        //        else {
        //            // Throwing Stars and Bullets - Add all into one slot regardless of quantity.
        //            Item nItem = new Item(item.ItemId, 0, quantity);
        //            //nItem.log("Created while adding by id. Quantity: " + quantity + " (" + logInfo + " )", false);
        //            byte newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
        //            if (newSlot == 0xFF)
        //            {
        //                c.Send(PacketCreator.GetInventoryFull());
        //                c.Send(PacketCreator.GetShowInventoryFull());
        //                return false;
        //            }
        //            c.Send(PacketCreator.AddInventorySlot(type, nItem));
        //            c.Send(PacketCreator.EnableActions());
        //        }
        //    }
        //    else {
        //        if (quantity == 1)
        //        {
        //            byte newSlot = c.Player.Inventorys[type.Value].AddItem(item);
        //            //item.log("Adding from drop. (" + logInfo + " )", false);

        //            if (newSlot == 0xFF)
        //            {
        //                c.Send(PacketCreator.GetInventoryFull());
        //                c.Send(PacketCreator.GetShowInventoryFull());
        //                return false;
        //            }
        //            c.Send(PacketCreator.AddInventorySlot(type, item, true));
        //        }
        //        else {
        //            throw new Exception("Trying to create equip with non-one quantity");
        //        }
        //    }
        //    if (show)
        //    {
        //        c.Send(PacketCreator.GetShowItemGain(item.ItemId, item.Quantity));
        //    }
        //    return true;
        //}

        public static bool CheckSpace(MapleClient c, int itemid, int quantity, string owner)
        {
            var ii = MapleItemInformationProvider.Instance;
            var type = ii.GetInventoryType(itemid);

            if (type != MapleInventoryType.Equip)
            {
                var slotMax = ii.GetSlotMax(c, itemid);
                var existing = c.Player.Inventorys[type.Value].ListById(itemid);
                if (!ii.IsThrowingStar(itemid) && !ii.IsBullet(itemid))
                {
                    if (existing.Any())
                    {
                        // first update all existing slots to slotMax
                        foreach (var eItem in existing)
                        {
                            var oldQ = eItem.Quantity;
                            if (oldQ < slotMax && owner == eItem.Owner)
                            {
                                var newQ = (short)Math.Min(oldQ + quantity, slotMax);
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
                {
                    // add new slots if there is still something left
                    numSlotsNeeded = (int)Math.Ceiling((double)quantity / slotMax);
                }
                else if (ii.IsThrowingStar(itemid) || ii.IsBullet(itemid))
                {
                    numSlotsNeeded = 1;
                }
                else
                {
                    numSlotsNeeded = 1;
                    Console.WriteLine("SUCK ERROR - FIX ME! - 0 slotMax");
                }
                return !c.Player.Inventorys[type.Value].IsFull(numSlotsNeeded - 1);
            }
            return !c.Player.Inventorys[type.Value].IsFull();
        }

        public static void RemoveAllById(MapleClient c, int itemId, bool checkEquipped)
        {
            var type = MapleItemInformationProvider.Instance.GetInventoryType(itemId);
            foreach (var item in c.Player.Inventorys[type.Value].ListById(itemId))
            {
                if (item != null)
                {
                    RemoveFromSlot(c, type, item.Position, item.Quantity, true);
                }
            }
            if (checkEquipped)
            {
                var ii = c.Player.Inventorys[type.Value].FindById(itemId);
                if (ii != null)
                {
                    c.Player.Inventorys[MapleInventoryType.Equipped.Value].RemoveItem(ii.Position);
                    //c.Character.equipChanged();
                }
            }
        }

        public static bool AddById(MapleClient c, int itemId, short quantity, string logInfo, string owner = null, int petid = -1)
        {
            if (quantity < 0)
            {
                return false;
            }
            var ii = MapleItemInformationProvider.Instance;
            var type = ii.GetInventoryType(itemId);
            if (type != MapleInventoryType.Equip)
            {
                var slotMax = ii.GetSlotMax(c, itemId);
                var existing = c.Player.Inventorys[type.Value].ListById(itemId);
                if (!ii.IsThrowingStar(itemId) && !ii.IsBullet(itemId))
                {
                    if (existing.Any())
                    {
                        // first update all existing slots to slotMax

                        for (var i = 0; i < existing.Count && quantity > 0; i++)
                        {
                            var eItem = (Item)existing[i];
                            var oldQ = eItem.Quantity;
                            if (oldQ < slotMax && (eItem.Owner == owner || owner == null))
                            {
                                var newQ = (short)Math.Min(oldQ + quantity, slotMax);
                                quantity -= (short)(newQ - oldQ);
                                eItem.Quantity = newQ;
                                //eItem.log("Added " + (newQ - oldQ) + " items to stack, new quantity is " + newQ + " (" + logInfo + " )", false);
                                c.Send(PacketCreator.UpdateInventorySlot(type, eItem));
                            }
                        }
                    }
                    while (quantity > 0)
                    {
                        // add new slots if there is still something left
                        var newQ = Math.Min(quantity, slotMax);
                        if (newQ != 0)
                        {
                            quantity -= newQ;
                            var nItem = new Item(itemId, 0, newQ); //, petid);
                            //nItem.log("Created while adding by id. Quantity: " + newQ + " (" + logInfo + ")", false);
                            var newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
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
                        else
                        {
                            c.Send(PacketCreator.EnableActions());
                            return false;
                        }
                    }
                }
                else
                {
                    // Throwing Stars and Bullets - Add all into one slot regardless of quantity.
                    var nItem = new Item(itemId, 0, quantity);
                    //nItem.log("Created while adding by id. Quantity: " + quantity + " (" + logInfo + " )", false);
                    var newSlot = c.Player.Inventorys[type.Value].AddItem(nItem);
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
                    var nEquip = ii.GetEquipById(itemId);
                    //nEquip.log("Created while adding by id. (" + logInfo + " )", false);
                    if (owner != null)
                    {
                        nEquip.Owner = owner;
                    }

                    var newSlot = c.Player.Inventorys[type.Value].AddItem(nEquip);
                    if (newSlot == 128)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                        return false;
                    }
                    c.Send(PacketCreator.AddInventorySlot(type, nEquip));
                }
                else
                {
                    throw new Exception("Trying to create equip with non-one quantity");
                }
            }
            return true;
        }

        public static void RemoveFromSlot(MapleClient c, MapleInventoryType type, byte slot, short quantity, bool fromDrop, bool consume = false)
        {
            if (quantity < 0)
            {
                return;
            }
            var item = c.Player.Inventorys[type.Value].Inventory[slot];
            var ii = MapleItemInformationProvider.Instance;
            var allowZero = consume && (ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId));
            c.Player.Inventorys[type.Value].RemoveItem(slot, quantity, allowZero);
            if (item.Quantity == 0 && !allowZero)
            {
                c.Send(PacketCreator.ClearInventoryItem(type, item.Position, fromDrop));
            }
            else
            {
                if (!consume)
                {
                    //item.l(c.Player.getName() + " removed " + quantity + ". " + item.getQuantity() + " left.", false);
                }
                c.Send(PacketCreator.UpdateInventorySlot(type, (Item)item, fromDrop));
            }
        }

        public static void RemoveById(MapleClient c, MapleInventoryType type, int itemId, int quantity, bool fromDrop, bool consume, bool v)
        {
            var items = c.Player.Inventorys[type.Value].ListById(itemId);
            var remremove = quantity;
            foreach (var item in items)
            {
                if (remremove <= item.Quantity)
                {
                    RemoveFromSlot(c, type, item.Position, (short)remremove, fromDrop, consume);
                    remremove = 0;
                    break;
                }
                remremove -= item.Quantity;
                RemoveFromSlot(c, type, item.Position, item.Quantity, fromDrop, consume);
            }
            if (remremove > 0)
            {
                throw new Exception("[h4x] Not enough items available (" + itemId + ", " + (quantity - remremove) + "/" +
                                    quantity + ")");
            }
        }

        public static void RemoveById(MapleClient c, MapleInventoryType type, int itemId, int quantity, bool fromDrop, bool consume)
        {
            if (quantity < 0)
            {
                return;
            }
            var items = c.Player.Inventorys[type.Value].ListById(itemId);
            var remremove = quantity;
            foreach (var item in items)
            {
                if (remremove <= item.Quantity)
                {
                    RemoveFromSlot(c, type, item.Position, (short)remremove, fromDrop, consume);
                    remremove = 0;
                    break;
                }
                remremove -= item.Quantity;
                RemoveFromSlot(c, type, item.Position, item.Quantity, fromDrop, consume);
            }
            if (remremove > 0)
            {
                throw new Exception("Not enough cheese available ( ItemID:" + itemId + ", Remove Amount:" + (quantity - remremove) + "| Current Amount:" + quantity + ")");
            }
        }

        public static void Move(MapleClient c, MapleInventoryType type, short src, short dst)
        {
            byte srcSlot = (byte)src;
            byte dstSlot = (byte)dst;

            if (srcSlot > 127 || dstSlot > 127 || srcSlot > c.Player.Inventorys[type.Value].SlotLimit || dstSlot > c.Player.Inventorys[type.Value].SlotLimit)
            {
                return;
            }
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            IMapleItem source;
            IMapleItem initialTarget;
            if (!c.Player.Inventorys[type.Value].Inventory.TryGetValue(srcSlot, out source))
            {
                return;
            }
            short olddstQ = -1;
            if (c.Player.Inventorys[type.Value].Inventory.TryGetValue(dstSlot, out initialTarget))
            {
                olddstQ = initialTarget.Quantity;
            }
            short oldsrcQ = source.Quantity;
            short slotMax = ii.GetSlotMax(c, source.ItemId);
            bool op = c.Player.Inventorys[type.Value].Move(srcSlot, dstSlot, slotMax);
            if (!op)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            if (type != MapleInventoryType.Equip && initialTarget != null && initialTarget.ItemId == source.ItemId && !ii.IsThrowingStar(source.ItemId) && !ii.IsBullet(source.ItemId))
            {
                c.Send(olddstQ + oldsrcQ > slotMax
                    ? PacketCreator.MoveAndMergeWithRestInventoryItem(type, srcSlot, dstSlot,
                        (short) ((olddstQ + oldsrcQ) - slotMax), slotMax)
                    : PacketCreator.MoveAndMergeInventoryItem(type, srcSlot, dstSlot,
                        ((Item) c.Player.Inventorys[type.Value].Inventory[dstSlot]).Quantity));
            }
            else
            {
                c.Send(PacketCreator.MoveInventoryItem(type, src, dst, 0));
            }
        }

        public static void Equip(MapleClient c, short src, short dst)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;

            byte srcSlot = (byte)src;
            byte dstSlot = (byte)dst;

            Equip source = c.Player.Inventorys[MapleInventoryType.Equip.Value].Inventory.FirstOrDefault(x=>x.Key==srcSlot).Value as Equip;
            Equip target = c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.FirstOrDefault(x => x.Key == dstSlot).Value as Equip;

            if (source==null)
            {
                return;
            }
            if (!c.Player.IsGm)
            {
                switch (source.ItemId)
                {
                    case 1002140: // Wizet Invincible Hat
                    case 1042003: // Wizet Plain Suit
                    case 1062007: // Wizet Plain Suit Pants
                    case 1322013: // Wizet Secret Agent Suitcase
                        RemoveAllById(c, source.ItemId, false);
                        c.Player.DropMessage(PacketCreator.ServerMessageType.Popup, "无法佩带此物品");
                        return;
                }
            }
            int reqLevel = ii.GetReqLevel(source.ItemId);
            int reqStr = ii.GetReqStr(source.ItemId);
            int reqDex = ii.GetReqDex(source.ItemId);
            int reqInt = ii.GetReqInt(source.ItemId);
            int reqLuk = ii.GetReqLuk(source.ItemId);
            bool cashSlot = false;
            if (source.ItemId == 1812006)
            {
                RemoveAllById(c, source.ItemId, false);
                c.Player.DropMessage(PacketCreator.ServerMessageType.Popup, "物品已被封印");
                return;
            }
            if (dstSlot < 0x9D)
            {
                cashSlot = true;
            }
            if (!ii.IsCash(source.ItemId))
            {
                string type = ii.GetType(source.ItemId);
                if ((type.Equals("Cp", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xFF) ||
                        (type.Equals("Af", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xFE) ||
                        (type.Equals("Ay", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xFD) ||
                        (type.Equals("Ae", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xFC) ||
                        ((type.Equals("Ma", StringComparison.CurrentCultureIgnoreCase) || type.Equals("MaPn", StringComparison.CurrentCultureIgnoreCase)) && dstSlot != 0xFB) ||
                        (type.Equals("Pn", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xFA) ||
                        (type.Equals("So", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xF9) ||
                        (type.Equals("Gv", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xF8) ||
                        (type.Equals("Sr", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xF7) ||
                        (type.Equals("Si", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xF6) ||
                        ((type.Equals("Wp", StringComparison.CurrentCultureIgnoreCase) || type.Equals("WpSi", StringComparison.CurrentCultureIgnoreCase)) && dstSlot != 0xF5) ||
                        (type.Equals("Pe", StringComparison.CurrentCultureIgnoreCase) && dstSlot != 0xEF))
                {
                    c.Send(PacketCreator.EnableActions());
                    return;
                }
            }
            if ((ii.GetName(source.ItemId).Contains("(Male)") && !c.Player.Gender) ||
                    (ii.GetName(source.ItemId).Contains("(Female)") && c.Player.Gender) ||
                    reqLevel > c.Player.Level ||
                    reqStr > c.Player.Localstr ||
                    reqDex > c.Player.Localdex ||
                    reqInt > c.Player.Localint ||
                    reqLuk > c.Player.Localluk ||
                    (cashSlot && !ii.IsCash(source.ItemId)))
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }

            switch (dstSlot)
            {
                case 0xFA:
                    {
                        // unequip the overall
                        IMapleItem top;
                        if (c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xFB, out top) && ii.IsOverall(top.ItemId))
                        {
                            if (c.Player.Inventorys[MapleInventoryType.Equip.Value].IsFull())
                            {
                                c.Send(PacketCreator.GetInventoryFull());
                                c.Send(PacketCreator.GetShowInventoryFull());
                                return;
                            }
                            UnEquip(c, -5, c.Player.Inventorys[MapleInventoryType.Equip.Value].GetNextFreeSlot());
                        }
                    }
                    break;
                case 0xFB:
                    {// unequip the bottom and top
                        IMapleItem top = c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.FirstOrDefault(x => x.Key == 0xFB).Value;
                        IMapleItem bottom = c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.FirstOrDefault(x => x.Key == 0xFA).Value;
                        if (top != null && ii.IsOverall(source.ItemId))
                        {
                            if (c.Player.Inventorys[MapleInventoryType.Equip.Value].IsFull(bottom != null && ii.IsOverall(source.ItemId) ? 1 : 0))
                            {
                                c.Send(PacketCreator.GetInventoryFull());
                                c.Send(PacketCreator.GetShowInventoryFull());
                                return;
                            }
                            UnEquip(c, -5, c.Player.Inventorys[MapleInventoryType.Equip.Value].GetNextFreeSlot());
                        }
                        if (bottom != null && ii.IsOverall(source.ItemId))
                        {
                            if (c.Player.Inventorys[MapleInventoryType.Equip.Value].IsFull())
                            {
                                c.Send(PacketCreator.GetInventoryFull());
                                c.Send(PacketCreator.GetShowInventoryFull());
                                return;
                            }
                            UnEquip(c, -6, c.Player.Inventorys[MapleInventoryType.Equip.Value].GetNextFreeSlot());
                        }
                    }
                    break;
                case 0xF6:
                    // check if weapon is two-handed
                    IMapleItem weapon;
                    if ( c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xF5,out weapon) && ii.IsTwoHanded(weapon.ItemId))
                    {
                        if (c.Player.Inventorys[MapleInventoryType.Equip.Value].IsFull())
                        {
                            c.Send(PacketCreator.GetInventoryFull());
                            c.Send(PacketCreator.GetShowInventoryFull());
                            return;
                        }
                        UnEquip(c, -11, c.Player.Inventorys[MapleInventoryType.Equip.Value].GetNextFreeSlot());
                    }
                    break;
                case 0xF5:
                    IMapleItem shield;
                    if (c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xF6, out shield) && ii.IsTwoHanded(source.ItemId))
                    {
                        if (c.Player.Inventorys[MapleInventoryType.Equip.Value].IsFull())
                        {
                            c.Send(PacketCreator.GetInventoryFull());
                            c.Send(PacketCreator.GetShowInventoryFull());
                            return;
                        }
                        UnEquip(c, -10, c.Player.Inventorys[MapleInventoryType.Equip.Value].GetNextFreeSlot());
                    }
                    break;
                case 0xEE:
                    //if (c.Player.Mount != null)
                    //{
                    //    c.Player.getMount().setItemId(source.ItemId);
                    //}
                    break;
            }

            source = c.Player.Inventorys[MapleInventoryType.Equip.Value].Inventory.FirstOrDefault(x => x.Key == srcSlot).Value as Equip;
            target = c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.FirstOrDefault(x => x.Key == dstSlot).Value as Equip;

            c.Player.Inventorys[MapleInventoryType.Equip.Value].RemoveSlot(srcSlot);

            if (target!=null)
            {
                c.Player.Inventorys[MapleInventoryType.Equipped.Value].RemoveSlot(dstSlot);
            }

            source.Position = dstSlot;

            c.Player.Inventorys[MapleInventoryType.Equipped.Value].AddFromDb(source);

            if (target!=null)
            {
                target.Position = srcSlot;
                c.Player.Inventorys[MapleInventoryType.Equip.Value].AddFromDb(target);
            }

            if (c.Player.GetBuffedValue(MapleBuffStat.Booster) != null && ii.IsWeapon(source.ItemId))
            {
                c.Player.CancelBuffStats(MapleBuffStat.Booster);
            }

            c.Send(PacketCreator.MoveInventoryItem(MapleInventoryType.Equip,src,dst, 2));
            c.Player.EquipChanged();
        }

        public static void UnEquip(MapleClient c,short src, short dst)
        {
            byte srcSlot = (byte)src;
            byte dstSlot = (byte)dst;

            Equip source = c.Player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.FirstOrDefault(x => x.Key == srcSlot).Value as Equip;
            Equip target = c.Player.Inventorys[MapleInventoryType.Equip.Value].Inventory.FirstOrDefault(x => x.Key == dstSlot).Value as Equip;

            if (dstSlot > 127)
            {
                Console.WriteLine("Unequipping to negative slot. ({0}: {1}->{2})", c.Player.Name, srcSlot, dstSlot);
            }
            if (source == null)
            {
                return;
            }
            if (target != null && (srcSlot > 127 || srcSlot == 0))
            {
                // do not allow switching with equip
                c.Send(PacketCreator.GetInventoryFull());
                return;
            }

            c.Player.Inventorys[MapleInventoryType.Equipped.Value].RemoveSlot(srcSlot);
            if (target != null)
            {
                c.Player.Inventorys[MapleInventoryType.Equip.Value].RemoveSlot(dstSlot);
            }
            source.Position = dstSlot;
            c.Player.Inventorys[MapleInventoryType.Equip.Value].AddFromDb(source);
            if (target != null)
            {
                target.Position = srcSlot;
                c.Player.Inventorys[MapleInventoryType.Equipped.Value].AddFromDb(target);
            }
            c.Send(PacketCreator.MoveInventoryItem(MapleInventoryType.Equip,src, dst, 1));
            c.Player.EquipChanged();
        }

        public static void Drop(MapleClient c, MapleInventoryType type, byte srcSlot, short quantity)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;

            if (srcSlot > 127)
            {
                type = MapleInventoryType.Equipped;
            }
            IMapleItem source = c.Player.Inventorys[type.Value].Inventory[srcSlot];
            if (quantity > ii.GetSlotMax(c, source.ItemId))
            {
                //try
                //{
                //    c.getChannelServer().getWorldInterface().broadcastGMMessage(c.Player.getName(), PacketCreator.serverNotice(0, c.Player.getName() + " is dropping more than slotMax.").getBytes());
                //}
                //catch (Throwable u)
                //{
                //}
            }
            if (quantity < 0 || quantity == 0 && !ii.IsThrowingStar(source.ItemId) && !ii.IsBullet(source.ItemId))
            {
                //String message = "Dropping " + quantity + " " + (source == null ? "?" : source.ItemId) + " (" +type.name() + "/" + srcSlot + ")";
                //log.info(MapleClient.getLogMessage(c, message));
                c.Close(); // disconnect the client as is inventory is inconsistent with the serverside inventory
                return;
            }
            Point dropPos = c.Player.Position;
            if (quantity < source.Quantity && !ii.IsThrowingStar(source.ItemId) && !ii.IsBullet(source.ItemId))
            {
                IMapleItem target = source.Copy();
                target.Quantity = quantity;
                source.Quantity -= quantity;
                c.Send(PacketCreator.DropInventoryItemUpdate(type, source));
                bool weddingRing = source.ItemId == 1112804;
                bool liRing = source.ItemId == 1112405;
                if (weddingRing)
                {
                    c.Player.Map.disappearingItemDrop(c.Player, c.Player, target, dropPos);
                }
                else if (liRing)
                {
                    c.Player.Map.disappearingItemDrop(c.Player, c.Player, target, dropPos);
                }
                else if (c.Player.Map.Everlast)
                {
                    if (ii.IsDropRestricted(target.ItemId))
                    {
                        c.Player.Map.disappearingItemDrop(c.Player, c.Player, target, dropPos);
                    }
                    else
                    {
                        c.Player.Map.spawnItemDrop(c.Player, c.Player, target, dropPos, true, false);
                    }
                }
                else
                {
                    if (ii.IsDropRestricted(target.ItemId))
                    {
                        c.Player.Map.disappearingItemDrop(c.Player, c.Player, target, dropPos);
                    }
                    else
                    {

                        c.Player.Map.spawnItemDrop(c.Player, c.Player, target, dropPos, true, false);

                    }
                }
            }
            else
            {
                c.Player.Inventorys[type.Value].RemoveSlot(srcSlot);
                c.Send(PacketCreator.DropInventoryItem(srcSlot > 127 ? MapleInventoryType.Equip : type, srcSlot));
                bool liRing = source.ItemId == 1112405;
                if (srcSlot > 127)
                {
                    c.Player.EquipChanged();
                }
                if (c.Player.Map.Everlast)
                {
                    if (ii.IsDropRestricted(source.ItemId))
                    {
                        c.Player.Map.disappearingItemDrop(c.Player, c.Player, source, dropPos);
                    }
                    else
                    {
                        c.Player.Map.spawnItemDrop(c.Player, c.Player, source, dropPos, true, false);
                        if (liRing)
                        {
                            c.Player.Map.disappearingItemDrop(c.Player, c.Player, source, dropPos);
                        }
                        else
                        {
                            c.Player.Map.spawnItemDrop(c.Player, c.Player, source, dropPos, true, true);
                        }
                    }
                }
                else
                {
                    if (ii.IsDropRestricted(source.ItemId))
                    {
                        c.Player.Map.disappearingItemDrop(c.Player, c.Player, source, dropPos);
                    }
                    else
                    {
                        if (liRing)
                        {
                            c.Player.Map.disappearingItemDrop(c.Player, c.Player, source, dropPos);
                        }
                        else
                        {
                            c.Player.Map.spawnItemDrop(c.Player, c.Player, source, dropPos, true, true);
                        }
                    }
                }
            }
        }
    }
}