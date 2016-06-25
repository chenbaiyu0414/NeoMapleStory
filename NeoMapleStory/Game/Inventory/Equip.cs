using System;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Inventory
{
    public class Equip : Item, IEquip
    {
        public Equip(int id, byte position)
            : base(id, position, 1)
        {
            IsRing = false;
        }

        public Equip(int id, byte position, bool ring)
            : base(id, position, 1)
        {
            IsRing = false;
            ItemExp = 0;
            ItemLevel = 0;
        }

        public Equip(int id, byte position, bool ring, int partnerUniqueId, int partnerId, string partnerName)
            : base(id, position, 1)
        {
            IsRing = false;
            PartnerUniqueId = partnerUniqueId;
            PartnerId = partnerId;
            PartnerName = partnerName;
        }

        public MapleJob Job { get; set; }
        public byte UpgradeSlots { get; set; }
        public byte Level { get; set; }
        public byte Locked { get; set; }
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Int { get; set; }
        public short Luk { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short Watk { get; set; }
        public short Matk { get; set; }
        public short Wdef { get; set; }
        public short Mdef { get; set; }
        public short Acc { get; set; }
        public short Avoid { get; set; }
        public short Hands { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Vicious { get; set; }
        public bool IsRing { get; set; }
        public int PartnerUniqueId { get; set; }
        public int PartnerId { get; set; }
        public string PartnerName { get; set; } = "";
        public int ItemExp { get; set; }
        public int ItemLevel { get; set; }
        public ScrollResult ScrollResult { get; set; }
        public new MapleItemType Type => MapleItemType.Equip;

        public new IMapleItem Copy()
        {
            var ret = new Equip(ItemId, Position, IsRing)
            {
                Str = Str,
                Dex = Dex,
                Int = Int,
                Luk = Luk,
                Hp = Hp,
                Mp = Mp,
                Matk = Matk,
                Mdef = Mdef,
                Watk = Watk,
                Wdef = Wdef,
                Acc = Acc,
                Avoid = Avoid,
                Hands = Hands,
                Speed = Speed,
                Jump = Jump,
                Flag = Flag,
                Locked = Locked,
                UpgradeSlots = UpgradeSlots,
                Level = Level,
                Vicious = Vicious,
                Owner = Owner,
                Quantity = Quantity
            };
            return ret;
        }

        public new short Quantity
        {
            get { return base.Quantity; }
            set
            {
                if (value < 0 || value > 1)
                    Console.WriteLine($"Setting the quantity to {Quantity} on an equip (itemid: {ItemId})");
                base.Quantity = value;
            }
        }

        public void GainItemExp(MapleClient c, int gain, bool timeless)
        {
            //itemExp += gain;
            //int expNeeded = 0;
            //if (timeless)
            //    expNeeded = ExpTable.getTimelessItemExpNeededForLevel(itemLevel + 1);
            //else
            //    expNeeded = ExpTable.getReverseItemExpNeededForLevel(itemLevel + 1);
            //if (itemExp >= expNeeded)
            //{
            //    gainLevel();
            //    c.getSession().write(MaplePacketCreator.showItemLevelup());
            //}
        }
    }
}