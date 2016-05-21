using System;
using NeoMapleStory.Core;

namespace NeoMapleStory.Game.Client.AntiCheat
{
    public class CheatingOffenseEntry
    {
        private readonly long m_mFirstOffense;

        public CheatingOffenseEntry(CheatingOffense offense, MapleCharacter chrfor)
        {
            Offense = offense;
            ToCharacter = chrfor;
            m_mFirstOffense = DateTime.Now.GetTimeMilliseconds();
        }

        public CheatingOffense Offense { get; }
        public int Count { get; private set; }
        public MapleCharacter ToCharacter { get; }
        public long LastOffense { get; private set; }

        public string Param { get; set; }
        public int DatabaseId { get; set; } = -1;

        public int Points => Count*Offense.Points;

        public void IncrementCount()
        {
            Count++;
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

        public override int GetHashCode()
        {
            var result = 1;
            result = 31*result + (ToCharacter == null ? 0 : ToCharacter.Id);
            result = 31*result + (Offense == null ? 0 : Offense.GetHashCode());
            result = 31*result + m_mFirstOffense.GetHashCode();
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
            var other = (CheatingOffenseEntry) obj;
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
            if (other.m_mFirstOffense != m_mFirstOffense)
            {
                return false;
            }
            return true;
        }
    }
}