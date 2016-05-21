using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Client
{
    public class MapleBuddyList
    {
        public enum BuddyAddResult
        {
            BuddylistFull,
            AlreadyOnList,
            Ok
        }

        public enum BuddyOperation
        {
            Added,
            Deleted
        }

        private readonly Dictionary<int, MapleBuddyListEntry> m_buddies = new Dictionary<int, MapleBuddyListEntry>();
        private readonly List<CharacterNameAndId> m_pendingRequests = new List<CharacterNameAndId>();

        public MapleBuddyList(int capacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; set; }

        public MapleBuddyListEntry this[int characterId] => m_buddies[characterId];

        public MapleBuddyListEntry this[string characterName]
        {
            get
            {
                var lowerCaseName = characterName.ToLower();
                foreach (var ble in m_buddies.Values)
                {
                    if (ble.CharacterName.ToLower() == lowerCaseName)
                    {
                        return ble;
                    }
                }
                return null;
            }
        }

        public bool Contains(int characterId)
        {
            return m_buddies.ContainsKey(characterId);
        }

        public bool ContainsVisible(int characterId)
        {
            MapleBuddyListEntry ble;

            if (!m_buddies.TryGetValue(characterId, out ble))
            {
                return false;
            }
            return ble.Visible;
        }

        public void Add(MapleBuddyListEntry entry) => m_buddies.Add(entry.CharacterId, entry);


        public void Remove(int characterId) => m_buddies.Remove(characterId);


        public List<MapleBuddyListEntry> GetBuddies() => m_buddies.Values.ToList();


        public bool IsFull() => m_buddies.Count >= Capacity;


        public List<int> GetBuddyIdList() => m_buddies.Keys.ToList();


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
            var element = m_pendingRequests.LastOrDefault();
            m_pendingRequests.Remove(element);
            return element;
        }

        public bool HasPendingRequestFrom(string name)
        {
            foreach (var cnai in m_pendingRequests)
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
            if (!m_pendingRequests.Any())
            {
                c.Send(PacketCreator.RequestBuddylistAdd(cidFrom, nameFrom));
            }
            else
            {
                m_pendingRequests.Add(new CharacterNameAndId(cidFrom, nameFrom));
            }
        }
    }

    public class CharacterNameAndId
    {
        public CharacterNameAndId(int id, string name)
        {
            CharacterId = id;
            CharacterName = name;
        }

        public int CharacterId { get; private set; }
        public string CharacterName { get; }
    }
}