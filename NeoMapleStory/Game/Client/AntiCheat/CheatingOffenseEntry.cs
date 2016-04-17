using System;
using NeoMapleStory.Core;

namespace NeoMapleStory.Game.Client.AntiCheat
{
     public class CheatingOffenseEntry
    {
        public CheatingOffense Offense { get; private set; }
        public int Count { get; private set; }
        public MapleCharacter ToCharacter { get; private set; }
        public long LastOffense { get; private set; }

        public string Param { get; set; }
        public int DatabaseId { get; set; } = -1;

        private readonly long _mFirstOffense;

        public CheatingOffenseEntry(CheatingOffense offense, MapleCharacter chrfor)
        {
            this.Offense = offense;
            this.ToCharacter = chrfor;
            _mFirstOffense = DateTime.Now.GetTimeMilliseconds();
        }

        public void IncrementCount()
        {
            this.Count++;
            LastOffense = DateTime.Now.GetTimeMilliseconds();
        }

        public bool IsExpired()
        {
            if (LastOffense < DateTime.Now.GetTimeMilliseconds() - Offense.ValidityDuration)
            {
                return true;
            }
            return false;
        }

        public int Points => Count * Offense.Points;

        public override int GetHashCode()
        {
            int result = 1;
            result = 31 * result + (ToCharacter == null ? 0 : ToCharacter.Id);
            result = 31 * result + (Offense == null ? 0 : Offense.GetHashCode());
            result = 31 * result + _mFirstOffense.GetHashCode();
            return result;
        }

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
            CheatingOffenseEntry other = (CheatingOffenseEntry)obj;
            if (ToCharacter == null)
            {
                if (other.ToCharacter != null)
                {
                    return false;
                }
            }
            else if (ToCharacter.Id != other.ToCharacter.Id)
            {
                return false;
            }
            if (Offense == null)
            {
                if (other.Offense != null)
                {
                    return false;
                }
            }
            else if (!Offense.Equals(other.Offense))
            {
                return false;
            }
            if (other._mFirstOffense != _mFirstOffense)
            {
                return false;
            }
            return true;
        }
    }
}
