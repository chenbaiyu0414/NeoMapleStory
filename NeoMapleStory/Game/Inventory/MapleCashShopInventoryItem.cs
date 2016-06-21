using System;

namespace NeoMapleStory.Game.Inventory
{
    public class MapleCashShopInventoryItem
    {
        public int UniqueId { get; private set; }
        public int ItemId { get; private set; }
        public int Sn { get; private set; }
        public short Quantity { get; private set; }
        public DateTime? Expire { get; set; }
        public bool IsGift { get; private set; }
        public bool IsRing { get; set; } = false;
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";

        public MapleCashShopInventoryItem(int uniqueid, int itemid, int sn, short quantity, bool gift)
        {
            UniqueId = uniqueid;
            ItemId = itemid;
            Sn = sn;
            Quantity = quantity;
            IsGift = gift;
        }

        public IMapleItem ToItem()
        {
            IMapleItem newitem;
            MapleInventoryType type = MapleItemInformationProvider.Instance.GetInventoryType(ItemId);
            if (type == MapleInventoryType.Equip)
            {
                newitem = new Equip(ItemId, 0xFF)
                {
                    Expiration = Expire,
                    UniqueId = UniqueId
                };
                ((Equip)newitem).IsRing = IsRing;
            }
            else
            {
                newitem = new Item(ItemId, 0xFF, Quantity)
                {
                    Expiration = Expire,
                    UniqueId = UniqueId
                };
            }
            return newitem;
        }
    }
}
