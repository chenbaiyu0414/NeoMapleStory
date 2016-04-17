using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Client
{
    public class MapleBuddyList
    {
        public enum BuddyOperation
        {
            Added, Deleted
        }

        public enum BuddyAddResult
        {
            BuddylistFull, AlreadyOnList, Ok
        }

        private readonly Dictionary<int, MapleBuddyListEntry> _buddies = new Dictionary<int, MapleBuddyListEntry>();
        public int Capacity { get; set; }
        private readonly List<CharacterNameAndId> _pendingRequests = new List<CharacterNameAndId>();

        public MapleBuddyList(int capacity)
        {
            Capacity = capacity;
        }

        public bool Contains(int characterId)
        {
            return _buddies.ContainsKey(characterId);
        }

        public bool ContainsVisible(int characterId)
        {
            MapleBuddyListEntry ble;

            if (!_buddies.TryGetValue(characterId, out ble))
            {
                return false;
            }
            return ble.Visible;
        }

        public MapleBuddyListEntry this[int characterId] => _buddies[characterId];

        public MapleBuddyListEntry this[string characterName]
        {
            get
            {
                string lowerCaseName = characterName.ToLower();
                foreach (var ble in _buddies.Values)
                {
                    if (ble.CharacterName.ToLower() == lowerCaseName)
                    {
                        return ble;
                    }
                }
                return null;
            }
        }

        public void Add(MapleBuddyListEntry entry) => _buddies.Add(entry.CharacterId, entry);


        public void Remove(int characterId) => _buddies.Remove(characterId);


        public List<MapleBuddyListEntry> GetBuddies() => _buddies.Values.ToList();


        public bool IsFull() => _buddies.Count >= Capacity;


        public List<int> GetBuddyIdList() => _buddies.Keys.ToList();


        public void LoadFromDb(int characterId)
        {
            //try
            //{
            //    PreparedStatement ps = DatabaseConnection.getConnection().prepareStatement("SELECT b.buddyid, b.group, b.pending, c.name as buddyname FROM buddies as b, characters as c WHERE c.id = b.buddyid AND b.characterid = ?");
            //    ps.setInt(1, characterId);
            //    ResultSet rs = ps.executeQuery();
            //    while (rs.next())
            //    {
            //        if (rs.getInt("pending") == 1)
            //        {
            //            pendingRequests.push(new CharacterNameAndId(rs.getInt("buddyid"), rs.getString("buddyname")));
            //        }
            //        else {
            //            put(new BuddylistEntry(rs.getString("buddyname"), rs.getString("group"), rs.getInt("buddyid"), -1, true));
            //        }
            //    }
            //    rs.close();
            //    ps.close();

            //    ps = DatabaseConnection.getConnection().prepareStatement("DELETE FROM buddies WHERE pending = 1 AND characterid = ?");
            //    ps.setInt(1, characterId);
            //    ps.executeUpdate();
            //    ps.close();
            //}
            //catch
            //{
            //}
        }

        public CharacterNameAndId PollPendingRequest()
        {
            var element = _pendingRequests.LastOrDefault();
            _pendingRequests.Remove(element);
            return element;
        }

        public bool HasPendingRequestFrom(string name)
        {
            foreach (CharacterNameAndId cnai in _pendingRequests)
            {
                if (cnai.CharacterName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddBuddyRequest(MapleClient c, int cidFrom, string nameFrom, int channelFrom)
        {
            Add(new MapleBuddyListEntry(nameFrom, cidFrom, channelFrom, false));
            if (!_pendingRequests.Any())
            {
                c.Send(PacketCreator.RequestBuddylistAdd(cidFrom, nameFrom));
            }
            else {
                _pendingRequests.Add(new CharacterNameAndId(cidFrom, nameFrom));
            }
        }
    }
     public class CharacterNameAndId
    {

        public int CharacterId { get; private set; }
        public string CharacterName { get; private set; }

        public CharacterNameAndId(int id, string name)
        {
            CharacterId = id;
            CharacterName = name;
        }
    }
}
