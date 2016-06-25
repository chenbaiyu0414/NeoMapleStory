using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using Newtonsoft.Json;

namespace NeoMapleStory.Game.Shop
{
    class ShopMapping
    {
        public int ShopId { get; set; }
        public int NpcId { get; set; }
    }
    class OriginalShopItem
    {
        public int ShopId { get; set; }
        public int ItemId { get; set; }
        public int Price { get; set; }
        public byte Position { get; set; }
    }
    public class MapleShop
    {
        private static readonly List<int> MRechargeableItems = new List<int>();
        private readonly List<MapleShopItem> m_mItems;

        static MapleShop()
        {
            for (var i = 2070000; i <= 2070018; i++)
            {
                MRechargeableItems.Add(i);
            }
            MRechargeableItems.Remove(2070014); // doesn't exist
            MRechargeableItems.Remove(2070015);
            MRechargeableItems.Remove(2070016);
            MRechargeableItems.Remove(2070017);
            MRechargeableItems.Remove(2070018);

            for (var i = 2330000; i <= 2330005; i++)
            {
                MRechargeableItems.Add(i);
            }
            MRechargeableItems.Add(2331000); //Blaze Capsule
            MRechargeableItems.Add(2332000); //Glaze Capsule
        }

        private MapleShop(int id, int npcId)
        {
            ShopId = id;
            Npcid = npcId;
            m_mItems = new List<MapleShopItem>();
        }

        public int ShopId { get; private set; }
        public int Npcid { get; }

        public void AddItem(MapleShopItem item) => m_mItems.Add(item);


        public void SendShop(MapleClient c)
        {
            c.Player.Shop = this;
            c.Send(PacketCreator.GetNpcShop(c, Npcid, m_mItems));
        }

        public void Buy(MapleClient c, int itemId, short quantity)
        {
            if (quantity <= 0)
            {
                Console.WriteLine($"{c.Player.Name} is buying an invalid amount: { quantity } of itemid: { itemId}");
                c.Close();
                return;
            }
            MapleShopItem item = FindByItemId(itemId);
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            if (item != null && item.Price > 0 && c.Player.Meso.Value >= item.Price * quantity)
            {
                if (MapleInventoryManipulator.CheckSpace(c, itemId, quantity, ""))
                {
                    if (itemId >= 5000000 && itemId <= 5000100)
                    {
                        if (quantity > 1)
                        {
                            quantity = 1;
                        }
                        int petId = MaplePet.Create(itemId);
                        MapleInventoryManipulator.AddById(c, itemId, quantity, "Pet was purchased.", null, petId);
                    }
                    else if (ii.IsRechargable(itemId))
                    {
                        short rechquantity = ii.GetSlotMax(c, item.ItemId);
                        MapleInventoryManipulator.AddById(c, itemId, rechquantity, "Rechargable item purchased.", null, -1);
                    }
                    else
                    {
                        MapleInventoryManipulator.AddById(c, itemId, quantity, c.Player.Name + " bought " + quantity + " for " + item.Price * quantity + " from shop " + ShopId);
                    }
                    c.Player.GainMeso(-(item.Price * quantity), false);
                    c.Send(PacketCreator.ConfirmShopTransaction(0));
                }
                else
                {
                    c.Send(PacketCreator.ConfirmShopTransaction(3));
                }
            }
        }

