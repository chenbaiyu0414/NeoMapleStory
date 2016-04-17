using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.World
{
    public class MapleMessenger
    {
        public int Id { get; set; }

        private readonly List<MapleMessengerCharacter> _mMembers = new List<MapleMessengerCharacter>();        
        private bool _mPos0;
        private bool _mPos1;
        private bool _mPos2;

        public MapleMessenger(int id, MapleMessengerCharacter chrfor)
        {
            _mMembers.Add(chrfor);
            chrfor.Position= GetLowestPosition();
            Id = id;
        }

        public bool ContainsMembers(MapleMessengerCharacter member)=> _mMembers.Contains(member);


        public void AddMember(MapleMessengerCharacter member)
        {
            _mMembers.Add(member);
            member.Position = GetLowestPosition();
        }

        public void RemoveMember(MapleMessengerCharacter member)
        {
            int position = member.Position;
            if (position == 0)
            {
                _mPos0 = false;
            }
            else if (position == 1)
            {
                _mPos1 = false;
            }
            else if (position == 2)
            {
                _mPos2 = false;
            }
            _mMembers.Remove(member);
        }

        public void SilentRemoveMember(MapleMessengerCharacter member) => _mMembers.Remove(member);

        public void SilentAddMember(MapleMessengerCharacter member, int position)
        {
            _mMembers.Add(member);
            member.Position = position;
        }

        public void UpdateMember(MapleMessengerCharacter member)
        {
            for (int i = 0; i < _mMembers.Count; i++)
            {
                MapleMessengerCharacter chr = _mMembers[i];
                if (chr == member)
                {
                    _mMembers[i] = member;
                }
            }
        }

        public List<MapleMessengerCharacter> GetMembers() => _mMembers;

        public int GetLowestPosition()
        {
            int position;
            if (_mPos0)
            {
                if (_mPos1)
                {
                    _mPos2 = true;
                    position = 2;
                }
                else {
                    _mPos1 = true;
                    position = 1;
                }
            }
            else {
                _mPos0 = true;
                position = 0;
            }
            return position;
        }

        public int GetPositionByName(string name) => _mMembers.FirstOrDefault(x => x.CharacterName == name)?.Position ?? 4;

        public override int GetHashCode()=> 31 + Id;

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
            MapleMessenger other = (MapleMessenger)obj;
            if (Id != other.Id)
            {
                return false;
            }
            return true;
        }
    }
}
