using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.World
{
    public class PlayerBuffValueHolder
    {
        public long StartTime { get; private set; }
        public MapleStatEffect Effect { get; private set; }

        private readonly int _mId;

        public PlayerBuffValueHolder(long startTime, MapleStatEffect effect)
        {
            StartTime = startTime;
            Effect = effect;
            _mId = (int)(Randomizer.NextDouble() * 100);
        }

        public override int GetHashCode() => 1 * 31 + _mId;

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this != obj)
            {
                return false;
            }
            PlayerBuffValueHolder other = (PlayerBuffValueHolder)obj;
            if (_mId != other._mId)
            {
                return false;
            }
            return true;
        }
    }
}
