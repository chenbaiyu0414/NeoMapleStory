using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using Quartz;

namespace NeoMapleStory.Game.Map
{
    public enum MapleMapObjectType
    {
        Npc,
        Monster,
        Item,
        Player,
        Door,
        Summon,
        Shop,
        Mist,
        Reactor,
        HiredMerchant,
        Love
    }


    public class MapleMap
    {
        private static readonly int MaxOid = 20000;

        private static readonly List<MapleMapObjectType> RangedMapobjectTypes = new List<MapleMapObjectType>
        {
            MapleMapObjectType.Item,
            MapleMapObjectType.Monster,
            MapleMapObjectType.Door,
            MapleMapObjectType.Summon,
            MapleMapObjectType.Reactor
        };

        private readonly int m_channel;

        private readonly MapleMapEffect m_mapEffect = null;


        private readonly MapleMapTimer m_mapTimer = null;
        private readonly float m_monsterRate;

        /**
         * Holds a mapping of all oid -> MapleMapObject on this map. mapobjects is NOT a lock collection since it
         * has to be lock together with runningOid that's why all access to mapobjects have to be done trough an
         * explicit lock block
         */

        private readonly List<SpawnPoint> m_monsterSpawn = new List<SpawnPoint>();

        private bool? m_boat;

        public bool CannotInvincible { get; set; } = false;
        private bool m_canVipRock = true;


        private bool m_docked;
        private readonly int m_dropLife = 180000; // 以毫秒为单位掉落后消失的时间
        private bool m_dropsDisabled = false;

        private bool m_hasEvent;
        private float m_origMobRate;

        private int m_runningOid = 100;
        //private MapleOxQuiz ox = null;
        private TriggerKey m_spawnWorker;

        public MapleMap(int mapid, int channel, int returnMapId, float monsterRate)
        {
            MapId = mapid;
            m_channel = channel;
            ReturnMapId = returnMapId;
            m_monsterRate = monsterRate;
            m_origMobRate = monsterRate;

            if (monsterRate > 0)
            {
                m_spawnWorker = TimerManager.Instance.RepeatTask(RespawnWorker, 7*1000);
            }
        }

        public int MapId { get; set; }


        public Dictionary<int, IMapleMapObject> Mapobjects { get; } =
            new Dictionary<int, IMapleMapObject>();

        public List<MapleCharacter> Characters { get; set; } = new List<MapleCharacter>();

        public Dictionary<int, IMaplePortal> Portals { get; } = new Dictionary<int, IMaplePortal>();

        public List<Rectangle> Areas { get; set; } = new List<Rectangle>();

        public MapleFootholdTree Footholds { get; set; } = null;

        public int ReturnMapId { get; set; }

        public MapleMap ReturnMap => MasterServer.Instance.ChannelServers[m_channel].MapFactory.GetMap(ReturnMapId);

        public bool Clock { get; set; }

        public bool? HasBoat
        {
            get
            {
                if (m_boat.Value && m_docked)
                    return true;
                if (m_boat.Value)
                    return false;
                return null;
            }
            set { m_boat = value; }
        }

        public string MapName { get; set; }

        public string StreetName { get; set; }

        public bool Everlast { get; set; } = false;

        public int ForcedReturnMapId { get; set; } = 999999999;

        public MapleMap ForcedReturnMap
        {
            get { return MasterServer.Instance.ChannelServers[m_channel].MapFactory.GetMap(ForcedReturnMapId); }
            set { ForcedReturnMap = value; }
        }

        public int TimeLimit { get; set; }

        public int DecHp { get; set; } = 0;

        public int ProtectItem { get; set; } = 0;

        public bool Town { get; set; }

        public bool AllowShops { get; set; }

        public bool Muted { get; set; }

        public bool Lootable { get; set; } = true;

        public bool CanEnter { get; set; } = true;

        public bool CanExit { get; set; } = true;

        public int FieldLimit { get; set; } = 0;

        public int FieldType { get; set; }

        public string OnUserEnter { get; set; }

        public string OnFirstUserEnter { get; set; }

        public int TimeMobId { get; set; }

        public string TimeMobMessage { get; set; } = "";

        public int MaxRegularSpawn => (int) (m_monsterSpawn.Count/m_monsterRate);

        public bool HasEvent { get; set; }

        public bool IsLootable { get; set; } = true;
        public InterLockedInt SpawnedMonstersOnMap { get; } = new InterLockedInt(0);

        public IMaplePortal FindClosestSpawnpoint(Point from)
        {
            IMaplePortal closest = null;
            var shortestDistance = double.PositiveInfinity;
            foreach (var portal in Portals.Values)
            {
                var distance = portal.Position.DistanceSquare(from);
                if (portal.Type == PortalType.MapPortal && distance < shortestDistance &&
                    portal.TargetMapId == 999999999)
                {
                    closest = portal;
                    shortestDistance = distance;
                }
            }
            return closest;
        }


        public void AddMapObject(IMapleMapObject mapobject)
        {
            lock (Mapobjects)
            {
                mapobject.ObjectId = m_runningOid;
                if (Mapobjects.ContainsKey(m_runningOid))
                    Mapobjects[m_runningOid] = mapobject;
                else
                    Mapobjects.Add(m_runningOid, mapobject);

                IncrementRunningOid();
            }
        }

        private void IncrementRunningOid()
        {
            m_runningOid++;
            for (var numIncrements = 1; numIncrements < MaxOid; numIncrements++)
            {
                if (m_runningOid > MaxOid)
                {
                    m_runningOid = 100;
                }
                if (Mapobjects.ContainsKey(m_runningOid))
                {
                    m_runningOid++;
                }
                else
                {
                    return;
                }
            }
            throw new Exception($"Out of OIDs on map {MapId} (channel: {m_channel})");
        }

