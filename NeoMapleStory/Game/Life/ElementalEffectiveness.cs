using System;

namespace NeoMapleStory.Game.Life
{
    public class ElementalEffectiveness
    {


        public ElementalEffectiveness(int value)
        {
            Value = value;
        }

        public static ElementalEffectiveness Normal { get; } = new ElementalEffectiveness(0);
        public static ElementalEffectiveness Immune { get; } = new ElementalEffectiveness(1);
        public static ElementalEffectiveness Strong { get; } = new ElementalEffectiveness(2);
        public static ElementalEffectiveness Weak { get; } = new ElementalEffectiveness(3);
        public static ElementalEffectiveness Neutral { get; } = new ElementalEffectiveness(4);

        public int Value { get; set; }

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

        public static bool operator ==(ElementalEffectiveness e1, ElementalEffectiveness e2)
        {
            if (e1 == null || e2 == null)
                return false;
            return e1.Value == e2.Value;
        }

        public static bool operator !=(ElementalEffectiveness e1, ElementalEffectiveness e2)
        {
            return !(e1 == e2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Value == ((ElementalEffectiveness) obj).Value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}