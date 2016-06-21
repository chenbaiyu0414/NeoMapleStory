using System.Collections.Generic;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Game.Mob;
using System.Linq;

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

            
            MapleMonster theMonster = null;

            using (var db = new NeoMapleStoryDatabase())
            {
                var dropQuery = db.MonsterDrops.Where(x => x.MonsterId == monsterid).Select(x => x);

                foreach (var dropInfo in dropQuery)
                {
                    var rowmonsterid = dropInfo.MonsterId;
                    var chance = dropInfo.Chance;
                    var questid = dropInfo.QuestId;
                    if (rowmonsterid != monsterid && rowmonsterid != 0)
                    {
                        if (theMonster == null)
                        {
                            theMonster = MapleLifeFactory.GetMonster(monsterid);
                        }
                        chance += theMonster.Stats.Level*rowmonsterid;
                    }
                    ret.Add(new DropEntry(dropInfo.ItemId, chance, questid));
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