        public void RespawnWorker()
        {
            if (!Characters.Any())
            {
                return;
            }

            var numShouldSpawn = MaxRegularSpawn - SpawnedMonstersOnMap.Value;
            if (numShouldSpawn > 0)
            {
                var randomSpawn = new List<SpawnPoint>(m_monsterSpawn);
                randomSpawn.Shuffle();
                var spawned = 0;
                foreach (var spawnPoint in randomSpawn)
                {
                    if (spawnPoint.ShouldSpawn())
                    {
                        spawnPoint.SpawnMonster(this);
                        spawned++;
                    }
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }

        public void AddMonsterSpawn(MapleMonster monster, int mobTime)
        {
            var newpos = CalcPointBelow(monster.Position);
            newpos.Y -= 1;
            var sp = new SpawnPoint(monster, newpos, mobTime);
            m_monsterSpawn.Add(sp);
        }

        private Point CalcPointBelow(Point initial)
        {
            var fh = Footholds.FindBelow(initial);
            if (fh == null)
            {
                return Point.Empty;
            }
            var dropY = fh.Point1.Y;
            if (!fh.IsWall() && fh.Point1.Y != fh.Point2.Y)
            {
                double s1 = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                double s2 = Math.Abs(fh.Point2.X - fh.Point1.X);
                double s4 = Math.Abs(initial.X - fh.Point1.X);
                var alpha = Math.Atan(s2/s1);
                var beta = Math.Atan(s1/s2);
                var s5 = Math.Cos(alpha)*(s4/Math.Cos(beta));
                if (fh.Point2.Y < fh.Point1.Y)
                {
                    dropY = fh.Point1.Y - (int) s5;
                }
                else
                {
                    dropY = fh.Point1.Y + (int) s5;
                }
            }
            return new Point(initial.X, dropY);
        }

        public bool IsPqMap()
        {
            //Does NOT include CPQ maps
            var tmapid = MapId;
            if ((tmapid > 922010000 && tmapid < 922011100) || (tmapid >= 103000800 && tmapid < 103000890))
            {
                //kpq + lpq only atm
                return true;
            }
            return false;
        }


        public void SpawnMonster(MapleMonster monster)
        {
            if (!Characters.Any() && !IsPqMap())
            {
                return;
            }
            monster.Map = this;
            var removeAfter = monster.Stats.RemoveAfter;
            if (removeAfter > 0)
            {
                TimerManager.Instance.RunOnceTask(() => { KillMonster(monster, Characters[0], false, false, 3); },
                    removeAfter);
            }
            lock (Mapobjects)
            {
                SpawnAndAddRangedMapObject(monster, mc => { mc.Send(PacketCreator.SpawnMonster(monster, true)); }, null);

                UpdateMonsterController(monster);
            }
            SpawnedMonstersOnMap.Increment();
        }

        public List<IMapleMapObject> GetAllPlayers()
        {
            return GetMapObjectsInRange(new Point(0, 0), double.PositiveInfinity,
                new List<MapleMapObjectType> {MapleMapObjectType.Player});
        }

        public bool DamageMonster(MapleCharacter chr, MapleMonster monster, int damage)
        {
            if (monster.Id == 8500000 || monster.Id == 8800000)
            {
                //SpeedRankings.setStartTime(monster.Id == 8500000 ? 1 : 0, monster.Id, DateTime.Now.GetTimeMilliseconds());
            }
            if (monster.Id == 8800000)
            {
                var objects = chr.Map.Mapobjects.Values;
                foreach (var mapobject in objects)
                {
                    var mons = chr.Map.GetMonsterByOid(mapobject.ObjectId);
                    if (mons != null && mons.Id >= 8800003 && mons.Id <= 8800010)
                    {
                        return true;
                    }
                }
            }

            if (monster.IsAlive)
            {
                lock (monster)
                {
                    if (!monster.IsAlive)
                    {
                        return false;
                    }
                    if (damage > 0)
                    {
                        var monsterhp = monster.Hp;
                        monster.Damage(chr, damage, true);
                        if (!monster.IsAlive)
                        {
                            // monster just died
                            KillMonster(monster, chr, true);
                            if (monster.Id >= 8810002 && monster.Id <= 8810009)
                            {
                                foreach (var mapobject in chr.Map.Mapobjects.Values)
                                {
                                    var mons = chr.Map.GetMonsterByOid(mapobject.ObjectId);
                                    if (mons != null)
                                    {
                                        if (mons.Id == 8810018 || mons.Id == 8810026)
                                        {
                                            DamageMonster(chr, mons, monsterhp);
                                        }
                                    }
                                }
                            }
                            else if ((monster.Id >= 8820002 && monster.Id <= 8820006) ||
                                     (monster.Id >= 8820015 && monster.Id <= 8820018))
                            {
                                foreach (var mapobject in chr.Map.Mapobjects.Values)
                                {
                                    var mons = chr.Map.GetMonsterByOid(mapobject.ObjectId);
                                    if (mons?.Id >= 8820010 && mons.Id <= 8820014)
                                    {
                                        DamageMonster(chr, mons, monsterhp);
                                    }
                                }
                            }
                        }
                        else if (monster.Id >= 8810002 && monster.Id <= 8810009)
                        {
                            foreach (var mapobject in chr.Map.Mapobjects.Values)
                            {
                                var mons = chr.Map.GetMonsterByOid(mapobject.ObjectId);
                                if (mons != null)
                                {
                                    if (mons.Id == 8810018 || mons.Id == 8810026)
                                    {
                                        DamageMonster(chr, mons, damage);
                                    }
                                }
                            }
                        }
                        else if ((monster.Id >= 8820002 && monster.Id <= 8820006) || (monster.Id >= 8820015 && monster.Id <= 8820018))
                        {
                            foreach (var mapobject in chr.Map.Mapobjects.Values)
                            {
                                var mons = chr.Map.GetMonsterByOid(mapobject.ObjectId);
                                if (mons?.Id >= 8820010 && mons.Id <= 8820014)
                                {
                                    DamageMonster(chr, mons, damage);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void KillMonster(MapleMonster monster, MapleCharacter chr, bool withDrops)
        {
            KillMonster(monster, chr, withDrops, false, 1);
        }

        public void KillMonster(MapleMonster monster, MapleCharacter chr, bool withDrops, bool secondTime)
        {
            KillMonster(monster, chr, withDrops, secondTime, 1);
        }

        public void KillMonster(int monsId)
        {
            foreach (var mmo in Mapobjects.Values)
            {
                var monster = mmo as MapleMonster;
                if (monster?.Id == monsId)
                {
                    var playerObjects = GetAllPlayers();
                    KillMonster(monster, (MapleCharacter) playerObjects[0], false);
                }
            }
        }

        public void KillMonster(MapleMonster monster, MapleCharacter chr, bool withDrops, bool secondTime, int animation)
        {
            if (chr.AntiCheatTracker.CheckHpLoss())
            {
                chr.AntiCheatTracker.RegisterOffense(CheatingOffense.AttackWithoutGettingHit);
            }
            var names = new StringBuilder();
            if (monster.Id == 8500002 || monster.Id == 8800002)
            {
                if (chr.Party != null)
                {
                    var party = chr.Party;
                    var partymems = party.GetPartyMembers();
                    foreach (var t in partymems)
                    {
                        names.Append(t.Name);
                        names.Append(", ");
                    }
                }
                else
                {
                    names.Append(chr.Name);
                }
                var rankType = monster.Id == 8500002 ? 1 : 0;
                var oid = monster.ObjectId;
                //SpeedRankings.setEndTime(rankType, oid, DateTime.Now.GetTimeMilliseconds());
                //SpeedRankings.insertRankingToSQL(rankType, names.toString(), SpeedRankings.calcTime(rankType, oid));
            }
            if (monster.Id == 8810018 && !secondTime)
            {
                TimerManager.Instance.RunOnceTask(() =>
                {
                    KillMonster(monster, chr, withDrops, true, 1);
                    //killAllMonsters();
                }, 3000);
                return;
            }
            //            if (monster.getbufftogive > -1) {
            //            broadcastMessage(MaplePacketCreator.showOwnBuffEffect(monster.getBuffToGive(), 11));
            //                MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
            //MapleStatEffect statEffect = mii.getItemEffect(monster.getBuffToGive());
            //            lock(this.Characters)
            //{
            //    foreach (MapleCharacter character in this.Characters)
            //    {
            //        if (character.IsAlive)
            //        {
            //            statEffect.applyTo(character);
            //            broadcastMessage(MaplePacketCreator.showBuffeffect(character.getId(), monster.getBuffToGive(), 11, (byte)3));
            //        }
            //    }
            //}
            //}
            //        if (monster.Id == 8810018) {
            //            foreach (MapleCharacter c in this.getCharacters()) {
            //    c.finishAchievement(26);
            //}
            //}
            //        if (chr.getMapId() >= 925020010 && chr.getMapId() <= 925033804) {
            //            for (MapleCharacter c : this.getCharacters()) {
            //    c.DoJoKill();
            //}
            //}
            SpawnedMonstersOnMap.Decrement();
            monster.Hp = 0;
            BroadcastMessage(PacketCreator.KillMonster(monster.ObjectId, true), monster.Position);
            RemoveMapObject(monster);

            if (monster.Id >= 8800003 && monster.Id <= 8800010)
            {
                var makeZakReal = true;
                foreach (var mobj in Mapobjects.Values)
                {
                    var mons = GetMonsterByOid(mobj.ObjectId);
                    if (mons?.Id >= 8800003 && mons.Id <= 8800010)
                    {
                        makeZakReal = false;
                    }
                }
                if (makeZakReal)
                {
                    foreach (var mobj in Mapobjects.Values)
                    {
                        var mons = chr.Map.GetMonsterByOid(mobj.ObjectId);
                        if (mons?.Id == 8800000)
                        {
                            //makeMonsterReal(mons);
                            //updateMonsterController(mons);
                        }
                    }
                }
            }
            var dropOwner = monster.KillBy(chr);
            if (withDrops /*&& !monster.dropdisabled*/)
            {
                if (dropOwner == null)
                {
                    dropOwner = chr;
                }
                DropFromMonster(dropOwner, monster);
            }
        }

        private void DropFromMonster(MapleCharacter dropOwner, MapleMonster monster)
        {
            //if (dropsDisabled || monster.dropsDisabled())
            //{
            //    return;
            //}

            var ii = MapleItemInformationProvider.Instance;

            var maxDrops = monster.GetMaxDrops(dropOwner);
            var explosive = monster.Stats.IsExplosive;

            var toDrop = new List<int>();

            for (var i = 0; i < maxDrops; i++)
            {
                toDrop.Add(monster.GetDrop(dropOwner));
            }

            if (dropOwner.EventInstanceManager == null)
            {
                var chance = (int) (Randomizer.NextDouble()*100);
                if (chance < 20)
                {
                    //20% chance of getting a maple leaf
                    toDrop.Add(4001126);
                }
            }

            if (monster.Id == 8810018)
            {
                toDrop.Add(2290096); //force add one MW per HT
            }

            var alreadyDropped = new List<int>();
            var htpendants = 0;
            var htstones = 0;
            for (var i = 0; i < toDrop.Count; i++)
            {
                if (toDrop[i] == 1122000)
                {
                    if (htpendants > 3)
                    {
                        toDrop[i] = -1;
                    }
                    else
                    {
                        htpendants++;
                    }
                }
                else if (toDrop[i] == 4001094)
                {
                    if (htstones > 2)
                    {
                        toDrop[i] = 1;
                    }
                    else
                    {
                        htstones++;
                    }
                }
                else if (alreadyDropped.Contains(toDrop[i]) && !explosive)
                {
                    toDrop.RemoveAt(i);
                    i--;
                }
                else
                {
                    alreadyDropped.Add(toDrop[i]);
                }
            }

            if (toDrop.Count > maxDrops)
            {
                toDrop = toDrop.Take(maxDrops).ToList();
            }

            var toPoint = new Point[toDrop.Count];
            var shiftDirection = 0;
            var shiftCount = 0;

            var curX = Math.Min(Math.Max(monster.Position.X - 25*(toDrop.Count/2), Footholds.MinDropX + 25),
                Footholds.MaxDropX - toDrop.Count*25);
            var curY = Math.Max(monster.Position.Y, Footholds.Point1.Y);

            while (shiftDirection < 3 && shiftCount < 1000)
            {
                if (shiftDirection == 1)
                {
                    curX += 25;
                }
                else if (shiftDirection == 2)
                {
                    curX -= 25;
                }
                for (var i = 0; i < toDrop.Count; i++)
                {
                    var wall = Footholds.FindWall(new Point(curX, curY),
                        new Point(curX + toDrop.Count*25, curY));
                    if (wall != null)
                    {
                        if (wall.Point1.X < curX)
                        {
                            shiftDirection = 1;
                            shiftCount++;
                            break;
                        }
                        if (wall.Point1.X == curX)
                        {
                            if (shiftDirection == 0)
                            {
                                shiftDirection = 1;
                            }
                            shiftCount++;
                            break;
                        }

                        shiftDirection = 2;
                        shiftCount++;
                        break;
                    }
                    if (i == toDrop.Count - 1)
                    {
                        shiftDirection = 3;
                    }
                    var dropPos = CalcDropPos(new Point(curX + i*25, curY), monster.Position);
                    toPoint[i] = new Point(curX + i*25, curY);
                    var drop = toDrop[i];

                    if (drop == -1)
                    {
                        var mesoRate = dropOwner.Client.ChannelServer.MesoRate;
                        var mesoDecrease = Math.Pow(0.93, monster.Stats.Exp/300.0);
                        if (mesoDecrease > 1.0)
                        {
                            mesoDecrease = 1.0;
                        }
                        var tempmeso = Math.Min(30000,
                            (int) (mesoDecrease*monster.Stats.Exp*(1.0 + Randomizer.NextDouble()*20)/10.0));
                        if (dropOwner.GetBuffedValue(MapleBuffStat.Mesoup) != null)
                        {
                            var buffedValue = dropOwner.GetBuffedValue(MapleBuffStat.Mesoup);
                            if (buffedValue != null)
                                tempmeso = (int) ((double) tempmeso*buffedValue.Value/100.0);
                        }

                        var meso = tempmeso;

                        if (meso > 0)
                        {
                            var dropMonster = monster;
                            var dropChar = dropOwner;
                            TimerManager.Instance.RunOnceTask(
                                () => { SpawnMesoDrop(meso*mesoRate, dropPos, dropMonster, dropChar, explosive); },
                                monster.Stats.GetAnimationTime("die1"));
                        }
                    }
                    else
                    {
                        IMapleItem idrop;
                        var type = ii.GetInventoryType(drop);
                        if (type == MapleInventoryType.Equip)
                        {
                            var nEquip = ii.RandomizeStats((Equip) ii.GetEquipById(drop));
                            idrop = nEquip;
                        }
                        else
                        {
                            idrop = new Item(drop, 0, 1);
                            if (ii.IsArrowForBow(drop) || ii.IsArrowForCrossBow(drop))
                            {
                                // Randomize quantity for certain items
                                idrop.Quantity = (short) (1 + 100*Randomizer.NextDouble());
                            }
                            else if (ii.IsThrowingStar(drop) || ii.IsBullet(drop))
                            {
                                idrop.Quantity = 1;
                            }
                        }

                        //Console.WriteLine(
                        //    "Created as a drop from monster " + monster.ObjectId + " (" + monster.Id + ") at " +
                        //    dropPos.ToString() + " on map " + MapId);

                        var mdrop = new MapleMapItem(idrop, dropPos, monster, dropOwner);
                        IMapleMapObject dropMonster = monster;
                        var dropChar = dropOwner;
                        TimerManager.Instance.RunOnceTask(() =>
                        {
                            SpawnAndAddRangedMapObject(mdrop, mc =>
                            {
                                mc.Send(PacketCreator.DropItemFromMapObject(drop,
                                    mdrop.ObjectId,
                                    dropMonster.ObjectId,
                                    explosive ? 0 : dropChar.Id,
                                    dropMonster.Position,
                                    dropPos,
                                    1));
                                ActivateItemReactors(mdrop);
                            }, null);

                            TimerManager.Instance.RunOnceTask(() => ExpireMapItemJob(mdrop), m_dropLife);
                        }, monster.Stats.GetAnimationTime("die1"));
                    }
                }
            }
        }

        private void ActivateItemReactors(MapleMapItem drop)
        {
            var item = drop.Item;
            var tMan = TimerManager.Instance; //check for reactors on map that might use this item
            foreach (var o in Mapobjects.Values)
            {
                if (o.GetType() == MapleMapObjectType.Reactor)
                {
                    var reactor = o as MapleReactor;
                    if (reactor == null)
                        continue;

                    if (reactor.ReactorType == 100)
                    {
                        if (reactor.ReactorItem.Item1 == item.ItemId && reactor.ReactorItem.Item2 <= item.Quantity)
                        {
                            var area = reactor.Area;

                            if (area.Contains(drop.Position))
                            {
                                MapleClient ownerClient = null;
                                if (drop.Owner != null)
                                {
                                    ownerClient = drop.Owner.Client;
                                }

                                if (!reactor.IsTimerActive)
                                {
                                    tMan.RunOnceTask(() => ActivateItemReactor(drop, reactor, ownerClient), 5000);
                                    reactor.IsTimerActive = true;
                                }
                            }
                        }
                    }
                }
            }
        }


        public MapleMonster GetMonsterByOid(int oid)
        {
            return
                Mapobjects.Values.FirstOrDefault(x => x.ObjectId == oid && x.GetType() == MapleMapObjectType.Monster) as
                    MapleMonster;
        }

        public MapleMonster GetMonsterById(int id)
        {
            lock (Mapobjects)
            {
                foreach (var obj in Mapobjects.Values)
                {
                    if (obj.GetType() != MapleMapObjectType.Monster) continue;
                    if (((MapleMonster) obj).Id == id)
                    {
                        return (MapleMonster) obj;
                    }
                }
            }
            return null;
        }

        public bool ContainsNpc(int npcid)
        {
            lock (Mapobjects)
            {
                return
                    Mapobjects.Values.Where(obj => obj.GetType() == MapleMapObjectType.Npc)
                        .Any(obj => ((MapleNpc) obj).Id == npcid);
            }
        }

        private bool IsNonRangedType(MapleMapObjectType type)
        {
            switch (type)
            {
                case MapleMapObjectType.Npc:
                case MapleMapObjectType.Player:
                case MapleMapObjectType.Mist:
                case MapleMapObjectType.HiredMerchant:
                case MapleMapObjectType.Love:
                    return true;
            }
            return false;
        }

        public void MoveMonster(MapleMonster monster, Point reportedPos)
        {
            monster.Position = reportedPos;
            lock (Characters)
            {
                foreach (var chr in Characters)
                {
                    UpdateMapObjectVisibility(chr, monster);
                }
            }
        }

        public void UpdateMonsterController(MapleMonster monster)
        {
            if (!monster.IsAlive)
            {
                return;
            }
            lock (monster)
            {
                if (!monster.IsAlive)
                {
                    return;
                }

                if (monster.GetController() != null)
                {
                    // monster has a controller already, check if he's still on this map
                    if (!monster.GetController().Map.Equals(this))
                    {
                        Console.WriteLine("Monstercontroller wasn't on same map");
                        monster.GetController().StopControllingMonster(monster);
                    }
                    else
                    {
                        // controller is on the map, monster has an controller, everything is fine
                        return;
                    }
                }

                var mincontrolled = -1;
                MapleCharacter newController = null;
                lock (Characters)
                {
                    foreach (var chr in Characters)
                    {
                        if (!chr.IsHidden && (chr.GetControlledMonsters().Count < mincontrolled || mincontrolled == -1))
                        {
                            mincontrolled = chr.GetControlledMonsters().Count;
                            newController = chr;
                        }
                    }
                }
                if (newController != null)
                {
                    // was a new controller found? (if not no one is on the map)
                    if (monster.Stats.IsFirstAttack)
                    {
                        newController.ControlMonster(monster, true);
                        monster.ControllerHasAggro = true;
                        monster.ControllerKnowsAboutAggro = true;
                    }
                    else
                    {
                        newController.ControlMonster(monster, false);
                    }
                }
            }
        }

        public List<IMapleMapObject> GetMapObjectsInRange(Point from, double rangeSq, List<MapleMapObjectType> types)
        {
            var ret = new List<IMapleMapObject>();
            lock (Mapobjects)
            {
                Mapobjects.Values.ToList().ForEach(obj =>
                {
                    if (types.Contains(obj.GetType()) && from.DistanceSquare(obj.Position) <= rangeSq)
                    {
                        ret.Add(obj);
                    }
                });
            }
            return ret;
        }

        private void SendObjectPlacement(MapleClient mapleClient)
        {
            foreach (var o in Mapobjects.Values)
            {
                if (IsNonRangedType(o.GetType()))
                {
                    o.SendSpawnData(mapleClient);
                }
                else if (o.GetType() == MapleMapObjectType.Monster)
                {
                    UpdateMonsterController((MapleMonster) o);
                }
            }
            var chr = mapleClient.Player;

            if (chr != null)
            {
                foreach (
                    var o in GetMapObjectsInRange(chr.Position, MapleCharacter.MaxViewRangeSq, RangedMapobjectTypes))
                {
                    if (o.GetType() == MapleMapObjectType.Reactor)
                    {
                        if (((MapleReactor) o).IsAlive)
                        {
                            o.SendSpawnData(chr.Client);
                            chr.VisibleMapObjects.Add(o);
                        }
                    }
                    else
                    {
                        o.SendSpawnData(chr.Client);
                        chr.VisibleMapObjects.Add(o);
                    }
                }
            }
            else
            {
                Console.WriteLine("sendObjectPlacement invoked with null char");
            }
        }

        private bool HasForcedEquip()
        {
            return FieldType == 81 || FieldType == 82;
        }

        private void SpawnAndAddRangedMapObject(IMapleMapObject mapobject, Action<MapleClient> packetbakery,
            ISpawnCondition condition)
        {
            lock (Mapobjects)
            {
                mapobject.ObjectId = m_runningOid;

                lock (Characters)
                {
                    foreach (var chr in Characters
                        .Where(chr => condition == null || condition.CanSpawn(chr))
                        .Where(chr => chr.Position.DistanceSquare(mapobject.Position) <= MapleCharacter.MaxViewRangeSq)
                        )
                    {
                        packetbakery(chr.Client);
                        chr.VisibleMapObjects.Add(mapobject);
                    }
                }

                if (Mapobjects.ContainsKey(m_runningOid))
                    Mapobjects[m_runningOid] = mapobject;
                else
                    Mapobjects.Add(m_runningOid, mapobject);

                IncrementRunningOid();
            }
        }

        public void SpawnSummon(MapleSummon summon)
        {
            SpawnAndAddRangedMapObject(summon, client =>
            {
                var skillLevel = summon.Owner.GetSkillLevel(SkillFactory.GetSkill(summon.SkillId));
                client.Send(PacketCreator.SpawnSpecialMapObject(summon, skillLevel, true));
            }, null);
        }

        private void UpdateMapObjectVisibility(MapleCharacter chr, IMapleMapObject mo)
        {
            if (!chr.VisibleMapObjects.Contains(mo))
            {
                // monster entered view range
                if (mo.GetType() == MapleMapObjectType.Summon ||
                    mo.Position.DistanceSquare(chr.Position) <= MapleCharacter.MaxViewRangeSq)
                {
                    chr.VisibleMapObjects.Add(mo);
                    mo.SendSpawnData(chr.Client);
                }
            }
            else
            {
                // monster left view range
                if (mo.GetType() != MapleMapObjectType.Summon &&
                    mo.Position.DistanceSquare(chr.Position) > MapleCharacter.MaxViewRangeSq)
                {
                    chr.VisibleMapObjects.Remove(mo);
                    mo.SendDestroyData(chr.Client);
                }
            }
        }

        public void SpawnMesoDrop(int meso, Point position, IMapleMapObject dropper, MapleCharacter owner, bool ffaLoot)
        {
            var droppos = CalcDropPos(position, position);
            var mdrop = new MapleMapItem(meso, droppos, dropper, owner);
            SpawnAndAddRangedMapObject(mdrop, client =>
            {
                client.Send(PacketCreator.DropMesoFromMapObject(meso, mdrop.ObjectId, dropper.ObjectId,
                    ffaLoot ? 0 : owner.Id, dropper.Position, droppos, 1));
            }, null);
            TimerManager.Instance.RunOnceTask(() => ExpireMapItemJob(mdrop), m_dropLife);
        }

        private Point CalcDropPos(Point initial, Point fallback)
        {
            var ret = CalcPointBelow(new Point(initial.X, initial.Y - 99));
            return ret == Point.Empty ? fallback : ret;
        }

        public void AddPlayer(MapleCharacter chr)
        {
            lock (Characters)
            {
                Characters.Add(chr);
            }
            lock (Mapobjects)
            {
                if (!chr.IsHidden)
                {
                    BroadcastMessage(chr, PacketCreator.SpawnPlayerMapobject(chr), false);
                    foreach (var pet in chr.Pets)
                    {
                        BroadcastMessage(chr, PacketCreator.ShowPet(chr, pet, false, false), false);
                    }
                }

                SendObjectPlacement(chr.Client);
                chr.Client.Send(PacketCreator.SpawnPlayerMapobject(chr));

                foreach (var pet in chr.Pets)
                {
                    chr.Client.Send(PacketCreator.ShowPet(chr, pet, false, false));
                }

                if (HasForcedEquip())
                {
                    chr.Client.Send(PacketCreator.ShowForcedEquip());
                }

                chr.Client.Send(PacketCreator.RemoveTutorialStats());

                if (chr.Map.MapId >= 914000200 && chr.Map.MapId <= 914000220)
                {
                    chr.Client.Send(PacketCreator.AddTutorialStats());
                }
                if (chr.Map.MapId >= 140090100 && chr.Map.MapId <= 140090500 ||
                    chr.Job.JobId == 1000 && chr.Map.MapId != 130030000)
                {
                    chr.Client.Send(PacketCreator.SpawnTutorialSummon(1));
                }

                if (Mapobjects.ContainsKey(chr.ObjectId))
                    Mapobjects[chr.ObjectId] = chr;
                else
                    Mapobjects.Add(chr.ObjectId, chr);
            }
            if (!OnUserEnter.Equals(""))
            {
                //MapScriptManager.getInstance().getMapScript(chr.Client, onUserEnter, false);
            }
            if (!OnFirstUserEnter.Equals(""))
            {
                if (Characters.Count == 1)
                {
                    //MapScriptManager.getInstance().getMapScript(chr.Client, onFirstUserEnter, true);
                }
            }

            var summonStat = chr.GetStatForBuff(MapleBuffStat.Summon);
            if (summonStat != null)
            {
                var summon = chr.Summons[summonStat.GetSourceId()];
                summon.Position = chr.Position;
                chr.Map.SpawnSummon(summon);
                UpdateMapObjectVisibility(chr, summon);
            }

            m_mapEffect?.SendStartData(chr.Client);

            if (chr.ChalkBoardText != null)
            {
                chr.Client.Send(PacketCreator.UseChalkboard(chr, false));
            }
            if (chr.Energybar >= 10000)
            {
                BroadcastMessage(chr, PacketCreator.GiveForeignEnergyCharge(chr.Id, 10000));
            }
            //if (timeLimit > 0 && ForcedReturnMap != null)
            //{
            //    chr.Client.Send(PacketCreator.getClock(timeLimit));
            //    chr.startMapTimeLimitTask(this, ForcedReturnMap);
            //}
            if (m_mapTimer != null)
            {
                chr.Client.Send(PacketCreator.GetClock(m_mapTimer.GetTimeLeft()));
            }
            //if (chr.getEventInstance() != null && chr.getEventInstance().isTimerStarted())
            //{
            //    chr.Client.Send(PacketCreator.getClock((int)(chr.getEventInstance().getTimeLeft() / 1000)));
            //}
            if (Clock)
            {
                var date = DateTime.Now;
                chr.Client.Send(PacketCreator.GetClockTime(date.Hour, date.Minute, date.Second));
            }
            if (HasBoat.HasValue && HasBoat.Value)
            {
                chr.Client.Send(PacketCreator.BoatPacket(true));
            }
            else if (HasBoat.HasValue && !HasBoat.Value && (chr.Map.MapId != 200090000 || chr.Map.MapId != 200090010))
            {
                chr.Client.Send(PacketCreator.BoatPacket(false));
            }
            chr.ReceivePartyMemberHp();
        }

        public void RemoveMapObject(IMapleMapObject obj) => RemoveMapObject(obj.ObjectId);

        public void RemoveMapObject(int num)
        {
            lock (Mapobjects)
            {
                if (Mapobjects.ContainsKey(num))
                {
                    Mapobjects.Remove(num);
                }
            }
        }


        public void RemovePlayer(MapleCharacter chr)
        {
            lock (Characters)
            {
                Characters.Remove(chr);
            }

            RemoveMapObject(chr.ObjectId);
            BroadcastMessage(PacketCreator.RemovePlayerFromMap(chr.Id));

            foreach (var monster in chr.GetControlledMonsters())
            {
                monster.SetController(null);
                monster.ControllerHasAggro = false;
                monster.ControllerKnowsAboutAggro = false;
                UpdateMonsterController(monster);
            }

            chr.LeaveMap();
            //chr.cancelMapTimeLimitTask();

            foreach (var summon in chr.Summons.Values)
            {
                if (summon.IsPuppet())
                {
                    chr.CancelBuffStats(MapleBuffStat.Puppet);
                }
                else
                {
                    RemoveMapObject(summon);
                }
            }
        }

        public void MovePlayer(MapleCharacter player, Point newPosition)
        {
            player.Position = newPosition;
            var visibleObjectsNow = player.VisibleMapObjects.ToArray();
            foreach (var mo in visibleObjectsNow)
            {
                if (mo != null)
                {
                    IMapleMapObject tempMapObject;
                    if (Mapobjects.TryGetValue(mo.ObjectId, out tempMapObject) && tempMapObject == mo)
                    {
                        UpdateMapObjectVisibility(player, mo);
                    }
                    else
                    {
                        player.VisibleMapObjects.Remove(mo);
                    }
                }
            }

            foreach (
                var mo in
                    GetMapObjectsInRange(player.Position, MapleCharacter.MaxViewRangeSq, RangedMapobjectTypes)
                        .Where(mo => mo != null)
                        .Where(mo => !player.VisibleMapObjects.Contains(mo)))
            {
                mo.SendSpawnData(player.Client);
                player.VisibleMapObjects.Add(mo);
            }

            if (MapId == 240040611)
            {
                if (
                    GetMapObjectsInRange(player.Position, 25000,
                        new List<MapleMapObjectType> {MapleMapObjectType.Reactor}).Any())
                {
                    var reactor = GetReactorByOid(2408004);
                    if (reactor.State == 0)
                    {
                        reactor.HitReactor(player.Client);
                    }
                }
            }
        }

        public IMaplePortal GetPortal(string portalname)
            => Portals.Values.FirstOrDefault(x => x.PortalName == portalname);

        public IMaplePortal GetPortal(int portalId) => Portals[portalId];

        public MapleReactor GetReactorByOid(int oid)
        {
            IMapleMapObject mmo;
            if (!Mapobjects.TryGetValue(oid, out mmo))
                return null;

            if (mmo.GetType() == MapleMapObjectType.Reactor)
                return (MapleReactor) mmo;

            return null;
        }

        public MapleReactor GetReactorByName(string name)
        {
            lock (Mapobjects)
            {
                return Mapobjects.Values.FirstOrDefault(
                    x => x.GetType() == MapleMapObjectType.Reactor && ((MapleReactor) x).ReactorName == name) as
                    MapleReactor;
            }
        }

        public void DestroyReactor(int oid)
        {
            var reactor = GetReactorByOid(oid);
            var tMan = TimerManager.Instance;
            BroadcastMessage(PacketCreator.DestroyReactor(reactor));
            reactor.IsAlive = false;
            RemoveMapObject(reactor);
            reactor.IsTimerActive = false;
            if (reactor.Delay > 0)
            {
                tMan.RunOnceTask(() => { RespawnReactor(reactor); }, reactor.Delay);
            }
        }

        public void SpawnReactor(MapleReactor reactor)
        {
            reactor.Map = this;
            lock (Mapobjects)
            {
                SpawnAndAddRangedMapObject(reactor, client => { client.Send(reactor.MakeSpawnData()); }, null);
            }
        }

        private void RespawnReactor(MapleReactor reactor)
        {
            reactor.State = 0;
            reactor.IsAlive = true;
            SpawnReactor(reactor);
        }

        public void ResetReactors()
        {
            lock (Mapobjects)
            {
                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    ((MapleReactor) x).State = 0;
                    ((MapleReactor) x).IsTimerActive = false;
                    BroadcastMessage(PacketCreator.TriggerReactor((MapleReactor) x, 0));
                });
            }
        }

        public void SetReactorState()
        {
            lock (Mapobjects)
            {
                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    ((MapleReactor) x).State = 1;
                    BroadcastMessage(PacketCreator.TriggerReactor((MapleReactor) x, 1));
                });
            }
        }


        public void ShuffleReactors()
        {
            var points = new List<Point>();
            lock (Mapobjects)
            {
                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor)
                    .ToList()
                    .ForEach(x => { points.Add(((MapleReactor) x).Position); });

                points.Shuffle();

                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    ((MapleReactor) x).Position = points.Last();
                    points.Remove(points.Last());
                });
            }
        }

        public List<IMapleMapObject> GetMapObjectsInRect(Rectangle box, List<MapleMapObjectType> types)
        {
            var ret = new List<IMapleMapObject>();
            lock (Mapobjects)
            {
                ret.AddRange(Mapobjects.Values.Where(l => types.Contains(l.GetType()) && box.Contains(l.Position)));
            }
            return ret;
        }

        public List<MapleCharacter> GetPlayersInRect(Rectangle box, List<MapleCharacter> chr)
        {
            var character = new List<MapleCharacter>();
            lock (Characters)
            {
                character.AddRange(Characters.Where(x => chr.Contains(x.Client.Player) && box.Contains(x.Position)));
            }
            return character;
        }

        public List<MapleCharacter> GetPlayersInRect(Rectangle box)
        {
            var character = new List<MapleCharacter>();
            lock (Characters)
            {
                character.AddRange(Characters.Where(x => box.Contains(x.Position)));
            }
            return character;
        }

        public void disappearingItemDrop(IMapleMapObject dropper,MapleCharacter owner,  IMapleItem item, Point pos)
        {
             Point droppos = CalcDropPos(pos, pos);
             MapleMapItem drop = new MapleMapItem(item, droppos, dropper, owner);
             BroadcastMessage(PacketCreator.DropItemFromMapObject(item.ItemId, drop.ObjectId, 0, 0, dropper.Position, droppos, 3), drop.Position);
        }

        public void spawnItemDrop(IMapleMapObject dropper, MapleCharacter owner, IMapleItem item, Point pos, bool ffaDrop, bool expire)
        {
            TimerManager tMan = TimerManager.Instance;
            Point droppos = CalcDropPos(pos, pos);
            MapleMapItem drop = new MapleMapItem(item, droppos, dropper, owner);
            SpawnAndAddRangedMapObject(drop, (c) =>
            {
                c.Send(PacketCreator.DropItemFromMapObject(item.ItemId, drop.ObjectId, 0, ffaDrop ? 0 : owner.Id, dropper.Position, droppos, 1));
            }, null);

            BroadcastMessage(PacketCreator.DropItemFromMapObject(item.ItemId, drop.ObjectId, 0, ffaDrop ? 0 : owner.Id, dropper.Position, droppos, 0), drop.Position);

            if (expire)
            {
                tMan.RunOnceTask(() => ExpireMapItemJob(drop), m_dropLife);
            }

            ActivateItemReactors(drop);
        }

        public void SpawnMonsterWithEffect(MapleMonster monster, int effect, Point pos)
        {
            try
            {
                monster.Map = this;
                var spos = new Point(pos.X, pos.Y - 1);
                spos = CalcPointBelow(spos);
                spos.Y--;
                monster.Position = spos;
                if (MapId < 925020000 || MapId > 925030000)
                {
                    //monster.disableDrops();
                }
                lock (Mapobjects)
                {
                    SpawnAndAddRangedMapObject(monster,
                        mc => { mc.Send(PacketCreator.SpawnMonster(monster, true, (byte) effect)); }, null);
                    if (monster.HasBossHpBar)
                    {
                        BroadcastMessage(monster.BossHpBarPacket, monster.Position);
                    }
                    UpdateMonsterController(monster);
                }
                SpawnedMonstersOnMap.Increment();
            }
            catch
            {
                throw new Exception("SpawnMonsterWithEffect 出错！");
            }
        }

        private void ExpireMapItemJob(MapleMapItem mapitem)
        {
            if (mapitem != null && Mapobjects.ContainsKey(mapitem.ObjectId))
            {
                lock (mapitem)
                {
                    if (mapitem.IsPickedUp)
                    {
                        return;
                    }
                    BroadcastMessage(PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 0, 0), mapitem.Position);
                    RemoveMapObject(mapitem);
                    mapitem.IsPickedUp = true;
                }
            }
        }

        private void ActivateItemReactor(MapleMapItem mapitem, MapleReactor reactor, MapleClient c)
        {
            Console.WriteLine("run Activate");
            if (mapitem != null && mapitem == Mapobjects[mapitem.ObjectId])
            {
                lock (mapitem)
                {
                    if (mapitem.IsPickedUp)
                    {
                        return;
                    }
                    BroadcastMessage(PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 0, 0), mapitem.Position);
                    RemoveMapObject(mapitem);
                    reactor.HitReactor(c);
                    reactor.IsTimerActive = true;
                    if (reactor.Delay > 0)
                    {
                        //This shit is negative.. Fix?
                        TimerManager.Instance.RunOnceTask(() =>
                        {
                            reactor.State = 0;
                            BroadcastMessage(PacketCreator.TriggerReactor(reactor, 0));
                            Console.WriteLine("run reactor");
                        }, reactor.Delay);
                    }
                }
            }
        }


