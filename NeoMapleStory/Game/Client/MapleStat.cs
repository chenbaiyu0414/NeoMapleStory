using System;

namespace NeoMapleStory.Game.Client
{
    public enum MapleStat
    {
        Unfind = 0x0,
        Skin = 0x1,
        Face = 0x2,
        Hair = 0x4,
        Level = 0x40,
        Job = 0x80,
        Str = 0x100,
        Dex = 0x200,
        Int = 0x400,
        Luk = 0x800,
        Hp = 0x1000,
        Maxhp = 0x2000,
        Mp = 0x4000,
        Maxmp = 0x8000,
        Availableap = 0x10000,
        Availablesp = 0x20000,
        Exp = 0x40000,
        Fame = 0x80000,
        Meso = 0x100000,
        Pet = 0x200000
    }
    public static class MapleStatExtension
    {
        public static MapleStat GetByValue(int value)
        {
            MapleStat result;
            return Enum.TryParse(value.ToString(), out result) ? result : MapleStat.Unfind;
        }
    }
}
