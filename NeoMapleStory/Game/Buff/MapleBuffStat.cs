namespace NeoMapleStory.Game.Buff
{
    public enum MapleBuffStat : long
    {
        Morph = 0x2, //变形
        Recovery = 0x4, //恢复
        MapleWarrior = 0x8, //冒险岛勇士
        Stance = 0x10,
        SharpEyes = 0x20, //火眼晶晶 - 赋予组队成员针对敌人寻找弱点并给予敌人致命伤的能力
        ManaReflection = 0x40, //魔法反击
        ShadowClaw = 0x100, // 暗器伤人
        Infinity = 0x20000000000000L, //终极无限 - 一定时间内搜集周围的魔力,不消耗魔法值
        HolyShield = 0x400, //圣灵之盾
        Hamstring = 0x800,
        Blind = 0x1000,
        Concentrate = 0x2000, // another no op buff
        EchoOfHero = 0x8000,
        GhostMorph = 0x20000, // ??? Morphs you into a ghost - no idea what for
        Dash = 0x60000000, //0x60000000
        BerserkFury = 0x8000000,
        EnergyCharge = 0x800000000L,
        MonsterRiding = 0x10000000000L,
        Watk = 0x100000000L,
        Wdef = 0x200000000L,
        Matk = 0x400000000L,
        Mdef = 0x800000000L,
        Acc = 0x1000000000L,
        Avoid = 0x2000000000L,
        Hands = 0x4000000000L,
        Speed = 0x8000000000L,
        Jump = 0x10000000000L,
        MagicGuard = 0x20000000000L,
        Darksight = 0x40000000000L, // also used by gm hide
        Booster = 0x80000000000L,
        SpeedInfusion = 0x800000000000L,
        Powerguard = 0x100000000000L,
        Hyperbodyhp = 0x200000000000L,
        Hyperbodymp = 0x400000000000L,
        Invincible = 0x800000000000L,
        Soularrow = 0x1000000000000L,
        Stun = 0x2000000000000L,
        Poison = 0x4000000000000L,
        Seal = 0x8000000000000L,
        Darkness = 0x10000000000000L,
        Combo = 0x20000000000000L,
        Summon = 0x20000000000000L, //hack buffstat for summons ^.- =does/should not increase damage... hopefully <3
        WkCharge = 0x40000000000000L,
        Dragonblood = 0x80000000000000L, // another funny buffstat...
        HolySymbol = 0x100000000000000L,
        Mesoup = 0x200000000000000L,
        Shadowpartner = 0x400000000000000L,
        //0x8000000000000
        Pickpocket = 0x800000000000000L,
        Puppet = 0x800000000000000L, // HACK - shares buffmask with pickpocket - odin special ^.-
        Mesoguard = 0x1000000000000000L,
        Weaken = 0x4000000000000000L //SWITCH_CONTROLS=0x8000000000000L
    }
}