        public void Sell(MapleClient c, MapleInventoryType type, byte slot, short quantity)
        {
            if (quantity == short.MinValue || quantity == 0)
            {
                quantity = 1;
            }

            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            IMapleItem item = c.Player.Inventorys[type.Value].Inventory[slot];
            if (ii.IsThrowingStar(item.ItemId))
            {
                quantity = item.Quantity;
            }
            if (quantity < 0)
            {
                AutobanManager.Instance.AddPoints(c, 1000, 0, "Selling " + quantity + " " + item.ItemId + " (" + type + "/" + slot + ")");
                return;
            }
            short iQuant = item.Quantity;

            if (iQuant == short.MinValue)
            {
                iQuant = 1;
            }

            if (quantity <= iQuant && iQuant > 0)
            {
                MapleInventoryManipulator.RemoveFromSlot(c, type, slot, quantity, false);
                double price;
                if (ii.IsThrowingStar(item.ItemId))
                {
                    price = ii.GetWholePrice(item.ItemId) / (double)ii.GetSlotMax(c, item.ItemId);
                }
                else
                {
                    price = ii.GetPrice(item.ItemId);
                }
                int recvMesos = (int)Math.Max(Math.Ceiling(price * quantity), 0);
                if (Math.Abs(price + 1) > 0.000001 && recvMesos > 0)
                {
                    c.Player.GainMeso(recvMesos, true);
                }

                c.Send(PacketCreator.ConfirmShopTransaction(0x8));
            }
        }

        public void ReCharge(MapleClient c, byte slot)
        {
            var ii = MapleItemInformationProvider.Instance;

            IMapleItem item;

            if (!c.Player.Inventorys[MapleInventoryType.Use.Value].Inventory.TryGetValue(slot, out item) || (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId)))
            {
                if (item != null && (!ii.IsThrowingStar(item.ItemId) || !ii.IsBullet(item.ItemId)))
                {
                    Console.WriteLine($"{c.Player.Name} is trying to recharge {item.ItemId}");
                }
                return;
            }
            var slotMax = ii.GetSlotMax(c, item.ItemId);

            if (item.Quantity < 0)
            {
                Console.WriteLine($"{c.Player.Name} is trying to recharge {item.ItemId} with quantity {item.Quantity}");
            }
            if (item.Quantity < slotMax)
            {
                var price = (int)Math.Round(ii.GetPrice(item.ItemId) * (slotMax - item.Quantity));
                if (c.Player.Meso.Value >= price)
                {
                    item.Quantity = slotMax;
                    c.Send(PacketCreator.UpdateInventorySlot(MapleInventoryType.Use, (Item)item));
                    c.Player.GainMeso(-price, false, true, false);
                    c.Send(PacketCreator.ConfirmShopTransaction(0x8));
                }
            }
        }

        protected MapleShopItem FindByItemId(int itemId) => m_mItems.FirstOrDefault(x => x.ItemId == itemId);


        public static MapleShop Create(int id, bool isShopId)
        {
            MapleShop ret = null;
            var ii = MapleItemInformationProvider.Instance;

            try
            {
                var shopMappings = JsonConvert.DeserializeObject<List<ShopMapping>>(File.ReadAllText($"{Environment.CurrentDirectory}\\Json\\Shops.json"));
                var shopQuery = shopMappings.Where(x => id == (isShopId ? x.ShopId : x.NpcId)).Select(x => x).FirstOrDefault();

                int shopId;
                if (shopQuery != null)
                    shopId = shopQuery.ShopId;
                else
                    return null;

                ret = new MapleShop(shopId, shopQuery.NpcId);

                List<int> recharges = new List<int>(MRechargeableItems);

                var items = JsonConvert.DeserializeObject<List<OriginalShopItem>>(File.ReadAllText($"{Environment.CurrentDirectory}\\Json\\ShopItems.json"));

                foreach (var item in items.Where(x => x.ShopId == shopId))
                {
                    int itemId = item.ItemId;
                    if (ii.IsThrowingStar(itemId) || ii.IsBullet(itemId))
                    {
                        MapleShopItem starItem = new MapleShopItem(1, itemId, item.Price);
                        ret.AddItem(starItem);
                        if (MRechargeableItems.Contains(starItem.ItemId))
                        {
                            recharges.Remove(starItem.ItemId);
                        }
                    }
                    else
                    {
                        ret.AddItem(new MapleShopItem(1000, itemId, item.Price));
                    }
                }

                foreach (var itemId in recharges)
                {
                    ret.AddItem(new MapleShopItem(1000, itemId, 0));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not load shop" + e);
            }

            return ret;
        }
    }
}