        public void Respawn()
        {
            if (!Characters.Any())
            {
                return;
            }
            var numShouldSpawn = (m_monsterSpawn.Count - SpawnedMonstersOnMap.Value)*(int) Math.Round(m_monsterRate);
            if (numShouldSpawn > 0)
            {
                var randomSpawn = new List<SpawnPoint>(m_monsterSpawn);
                randomSpawn.Shuffle();
                var spawned = 0;

                foreach (var spawnPoint in randomSpawn)
                {
                    if (spawnPoint.ShouldSpawn())
                    {
                        spawnPoint.SpawnMonster(this);
                        spawned++;
                    }
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }

        public void SpawnRevives(MapleMonster monster)
        {
            monster.Map = this;
            lock (Mapobjects)
            {
                SpawnAndAddRangedMapObject(monster,
                    client => { client.Send(PacketCreator.SpawnMonster(monster, false)); }, null);
                UpdateMonsterController(monster);
            }
            SpawnedMonstersOnMap.Increment();
        }

        #region BroadCast

        public void BroadcastNonGmMessage(MapleCharacter source, OutPacket packet)
        {
            lock (Characters)
            {
                Characters.ForEach(chr =>
                {
                    if (chr != source && (chr.GmLevel == 0 || !chr.IsHidden))
                    {
                        chr.Client.Send(packet);
                    }
                });
            }
        }

        public void BroadcastMessage(OutPacket packet)
            => BroadcastMessage(null, packet, double.PositiveInfinity, Point.Empty);


        public void BroadcastMessage(MapleCharacter source, OutPacket packet)
        {
            lock (Characters)
            {
                Characters.ForEach(chr =>
                {
                    if (chr != source)
                    {
                        chr.Client.Send(packet);
                    }
                });
            }
        }

        public void BroadcastMessage(MapleCharacter source, OutPacket packet, bool repeatToSource)
            => BroadcastMessage(repeatToSource ? null : source, packet, double.PositiveInfinity, source.Position);


        public void BroadcastMessage(MapleCharacter source, OutPacket packet, bool repeatToSource, bool ranged)
            =>
                BroadcastMessage(repeatToSource ? null : source, packet,
                    ranged ? MapleCharacter.MaxViewRangeSq : double.PositiveInfinity, source.Position);


        public void BroadcastMessage(OutPacket packet, Point rangedFrom)
            => BroadcastMessage(null, packet, MapleCharacter.MaxViewRangeSq, rangedFrom);


        public void BroadcastMessage(MapleCharacter source, OutPacket packet, Point rangedFrom)
            => BroadcastMessage(source, packet, MapleCharacter.MaxViewRangeSq, rangedFrom);


        private void BroadcastMessage(MapleCharacter source, OutPacket packet, double rangeSq, Point rangedFrom)
        {
            lock (Characters)
            {
                Characters.ForEach(chr =>
                {
                    if (chr != source)
                    {
                        if (rangeSq < double.PositiveInfinity)
                        {
                            if (rangedFrom.DistanceSquare(chr.Position) <= rangeSq)
                            {
                                chr.Client.Send(packet);
                            }
                        }
                        else
                        {
                            chr.Client.Send(packet);
                        }
                    }
                });
            }
        }

        #endregion
    }


    internal interface ISpawnCondition
    {
        bool CanSpawn(MapleCharacter chr);
    }
}