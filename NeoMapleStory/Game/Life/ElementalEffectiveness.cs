using System;

namespace NeoMapleStory.Game.Life
{
     public class ElementalEffectiveness
    {
        public static ElementalEffectiveness Normal { get; } = new ElementalEffectiveness();
        public static ElementalEffectiveness Immune { get; } = new ElementalEffectiveness();
        public static ElementalEffectiveness Strong { get; } = new ElementalEffectiveness();
        public static ElementalEffectiveness Weak { get; } = new ElementalEffectiveness();
        public static ElementalEffectiveness Neutral { get; } = new ElementalEffectiveness();

        public ElementalEffectiveness()
        {

        }

        public static ElementalEffectiveness GetByNumber(int num)
        {
            switch (num)
            {
                case 1:
                    return Immune;
                case 2:
                    return Strong;
                case 3:
                    return Weak;
                case 4:
                    return Neutral;
                default:
                    throw new Exception($"未知的Effectiveness:{num}");
            }
        }
    }
}
