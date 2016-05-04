using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Shop
{
    public class MapleShop
    {
        private static readonly List<int> MRechargeableItems = new List<int>();
        private readonly List<MapleShopItem> _mItems;

        public int ShopId { get; private set; }
        public int Npcid { get; private set; }

        static MapleShop()
        {
            for (int i = 2070000; i <= 2070018; i++)
            {
                MRechargeableItems.Add(i);
            }
            MRechargeableItems.Remove(2070014); // doesn't exist
            MRechargeableItems.Remove(2070015);
            MRechargeableItems.Remove(2070016);
            MRechargeableItems.Remove(2070017);
            MRechargeableItems.Remove(2070018);

            for (int i = 2330000; i <= 2330005; i++)
            {
                MRechargeableItems.Add(i);
            }
            MRechargeableItems.Add(2331000);//Blaze Capsule
            MRechargeableItems.Add(2332000);//Glaze Capsule
        }

        private MapleShop(int id, int npcId)
        {
            ShopId = id;
            Npcid = npcId;
            _mItems = new List<MapleShopItem>();
        }

        public void AddItem(MapleShopItem item) => _mItems.Add(item);


        public void SendShop(MapleClient c)
        {
            c.Player.Shop = this;
            c.Send(PacketCreator.GetNpcShop(c, Npcid, _mItems));
        }

        //public void Buy(MapleClient c, int itemId, short quantity)
        //{
        //    if (quantity <= 0)
        //    {
        //        Console.WriteLine($"{c.Character.CharacterName} is buying an invalid amount: { quantity } of itemid: { itemId}");
        //        c.Close();
        //        return;
        //    }
        //    MapleShopItem item = FindByItemID(itemId);
        //    MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
        //    if (item != null && item.Price > 0 && c.Character.Money.Value >= item.Price * quantity)
        //    {
        //        if (MapleInventoryManipulator.checkSpace(c, itemId, quantity, ""))
        //        {
        //            if (itemId >= 5000000 && itemId <= 5000100)
        //            {
        //                if (quantity > 1)
        //                {
        //                    quantity = 1;
        //                }
        //                int petId = MaplePet.createPet(itemId);
        //                MapleInventoryManipulator.addById(c, itemId, quantity, "Pet was purchased.", null, petId);
        //            }
        //            else if (ii.IsRechargable(itemId))
        //            {
        //                short rechquantity = ii.GetSlotMax(c, item.ItemID);
        //                MapleInventoryManipulator.addById(c, itemId, rechquantity, "Rechargable item purchased.", null, -1);
        //            }
        //            else {
        //                MapleInventoryManipulator.addById(c, itemId, quantity, c.Character.CharacterName + " bought " + quantity + " for " + item.Price * quantity + " from shop " + ShopID, null, -1);
        //            }
        //            c.Character.GainMeso(-(item.Price * quantity), false);
        //            c.Send(PacketCreator.ConfirmShopTransaction(0));
        //        }
        //        else {
        //            c.Send(PacketCreator.ConfirmShopTransaction(3));
        //        }
        //    }
        //}

        //public void Sell(MapleClient c, MapleInventoryType type, sbyte slot, short quantity)
        //{
        //    if (quantity == short.MinValue || quantity == 0)
        //    {
        //        quantity = 1;
        //    }

        //    MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
        //    IMapleItem item = c.Character.Inventorys[type.Value].Inventory[slot];
        //    if (ii.IsThrowingStar(item.ItemID))
        //    {
        //        quantity = item.Quantity;
        //    }
        //    if (quantity < 0)
        //    {
        //        AutobanManager.getInstance().addPoints(c, 1000, 0, "Selling " + quantity + " " + item.ItemID + " (" + type.ToString() + "/" + slot + ")");
        //        return;
        //    }
        //    short iQuant = item.Quantity;

        //    if (iQuant == short.MinValue)
        //    {
        //        iQuant = 1;
        //    }

        //    if (quantity <= iQuant && iQuant > 0)
        //    {
        //        MapleInventoryManipulator.removeFromSlot(c, type, slot, quantity, false);
        //        double price;
        //        if (ii.IsThrowingStar(item.ItemID))
        //        {
        //            price = ii.GetWholePrice(item.ItemID) / (double)ii.GetSlotMax(c, item.ItemID);
        //        }
        //        else {
        //            price = ii.GetPrice(item.ItemID);
        //        }
        //        int recvMesos = (int)Math.Max(Math.Ceiling(price * quantity), 0);
        //        if (price != -1 && recvMesos > 0)
        //        {
        //            c.Character.GainMeso(recvMesos, true);
        //        }

        //        c.Send(PacketCreator.ConfirmShopTransaction(0x8));
        //    }
        //}

        public void ReCharge(MapleClient c, byte slot)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;

            IMapleItem item = c.Player.Inventorys[MapleInventoryType.Use.Value].Inventory[slot];

            if (item == null || (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId)))
            {
                if (item != null && (!ii.IsThrowingStar(item.ItemId) || !ii.IsBullet(item.ItemId)))
                {
                    Console.WriteLine($"{c.Player.Name } is trying to recharge { item.ItemId}");
                }
                return;
            }
            short slotMax = ii.GetSlotMax(c, item.ItemId);

            if (item.Quantity < 0)
            {
                Console.WriteLine($"{c.Player.Name } is trying to recharge { item.ItemId} with quantity { item.Quantity}");
            }
            if (item.Quantity < slotMax)
            {
                int price = (int)Math.Round(ii.GetPrice(item.ItemId) * (slotMax - item.Quantity));
                if (c.Player.Money.Value >= price)
                {
                    item.Quantity = slotMax;
                    c.Send(PacketCreator.UpdateInventorySlot(MapleInventoryType.Use, (Item)item));
                    c.Player.GainMeso(-price, false, true, false);
                    c.Send(PacketCreator.ConfirmShopTransaction(0x8));
                }
            }
        }

        protected MapleShopItem FindByItemId(int itemId) => _mItems.FirstOrDefault(x => x.ItemId == itemId);


        public static MapleShop CreateFromDb(int id, bool isShopId)
        {
            MapleShop ret = null;
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            int shopId;
            //try
            //{
            //    using (var db = new NeoDatabase())
            //    {
            //        var result = db.Shops.Where(x => id == (isShopId ? x.ShopId : x.Npcid)).Select(x => x).FirstOrDefault();

            //        if (result != null)
            //            shopId = result.ShopId;
            //        else
            //            return null;

            //        ret = new MapleShop(shopId, result.Npcid);

            //        List<int> recharges = new List<int>(MRechargeableItems);
            //        var shopItem = db.ShopItems.Where(x => x.ShopId == shopId).Select(x => x).OrderBy(x => x.Position);
            //        foreach (var item in shopItem)
            //        {
            //            if (ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId))
            //            {
            //                MapleShopItem starItem = new MapleShopItem(1, item.ItemId, item.Price);
            //                ret.AddItem(starItem);
            //                if (MRechargeableItems.Contains(starItem.ItemId))
            //                {
            //                    recharges.Remove(starItem.ItemId);
            //                }
            //            }
            //            else {
            //                ret.AddItem(new MapleShopItem(1000, item.ItemId, item.Price));
            //            }
            //        }


            //        foreach (var itemId in recharges)
            //        {
            //            ret.AddItem(new MapleShopItem(1000, itemId, 0));
            //        }

            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Could not load shop" + e);
            //}

            return null;
        }
    }
}
