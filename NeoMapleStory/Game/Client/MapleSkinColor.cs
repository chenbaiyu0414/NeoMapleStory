namespace NeoMapleStory.Game.Client
{
    public class MapleSkinColor
    {
        public MapleSkinColor()
        {
        }

        public MapleSkinColor(byte colorId)
        {
            ColorId = colorId;
        }

        public static MapleSkinColor Normal { get; } = new MapleSkinColor(0);
        public static MapleSkinColor Dark { get; } = new MapleSkinColor(1);
        public static MapleSkinColor Black { get; } = new MapleSkinColor(2);
        public static MapleSkinColor Pale { get; } = new MapleSkinColor(3);
        public static MapleSkinColor Blue { get; } = new MapleSkinColor(4);
        public static MapleSkinColor Pink { get; } = new MapleSkinColor(5);
        public static MapleSkinColor Yellow { get; } = new MapleSkinColor(6);
        public static MapleSkinColor Gray { get; } = new MapleSkinColor(7);
        public static MapleSkinColor Yellowbrown { get; } = new MapleSkinColor(8);
        public static MapleSkinColor White { get; } = new MapleSkinColor(9);
        public static MapleSkinColor Green { get; } = new MapleSkinColor(10);
        public static MapleSkinColor Aran { get; } = new MapleSkinColor(11);

        public byte ColorId { get; private set; }

        public static MapleSkinColor GetByColorId(int colorId)
        {
            switch (colorId)
            {
                case 0:
                    return Normal;
                case 1:
                    return Dark;
                case 2:
                    return Black;
                case 3:
                    return Pale;
                case 4:
                    return Blue;
                case 5:
                    return Pink;
                case 6:
                    return Yellow;
                case 7:
                    return Gray;
                case 8:
                    return Yellowbrown;
                case 9:
                    return White;
                case 10:
                    return Green;
                case 11:
                    return Aran;
            }
            return Normal;
        }
    }
}