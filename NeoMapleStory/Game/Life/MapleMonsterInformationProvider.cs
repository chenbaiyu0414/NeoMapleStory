using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Core;

namespace NeoMapleStory.Game.Life
{
    public class MapleMonsterInformationProvider
    {
        public class DropEntry
        {
            public int ItemId { get; private set; }
            public int Chance { get; private set; }
            public int QuestId { get; private set; }
            public int AssignedRangeStart { get; set; }
            public int AssignedRangeLength { get; set; }

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

            public override string ToString()
            {
                return ItemId + " chance: " + Chance;
            }
        }


        public const int APPROX_FADE_DELAY = 90;

        public static MapleMonsterInformationProvider Instance { get; private set; } = new MapleMonsterInformationProvider();
        private Dictionary<int, List<DropEntry>> drops = new Dictionary<int, List<DropEntry>>();


        public List<DropEntry> RetrieveDropChances(int monsterid)
        {
            if (drops.ContainsKey(monsterid))
            {
                return drops[monsterid];
            }

            List<DropEntry> ret = new List<DropEntry>();
            if (monsterid > 9300183 && monsterid < 9300216)
            {
                for (int i = 2022359; i < 2022367; i++)
                {
                    ret.Add(new DropEntry(i, 10));
                }

                if (drops.ContainsKey(monsterid))
                    drops[monsterid] = ret;
                else
                    drops.Add(monsterid, ret);

                return ret;
            }
            if (monsterid > 9300215 && monsterid < 9300269)
            {
                for (int i = 2022430; i < 2022434; i++)
                {
                    ret.Add(new DropEntry(i, 3));
                }


                if (drops.ContainsKey(monsterid))
                    drops[monsterid] = ret;
                else
                    drops.Add(monsterid, ret);

                return ret;
            }

            MySqlCommand cmd = new MySqlCommand("SELECT ItemId, Chance, MonsterId, QuestId FROM MonsterDrops WHERE Monsterid = @MonsterID");
            cmd.Parameters.Add(new MySqlParameter("@MonsterID", monsterid));
            MapleMonster theMonster = null;

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int rowmonsterid = (int)reader["MonsterId"];
                    int chance = (int)reader["Chance"];
                    int questid = (int)reader["Questid"];
                    if (rowmonsterid != monsterid && rowmonsterid != 0)
                    {
                        if (theMonster == null)
                        {
                            theMonster = MapleLifeFactory.GetMonster(monsterid);
                        }
                        chance += theMonster.Stats.Level * rowmonsterid;
                    }
                    ret.Add(new DropEntry((int)reader["ItemId"], chance, questid));
                }
            }

            if (drops.ContainsKey(monsterid))
                drops[monsterid] = ret;
            else
                drops.Add(monsterid, ret);

            return ret;
        }

        public void ClearDrops()
        {
            drops.Clear();
        }
    }
}
