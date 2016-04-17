namespace NeoMapleStory.Game.Client
{
    public enum MapleDisease : long
    {
        Null = 0x0,
        Slow = 0x1,//缓慢
        Seduce = 0x80,//诱惑
        Fishable = 0x100,//钓鱼
        Curse = 0x200,// 诅咒
        Confuse = 0x80000,//诱惑
        Stun = 0x2000000000000L,//眩晕
        Poison = 0x4000000000000L,//中毒
        Seal = 0x8000000000000L,//封印
        Darkness = 0x10000000000000L,//黑暗
        Weaken = 0x4000000000000000L//减弱
    }
}
