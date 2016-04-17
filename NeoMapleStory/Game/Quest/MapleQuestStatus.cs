using NeoMapleStory.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Quest
{
    public enum MapleQuestStatusType
    {

        Undefined = -1,
        NotStarted,
        Started,
        Completed
    }

     public class MapleQuestStatus
    {
        public MapleQuest Quest { get; private set; }
        public MapleQuestStatusType Status { get; set; }
        public Dictionary<int, int> KilledMobs { get; private set; } = new Dictionary<int, int>();
        public int Npcid { get; set; }
        public long CompletionTime { get; set; }

        public int Forfeited
        {
            get { return Forfeited; }
            set
            {
                if (value >= Forfeited)
                    Forfeited = value;
                else
                    throw new Exception("无法设置比旧值低的forfeis");
            }
        }


        public MapleQuestStatus(MapleQuest quest, MapleQuestStatusType status)
        {
            Quest = quest;
            Status = status;
            CompletionTime = DateTime.Now.GetTimeMilliseconds();
            if (status == MapleQuestStatusType.Started)
            {
                RegisterMobs();
            }
        }

        public MapleQuestStatus(MapleQuest quest, MapleQuestStatusType status, int npcId)
        {
            Quest = quest;
            Status = status;
            Npcid = npcId;
            CompletionTime = DateTime.Now.GetTimeMilliseconds();
            if (status == MapleQuestStatusType.Started)
            {
                RegisterMobs();
            }
        }

        private void RegisterMobs()
        {
            List<int> relevants = Quest.GetRelevantMobs();
            foreach (int i in relevants)
                KilledMobs.Add(i, 0);
        }

        public bool MobKilled(int id)
        {
            int value;
            if (KilledMobs.TryGetValue(id,out value))
            {
                KilledMobs.Add(id, value + 1);
                return true;
            }
            return false;
        }

        public void SetMobKills(int id, int count) => KilledMobs.Add(id, count);


        public bool HasMobKills() => KilledMobs.Any();


        public int GetMobKills(int id) => KilledMobs.ContainsKey(id) ? KilledMobs[id] : 0;


        public Dictionary<int,int> GetMobKills() => KilledMobs;


        public int GetMobNum(int id) => KilledMobs.Values.ElementAtOrDefault(id);

    }
}
