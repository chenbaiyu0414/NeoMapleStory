using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Inventory
{
    public sealed class MapleInventory
    {
        public MapleInventory()
        {
        }

        public MapleInventory(MapleInventoryType type, byte slotLimit)
        {
            Inventory = new Dictionary<byte, IMapleItem>();
            SlotLimit = slotLimit;
            Type = type;
        }

        public Dictionary<byte, IMapleItem> Inventory { get; } = new Dictionary<byte, IMapleItem>();

        public byte SlotLimit { get; set; }

        public MapleInventoryType Type { get; }

        public IMapleItem FindById(int itemId) => Inventory.Values.FirstOrDefault(x => x.ItemId == itemId);

        public IMapleItem FindByUniqueId(int uniqueId) => Inventory.Values.FirstOrDefault(x => x.UniqueId == uniqueId);

        public int CountById(int itemId) => Inventory.Values.Where(x => x.ItemId == itemId).Sum(x => x.Quantity);

        public List<byte> FindAllKeysById(int itemId)
            => Inventory.Values.Where(x => x.ItemId == itemId).Select(x => x.Position).ToList();

        public List<IMapleItem> ListById(int itemId)
        {
            var list = Inventory.Values.Where(x => x.ItemId == itemId).Select(x => x).ToList();
            list.Sort();
            return list;
        }

        public byte AddItem(IMapleItem item)
        {
            var slotId = GetNextFreeSlot();

            if (slotId == 0xFF)
                return 0xFF;

            if (Inventory.ContainsKey(slotId))
                Inventory[slotId] = item;
            else
                Inventory.Add(slotId, item);
            item.Position = slotId;
            return slotId;
        }

        public byte GetNextFreeSlot()
        {
            if (IsFull())
                return 0xFF;

            for (byte i = 1; i <= SlotLimit; i++)
            {
                if (!Inventory.ContainsKey(i))
                {
                    return i;
                }
            }
            return 0xFF;
        }

        public void AddFromDb(IMapleItem item)
        {
            if (item.Position > 127 && Type != MapleInventoryType.Equipped)
            {
                Console.WriteLine($"Item with negative position in non-equipped IV wtf? ID:{item.ItemId}");
            }

            if (Inventory.ContainsKey(item.Position))
                Inventory[item.Position] = item;
            else
                Inventory.Add(item.Position, item);
        }

        public bool Move(byte sSlot, byte dSlot, short slotMax)
        {
            var ii = MapleItemInformationProvider.Instance;
            IMapleItem iSource;
            IMapleItem iTarget;

            if (!Inventory.TryGetValue(sSlot, out iSource))
            {
                throw new Exception("Trying to move empty slot");
            }

            if (!Inventory.TryGetValue(dSlot, out iTarget))
            {
                iSource.Position = dSlot;
                if (Inventory.ContainsKey(dSlot))
                    Inventory[dSlot] = iSource;
                else
                    Inventory.Add(dSlot, iSource);
                Inventory.Remove(sSlot);
            }
            else if (((Item)iTarget).ItemId == ((Item)iSource).ItemId && !ii.IsThrowingStar(((Item)iSource).ItemId) && !ii.IsBullet(((Item)iSource).ItemId))
            {
                var source = (Item)iSource;
                var target = (Item)iTarget;
                if (Type.Value == MapleInventoryType.Equip.Value)
                {
                    Swap(target, source);
                }
                if (source.Quantity + target.Quantity > slotMax)
                {
                    var rest = (short)(source.Quantity + target.Quantity - slotMax);
                    if (rest + slotMax != source.Quantity + target.Quantity)
                    {
                        return false;
                    }
                    source.Quantity = rest;
                    target.Quantity = slotMax;
                }
                else
                {
                    target.Quantity = (short)(source.Quantity + target.Quantity);
                    Inventory.Remove(sSlot);
                }
            }
            else
            {
                Swap((Item)iTarget, (Item)iSource);
            }
            return true;
        }

        private void Swap(IMapleItem source, IMapleItem target)
        {
            Inventory.Remove(source.Position);
            Inventory.Remove(target.Position);

            var swapPos = source.Position;

            source.Position = target.Position;
            target.Position = swapPos;

            if (Inventory.ContainsKey(source.Position))
                Inventory[source.Position] = source;
            else
                Inventory.Add(source.Position, source);

            if (Inventory.ContainsKey(target.Position))
                Inventory[target.Position] = target;
            else
                Inventory.Add(target.Position, target);
        }

        public void RemoveItem(byte slot, short quantity = 1, bool allowZero = false)
        {
            IMapleItem item;

            if (!Inventory.TryGetValue(slot, out item))
                return;

            item.Quantity -= quantity;

            if (item.Quantity < 0)
                item.Quantity = 0;

            if (item.Quantity == 0 && !allowZero)
                RemoveSlot(slot);
        }

        public void RemoveSlot(byte slot) => Inventory.Remove(slot);

        public bool IsFull() => Inventory.Count >= SlotLimit;

        public bool IsFull(int margin) => Inventory.Count + margin >= SlotLimit;

        public int CountItemType(int charid, MapleInventoryType itemtype)
        {
            //int it = (int)itemtype.getType();
            //try
            //{
            //    Connection con = DatabaseConnection.getConnection();
            //    PreparedStatement ps = con.prepareStatement("SELECT COUNT(*) AS c FROM inventoryitems WHERE characterid = ? AND inventorytype = ?");
            //    ps.setInt(1, charid);
            //    ps.setInt(2, it);
            //    ResultSet rs = ps.executeQuery();
            //    if (rs.next())
            //    {
            //        return Integer.parseInt(rs.getString("c"));
            //    }
            //    rs.close();
            //    ps.close();
            //}
            //catch (Exception e)
            //{
            //    e.printStackTrace();
            //}
            return 0;
        }
    }
}