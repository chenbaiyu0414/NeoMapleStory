using NeoMapleStory.Game.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;

namespace NeoMapleStory.Game.Script.Event
{
    public class EventInstanceManager
    {
        private List<MapleCharacter> chars = new List<MapleCharacter>();
        private List<MapleMonster> mobs = new List<MapleMonster>();
        private Dictionary<MapleCharacter, int> killCount = new Dictionary<MapleCharacter, int>();
        private EventManager em;
        private MapleMapFactory mapFactory;
        public string Name { get; private set; }
        public NameValueCollection props { get; } = new NameValueCollection();
        private long timeStarted = 0;
        private long eventTime = 0;

        public EventInstanceManager(EventManager em, string name)
        {
            this.em = em;
            this.Name = name;
            mapFactory = new MapleMapFactory(MapleDataProviderFactory.GetDataProvider("Map.wz"),
                MapleDataProviderFactory.GetDataProvider("String.wz"))
            {
                ChannelId = em.ChannelServer.ChannelId
            };
        }

        //public void registerPlayer(MapleCharacter chr)
        //{
        //    if ((chr != null) && (chr.EventInstanceManager == null))
        //    {
        //        this.chars.Add(chr);
        //        chr.EventInstanceManager = this;
        //        this.em.getIv().invokeFunction("playerEntry", new object[] {this, chr});
        //    }
        //}

        //public int getInstanceId()
        //{
        //    return ChannelServer.getInstance(1).getInstanceId();
        //}

        //public void addInstanceId()
        //{
        //    ChannelServer.getInstance(1).addInstanceId();
        //}

        public void startEventTimer(long time)
        {
            timeStarted = DateTime.Now.GetTimeMilliseconds();
            eventTime = time;
        }

        public bool isTimerStarted()
        {
            return eventTime > 0 && timeStarted > 0;
        }

        public long getTimeLeft()
        {
            return eventTime - (DateTime.Now.GetTimeMilliseconds() - timeStarted);
        }

        //public void registerParty(MapleParty party, MapleMap map)
        //{
        //    foreach (
        //        var c in
        //            party.GetMembers().SelectMany(x => map.Characters.Where(m => m.Id == x.CharacterId)))
        //    {
        //        registerPlayer(c);
        //    }
        //}

        //public void registerSquad(MapleSquad squad, MapleMap map)
        //{
        //    foreach (
        //        var c in
        //           squad.Members.SelectMany(x => map.Characters.Where(m => m.Id == x.Id)))
        //    {
        //        registerPlayer(c);
        //    }
        //}

        public void unregisterPlayer(MapleCharacter chr)
        {
            this.chars.Remove(chr);
            chr.EventInstanceManager = null;
        }


        public void registerMonster(MapleMonster mob)
        {
            mobs.Add(mob);
            mob.EventInstanceManager = this;
        }

        //public void unregisterMonster(MapleMonster mob)
        //{
        //    mobs.Remove(mob);
        //    mob.EventInstanceManager = null;
        //    if (!mobs.Any())
        //    {
        //        em.getIv().invokeFunction("allMonstersDead", this);
        //    }
        //}

        //public void playerKilled(MapleCharacter chr)
        //{
        //    em.getIv().invokeFunction("playerDead", this, chr);
        //}

        //public bool revivePlayer(MapleCharacter chr)
        //{
        //    object b = em.getIv().invokeFunction("playerRevive", this, chr);
        //    if (b is bool) {
        //        return (bool)b;
        //    }
        //    return true;
        //}

        //public void playerDisconnected(MapleCharacter chr)
        //{
        //    em.getIv().invokeFunction("playerDisconnected", this, chr);
        //}

        /**
         *
         * @param chr
         * @param mob
         */
        //public void monsterKilled(MapleCharacter chr, MapleMonster mob)
        //{

        //    int kc;
        //    int inc = (double)em.getIv().invokeFunction("monsterValue", this, mob.Id);
        //    if (!killCount.TryGetValue(chr ,out kc))
        //        kc = inc;
        //    else 
        //        kc += inc;

        //    if (killCount.ContainsKey(chr))
        //        killCount[chr] = kc;
        //    else
        //        killCount.Add(chr, kc);

        //}

        //public int getKillCount(MapleCharacter chr)
        //{
        //    int kc;
        //    return killCount.TryGetValue(chr, out kc) ? 0 : kc;
        //}

        //public void dispose()
        //{
        //    chars.Clear();
        //    mobs.Clear();
        //    killCount.Clear();
        //    mapFactory = null;
        //    em.disposeInstance(Name);
        //    em = null;
        //}

        //public MapleMapFactory getMapFactory()
        //{
        //    return mapFactory;
        //}

        //public void Schedule(string methodName, int delay)
        //{
        //    TimerManager.Instance.ScheduleJob(() =>
        //    {
        //        em.getIv().invokeFunction(methodName, this);
        //    }, delay);
        //}

        public void SaveWinner(MapleCharacter chr)
        {
            //PreparedStatement ps = DatabaseConnection.getConnection().prepareStatement("INSERT INTO eventstats (event, instance, characterid, channel) VALUES (?, ?, ?, ?)");
            //ps.setString(1, em.getName());
            //ps.setString(2, getName());
            //ps.setInt(3, chr.getId());
            //ps.setInt(4, chr.getClient().getChannel());
            //ps.executeUpdate();
            //ps.close();
        }

        public MapleMap getMapInstance(int mapId)
        {
            bool wasLoaded = mapFactory.IsMapLoaded(mapId);
            MapleMap map = mapFactory.GetMap(mapId);
            if (!wasLoaded)
            {
                if (em.props.Get("shuffleReactors") != null && em.props.Get("shuffleReactors") == "true")
                {
                    //map.shuffleReactors();
                }
            }
            return map;
        }

        //public void leftParty(MapleCharacter chr)
        //{

        //    em.getIv().invokeFunction("leftParty", this, chr);

        //}

        //public void disbandParty()
        //{

        //    em.getIv().invokeFunction("disbandParty", this);

        //}

        ////Separate function to warp players to a "finish" map, if applicable
        //public void finishPQ()
        //{

        //    em.getIv().invokeFunction("clearPQ", this);

        //}

        //public void removePlayer(MapleCharacter chr)
        //{

        //    em.getIv().invokeFunction("playerExit", this, chr);

        //}

        public bool isLeader(MapleCharacter chr)
        {
            return (chr.Party.Leader.CharacterId == chr.Id);
        }

        //public void saveAllBossQuestPoints(int bossPoints)
        //{
        //    for (MapleCharacter character : chars)
        //    {
        //        int points = character.getBossPoints();
        //        character.setBossPoints(points + bossPoints);
        //    }
        //}

        //public void saveBossQuestPoints(int bossPoints, MapleCharacter character)
        //{
        //    int points = character.getBossPoints();
        //    character.setBossPoints(points + bossPoints);
        //}
    }
}
