using System;
using System.Collections.Generic;

namespace NeoMapleStory.Game.Inventory
{
    public class Item : IMapleItem
    {
        public Item(int id, byte position, short quantity)
        {
            ItemId = id;
            Position = position;
            Quantity = quantity;
            Flag = 0;
            var ii = MapleItemInformationProvider.Instance;
            if (ii.IsCash(ItemId))
            {
                //检测是否是商城点装
                UniqueId = ii.GetNextUniqueId(); //为物品ID设置UNIQUEID
            }
        }

        public int SenderId { get; set; }

        public string SendMessage { get; set; } = "";

        private List<int> PetsCanConsume { get; set; } = new List<int>();

        public int ItemId { get; }

        public byte Flag { get; set; }

        public byte Position { get; set; }

        public string Owner { get; set; }

        public short Quantity { get; set; }

        public DateTime? Expiration { get; set; }

        public int Sn { get; set; }

        public int UniqueId { get; set; }

        public MapleItemType Type => MapleItemType.Item;

        public IMapleItem Copy()
        {
            var ret = new Item(ItemId, Position, Quantity) {Owner = Owner};
            return ret;
        }

        public int CompareTo(IMapleItem other)
        {
            if (Math.Abs(Position) < Math.Abs(other.Position))
                return -1;
            return Math.Abs(Position) == Math.Abs(other.Position) ? 0 : 1;
        }

        public override string ToString() => $"Item: {ItemId} Quantity: {Quantity}";
    }
}