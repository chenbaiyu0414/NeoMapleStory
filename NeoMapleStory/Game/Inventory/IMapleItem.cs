using System;

namespace NeoMapleStory.Game.Inventory
{
    public enum MapleItemType : byte
    {
        Equip = 1,
        Item,
        Pet
    }

    public interface IMapleItem : IComparable<IMapleItem>
    {
        int ItemId { get; }

        byte Flag { get; set; }

        byte Position { get; set; }

        string Owner { get; set; }

        short Quantity { get; set; }

        DateTime? Expiration { get; set; }

        int Sn { get; set; }

        int UniqueId { get; set; }

        MapleItemType Type { get; }

        IMapleItem Copy();
    }
}