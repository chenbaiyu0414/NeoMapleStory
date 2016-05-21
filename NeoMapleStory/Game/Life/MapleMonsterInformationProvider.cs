using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Mob;

namespace NeoMapleStory.Game.Life
{
    public class MapleMonsterInformationProvider
    {
        public const int ApproxFadeDelay = 90;
        private readonly Dictionary<int, List<DropEntry>> m_drops = new Dictionary<int, List<DropEntry>>();

        public static MapleMonsterInformationProvider Instance { get; private set; } =
            new MapleMonsterInformationProvider();


        public List<DropEntry> RetrieveDropChances(int monsterid)
        {
            if (m_drops.ContainsKey(monsterid))
            {
                return m_drops[monsterid];
            }

            var ret = new List<DropEntry>();
            if (monsterid > 9300183 && monsterid < 9300216)
            {
                for (var i = 2022359; i < 2022367; i++)
                {
                    ret.Add(new DropEntry(i, 10));
                }

                if (m_drops.ContainsKey(monsterid))
                    m_drops[monsterid] = ret;
                else
                    m_drops.Add(monsterid, ret);

                return ret;
            }
            if (monsterid > 9300215 && monsterid < 9300269)
            {
                for (var i = 2022430; i < 2022434; i++)
                {
                    ret.Add(new DropEntry(i, 3));
                }


                if (m_drops.ContainsKey(monsterid))
                    m_drops[monsterid] = ret;
                else
                    m_drops.Add(monsterid, ret);

                return ret;
            }

            var cmd =
                new MySqlCommand(
                    "SELECT ItemId, Chance, MonsterId, QuestId FROM MonsterDrops WHERE Monsterid = @MonsterID");
            cmd.Parameters.Add(new MySqlParameter("@MonsterID", monsterid));
            MapleMonster theMonster = null;

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var rowmonsterid = (int) reader["MonsterId"];
                    var chance = (int) reader["Chance"];
                    var questid = (int) reader["Questid"];
                    if (rowmonsterid != monsterid && rowmonsterid != 0)
                    {
                        if (theMonster == null)
                        {
                            theMonster = MapleLifeFactory.GetMonster(monsterid);
                        }
                        chance += theMonster.Stats.Level*rowmonsterid;
                    }
                    ret.Add(new DropEntry((int) reader["ItemId"], chance, questid));
                }
            }

            if (m_drops.ContainsKey(monsterid))
                m_drops[monsterid] = ret;
            else
                m_drops.Add(monsterid, ret);

            return ret;
        }

        public void ClearDrops()
        {
            m_drops.Clear();
        }

        public class DropEntry
        {
            public DropEntry(int itemid, int chance, int questid)
            {
                ItemId = itemid;
                Chance = chance;
                QuestId = questid;
            }

            public DropEntry(int itemid, int chance)
            {
                ItemId = itemid;
                Chance = chance;
                QuestId = 0;
            }

            public int ItemId { get; }
            public int Chance { get; }
            public int QuestId { get; private set; }
            public int AssignedRangeStart { get; set; }
            public int AssignedRangeLength { get; set; }

            public override string ToString()
            {
                return ItemId + " chance: " + Chance;
            }
        }
    }
}