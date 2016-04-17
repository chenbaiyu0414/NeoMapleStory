using System;

namespace NeoMapleStory.Game.Map
{
    public abstract class AbstractAnimatedMapleMapObject : AbstractMapleMapObject, IAnimatedMapleMapObject
    {
        public bool IsFacingLeft => Math.Abs(Stance) % 2 == 1;

        public int Stance { get; set; } = 0;

    }
}
