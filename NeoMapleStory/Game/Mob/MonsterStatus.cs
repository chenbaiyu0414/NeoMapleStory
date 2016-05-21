namespace NeoMapleStory.Game.Mob
{
    public enum MonsterStatus
    {
        Watk = 0x1,
        Wdef = 0x2,
        Matk = 0x4,
        Mdef = 0x8,
        Acc = 0x10,
        Avoid = 0x20,
        Speed = 0x40,
        Stun = 0x80, //this is possibly only the bowman stun
        Freeze = 0x100,
        Poison = 0x200,
        Seal = 0x400,
        Taunt = 0x800,
        WeaponAttackUp = 0x1000,
        WeaponDefenseUp = 0x2000,
        MagicAttackUp = 0x4000,
        MagicDefenseUp = 0x8000,
        Doom = 0x10000,
        ShadowWeb = 0x20000,
        WeaponImmunity = 0x40000,
        MagicImmunity = 0x80000,
        NinjaAmbush = 0x400000,
        Hypnotized = 0x10000000
    }
}