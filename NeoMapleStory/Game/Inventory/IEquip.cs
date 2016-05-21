namespace NeoMapleStory.Game.Inventory
{
    public interface IEquip : IMapleItem
    {
        byte UpgradeSlots { get; set; }

        byte Locked { get; set; }

        byte Level { get; set; }

        short Str { get; set; }

        short Dex { get; set; }

        short Int { get; set; }

        short Luk { get; set; }

        short Hp { get; set; }

        short Mp { get; set; }

        short Watk { get; set; }

        short Matk { get; set; }

        short Wdef { get; set; }

        short Mdef { get; set; }

        short Acc { get; set; }

        short Avoid { get; set; }

        short Hands { get; set; }

        short Speed { get; set; }

        short Jump { get; set; }

        short Vicious { get; set; }

        bool IsRing { get; set; }


        int PartnerUniqueId { get; set; }

        int PartnerId { get; set; }

        string PartnerName { get; set; }

        int ItemExp { get; set; }

        int ItemLevel { get; set; }
    }
}