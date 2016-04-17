using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Map;

namespace NeoMapleStory.Game.Life
{
    public abstract class AbstractLoadedMapleLife : AbstractAnimatedMapleMapObject
    {
        public int Id { get; private set; }
        public int F { get; set; }
        public bool IsHide { get; set; }
        public int Fh { get; set; }
        public int StartFh { get; private set; }
        public int Cy { get; set; }
        public int Rx0 { get; set; }
        public int Rx1 { get; set; }
        public MapleCharacter Owner { get; set; }

        public AbstractLoadedMapleLife(int id)
        {
            Id = id;
        }

        public AbstractLoadedMapleLife(AbstractLoadedMapleLife life)
        {
            Id = life.Id;
            F = life.F;
            IsHide = life.IsHide;
            Fh = life.Fh;
            StartFh = life.Fh;
            Cy = life.Cy;
            Rx0 = life.Rx0;
            Rx1 = life.Rx1;
            Owner = life.Owner;
        }
    }
}
