using System;

namespace NeoMapleStory.Game.Life
{
    public class Element
    {
        public static Element Neutral { get; } = new Element();
        public static Element Fire { get; } = new Element();
        public static Element Ice { get; } = new Element();
        public static Element Lighting { get; } = new Element();
        public static Element Poison { get; } = new Element();
        public static Element Holy { get; } = new Element();

        public static Element GetByChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'F':
                    return Fire;
                case 'I':
                    return Ice;
                case 'L':
                    return Lighting;
                case 'S':
                    return Poison;
                case 'H':
                    return Holy;
                case 'P':
                    return Neutral;
            }
            throw new Exception("未知的元素字符");
        }
    }
}