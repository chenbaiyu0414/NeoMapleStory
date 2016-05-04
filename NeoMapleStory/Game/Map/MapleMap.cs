using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Packet;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.World;
using Quartz;

namespace NeoMapleStory.Game.Map
{
    public enum MapleMapObjectType
    {
        Npc, Monster, Item, Player, Door, Summon, Shop, Mist, Reactor, HiredMerchant, Love
    }


    public class MapleMap
    {
        public int MapId { get; set; }


        public Dictionary<int, IMapleMapObject> Mapobjects { get; private set; } =
            new Dictionary<int, IMapleMapObject>();

        public List<MapleCharacter> Characters { get; set; } = new List<MapleCharacter>();

        public Dictionary<int, IMaplePortal> Portals { get; private set; } = new Dictionary<int, IMaplePortal>();

        public List<Rectangle> Areas { get; set; } = new List<Rectangle>();

        public MapleFootholdTree Footholds { get; set; } = null;

        public int ReturnMapId { get; set; }

        public MapleMap ReturnMap => MasterServer.Instance.ChannelServers[_channel].MapFactory.GetMap(ReturnMapId);

        public bool Clock { get; set; }

        public bool? HasBoat
        {
            get
            {
                if (_boat.Value && _docked)
                    return true;
                else if (_boat.Value)
                    return false;
                else
                    return null;
            }
            set { _boat = value; }
        }

        public string MapName { get; set; }

        public string StreetName { get; set; }

        public bool Everlast { get; set; } = false;

        public int ForcedReturnMapId { get; set; } = 999999999;

        public MapleMap ForcedReturnMap
        {
            get { return MasterServer.Instance.ChannelServers[_channel].MapFactory.GetMap(ForcedReturnMapId); }
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

        public int MaxRegularSpawn => (int)(_monsterSpawn.Count / _monsterRate);

        public bool HasEvent { get; set; }

        public bool IsLootable { get; set; } = true;

        private static readonly int MaxOid = 20000;

        private static readonly List<MapleMapObjectType> RangedMapobjectTypes = new List<MapleMapObjectType>()
        {
            MapleMapObjectType.Item,
            MapleMapObjectType.Monster,
            MapleMapObjectType.Door,
            MapleMapObjectType.Summon,
            MapleMapObjectType.Reactor
        };

        /**
         * Holds a mapping of all oid -> MapleMapObject on this map. mapobjects is NOT a lock collection since it
         * has to be lock together with runningOid that's why all access to mapobjects have to be done trough an
         * explicit lock block
         */

        private readonly List<SpawnPoint> _monsterSpawn = new List<SpawnPoint>();
        public InterLockedInt SpawnedMonstersOnMap { get; } = new InterLockedInt(0);

        private int _runningOid = 100;

        private readonly int _channel;
        private readonly float _monsterRate;
        private bool _dropsDisabled = false;

        private bool? _boat;


        private bool _docked;

        private readonly MapleMapEffect _mapEffect = null;


        private readonly MapleMapTimer _mapTimer = null;
        private int _dropLife = 180000; // 以毫秒为单位掉落后消失的时间

        private bool _hasEvent;
        private float _origMobRate;
        //private MapleOxQuiz ox = null;
        private TriggerKey _spawnWorker;

        private bool _cannotInvincible = false;
        private bool _canVipRock = true;

        public MapleMap(int mapid, int channel, int returnMapId, float monsterRate)
        {

            MapId = mapid;
            _channel = channel;
            ReturnMapId = returnMapId;
            _monsterRate = monsterRate;
            _origMobRate = monsterRate;

            if (monsterRate > 0)
            {
                _spawnWorker = TimerManager.Instance.RepeatTask(RespawnWorker, 7*1000);
            }
        }

        public IMaplePortal FindClosestSpawnpoint(Point from)
        {
            IMaplePortal closest = null;
            double shortestDistance = double.PositiveInfinity;
            foreach (IMaplePortal portal in Portals.Values)
            {
                double distance = portal.Position.DistanceSquare(from);
                if (portal.Type == PortalType.MapPortal && distance < shortestDistance &&
                    portal.TargetMapId == 999999999)
                {
                    closest = portal;
                    shortestDistance = distance;
                }
            }
            return closest;
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


        public void AddMapObject(IMapleMapObject mapobject)
        {
            lock (Mapobjects)
            {
                mapobject.ObjectId = _runningOid;
                if (Mapobjects.ContainsKey(_runningOid))
                    Mapobjects[_runningOid] = mapobject;
                else
                    Mapobjects.Add(_runningOid, mapobject);

                IncrementRunningOid();
            }
        }

        private void IncrementRunningOid()
        {
            _runningOid++;
            for (int numIncrements = 1; numIncrements < MaxOid; numIncrements++)
            {
                if (_runningOid > MaxOid)
                {
                    _runningOid = 100;
                }
                if (Mapobjects.ContainsKey(_runningOid))
                {
                    _runningOid++;
                }
                else
                {
                    return;
                }
            }
            throw new Exception($"Out of OIDs on map {MapId} (channel: {_channel})");
        }

        public void RespawnWorker()
        {
            if (Characters.Any() == false)
            {
                return;
            }
            else
            {
                int numShouldSpawn = MaxRegularSpawn - SpawnedMonstersOnMap.Value;
                if (numShouldSpawn > 0)
                {
                    List<SpawnPoint> randomSpawn = new List<SpawnPoint>(_monsterSpawn);
                    randomSpawn.Shuffle();
                    int spawned = 0;
                    foreach (SpawnPoint spawnPoint in randomSpawn)
                    {
                        if (spawnPoint.ShouldSpawn())
                        {
                            spawnPoint.spawnMonster(this);
                            spawned++;
                        }
                        if (spawned >= numShouldSpawn)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void addMonsterSpawn(MapleMonster monster, int mobTime)
        {
            Point newpos = calcPointBelow(monster.Position);
            newpos.Y -= 1;
            SpawnPoint sp = new SpawnPoint(monster, newpos, mobTime);

            _monsterSpawn.Add(sp);
        }

        private Point calcPointBelow(Point initial)
        {
            MapleFoothold fh = Footholds.FindBelow(initial);
            if (fh == null)
            {
                return Point.Empty;
            }
            int dropY = fh.Point1.Y;
            if (!fh.IsWall() && fh.Point1.Y != fh.Point2.Y)
            {
                double s1 = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                double s2 = Math.Abs(fh.Point2.X - fh.Point1.X);
                double s4 = Math.Abs(initial.X - fh.Point1.X);
                double alpha = Math.Atan(s2 / s1);
                double beta = Math.Atan(s1 / s2);
                double s5 = Math.Cos(alpha) * (s4 / Math.Cos(beta));
                if (fh.Point2.Y < fh.Point1.Y)
                {
                    dropY = fh.Point1.Y - (int)s5;
                }
                else
                {
                    dropY = fh.Point1.Y + (int)s5;
                }
            }
            return new Point(initial.X, dropY);
        }

        public bool isPQMap()
        {
            //Does NOT include CPQ maps
            int tmapid = MapId;
            if ((tmapid > 922010000 && tmapid < 922011100) || (tmapid >= 103000800 && tmapid < 103000890))
            {
                //kpq + lpq only atm
                return true;
            }
            return false;
        }


        public void spawnMonster(MapleMonster monster)
        {
            if (!Characters.Any() && !isPQMap())
            {
                return;
            }
            monster.Map = this;
            int removeAfter = monster.Stats.RemoveAfter;
            if (removeAfter > 0)
            {
                TimerManager.Instance.RunOnceTask(() =>
                {
                    killMonster(monster, Characters[0], false, false, 3);
                }, removeAfter);
            }
            lock (Mapobjects)
            {
                SpawnAndAddRangedMapObject(monster, mc =>
                {
                    mc.Send(PacketCreator.SpawnMonster(monster, true));
                }, null);
                UpdateMonsterController(monster);
            }
            SpawnedMonstersOnMap.Increment();
        }

        public List<IMapleMapObject> getAllPlayers()
        {
            return GetMapObjectsInRange(new Point(0, 0), double.PositiveInfinity, new List<MapleMapObjectType> { MapleMapObjectType.Player});
        }

        public bool damageMonster(MapleCharacter chr, MapleMonster monster, int damage)
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
                    MapleMonster mons = chr.Map.getMonsterByOid(mapobject.ObjectId);
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
                        int monsterhp = monster.Hp;
                        monster.damage(chr, damage, true);
                        if (!monster.IsAlive)
                        {
                            // monster just died
                            killMonster(monster, chr, true);
                            if (monster.Id >= 8810002 && monster.Id <= 8810009)
                            {
                                foreach (var mapobject in chr.Map.Mapobjects.Values)
                                {
                                    MapleMonster mons = chr.Map.getMonsterByOid(mapobject.ObjectId);
                                    if (mons != null)
                                    {
                                        if (mons.Id == 8810018 || mons.Id == 8810026)
                                        {
                                            damageMonster(chr, mons, monsterhp);
                                        }
                                    }
                                }
                            }
                            else if ((monster.Id >= 8820002 && monster.Id <= 8820006) || (monster.Id >= 8820015 && monster.Id <= 8820018))
                            {
                                foreach (var mapobject in chr.Map.Mapobjects.Values)
                                {
                                    MapleMonster mons = chr.Map.getMonsterByOid(mapobject.ObjectId);
                                    if (mons?.Id >= 8820010 && mons.Id <= 8820014)
                                    {
                                        damageMonster(chr, mons, monsterhp);
                                    }
                                }
                            }
                        }
                        else if (monster.Id >= 8810002 && monster.Id <= 8810009)
                        {
                            foreach (var mapobject in chr.Map.Mapobjects.Values)
                            {
                                MapleMonster mons = chr.Map.getMonsterByOid(mapobject.ObjectId);
                                if (mons != null)
                                {
                                    if (mons.Id == 8810018 || mons.Id == 8810026)
                                    {
                                        damageMonster(chr, mons, damage);
                                    }
                                }
                            }
                        }
                        else if ((monster.Id >= 8820002 && monster.Id <= 8820006) || (monster.Id >= 8820015 && monster.Id <= 8820018))
                        {
                            foreach (var mapobject in chr.Map.Mapobjects.Values)
                            {
                                MapleMonster mons = chr.Map.getMonsterByOid(mapobject.ObjectId);
                                if (mons?.Id >= 8820010 && mons.Id <= 8820014)
                                {
                                    damageMonster(chr, mons, damage);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void killMonster( MapleMonster monster, MapleCharacter chr,bool withDrops)
        {
            killMonster(monster, chr, withDrops, false, 1);
        }

        public void killMonster(MapleMonster monster, MapleCharacter chr, bool withDrops, bool secondTime)
        {
            killMonster(monster, chr, withDrops, secondTime, 1);
        }

        public void killMonster(int monsId)
        {
            foreach (var mmo in Mapobjects.Values)
            {
                var monster = mmo as MapleMonster;
                if (monster?.Id == monsId)
                {
                    var playerObjects = getAllPlayers();
                    killMonster(monster, (MapleCharacter)playerObjects[0], false);
                }
            }
        }

        public void killMonster(MapleMonster monster, MapleCharacter chr, bool withDrops, bool secondTime,
            int animation)
        {
            if (chr.AntiCheatTracker.CheckHpLoss())
            {
                chr.AntiCheatTracker.registerOffense(CheatingOffense.AttackWithoutGettingHit);
            }
            StringBuilder names = new StringBuilder();
            if (monster.Id == 8500002 || monster.Id == 8800002)
            {
                if (chr.Party != null)
                {
                    MapleParty party = chr.Party;
                    List<MapleCharacter> partymems = party.GetPartyMembers();
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
                int rankType = monster.Id == 8500002 ? 1 : 0;
                int oid = monster.ObjectId;
                //SpeedRankings.setEndTime(rankType, oid, DateTime.Now.GetTimeMilliseconds());
                //SpeedRankings.insertRankingToSQL(rankType, names.toString(), SpeedRankings.calcTime(rankType, oid));
            }
            if (monster.Id == 8810018 && !secondTime)
            {
                TimerManager.Instance.RunOnceTask(() =>
                {
                    killMonster(monster, chr, withDrops, true, 1);
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
            removeMapObject(monster);

            if (monster.Id >= 8800003 && monster.Id <= 8800010)
            {
                bool makeZakReal = true;
                foreach (var mobj in Mapobjects.Values)
                {
                    MapleMonster mons = getMonsterByOid(mobj.ObjectId);
                    if (mons != null)
                    {
                        if (mons.Id >= 8800003 && mons.Id <= 8800010)
                        {
                            makeZakReal = false;
                        }
                    }
                }
                if (makeZakReal)
                {
                    foreach (var mobj in Mapobjects.Values)
                    {
                        MapleMonster mons = chr.Map.getMonsterByOid(mobj.ObjectId);
                        if (mons != null)
                        {
                            if (mons.Id == 8800000)
                            {
                                //makeMonsterReal(mons);
                                //updateMonsterController(mons);
                            }
                        }
                    }
                }
            }
            MapleCharacter dropOwner = monster.killBy(chr);
            if (withDrops /*&& !monster.dropdisabled*/)
            {
                if (dropOwner == null)
                {
                    dropOwner = chr;
                }
                dropFromMonster(dropOwner, monster);
            }
        }

        private void dropFromMonster(MapleCharacter dropOwner, MapleMonster monster)
        {
            //if (dropsDisabled || monster.dropsDisabled())
            //{
            //    return;
            //}

            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;

            int maxDrops = monster.getMaxDrops(dropOwner);
            bool explosive = monster.Stats.IsExplosive;

            List<int> toDrop = new List<int>();

            for (int i = 0; i < maxDrops; i++)
            {
                toDrop.Add(monster.getDrop(dropOwner));
            }

            if (dropOwner.EventInstanceManager == null)
            {
                int chance = (int) (Randomizer.NextDouble()*100);
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

            List<int> alreadyDropped = new List<int>();
            int htpendants = 0;
            int htstones = 0;
            for (int i = 0; i < toDrop.Count; i++)
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

            Point[] toPoint = new Point[toDrop.Count];
            int shiftDirection = 0;
            int shiftCount = 0;

            int curX = Math.Min(Math.Max(monster.Position.X - 25*(toDrop.Count/2), Footholds.MinDropX + 25),
                Footholds.MaxDropX - toDrop.Count*25);
            int curY = Math.Max(monster.Position.Y, Footholds.Point1.Y);

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
                for (int i = 0; i < toDrop.Count; i++)
                {
                    MapleFoothold wall = Footholds.FindWall(new Point(curX, curY),
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
                    Point dropPos = calcDropPos(new Point(curX + i*25, curY), monster.Position);
                    toPoint[i] = new Point(curX + i*25, curY);
                    int drop = toDrop[i];

                    if (drop == -1)
                    {
                        int mesoRate = dropOwner.Client.ChannelServer.MesoRate;
                        double mesoDecrease = Math.Pow(0.93, monster.Stats.Exp/300.0);
                        if (mesoDecrease > 1.0)
                        {
                            mesoDecrease = 1.0;
                        }
                        int tempmeso = Math.Min(30000,
                            (int) (mesoDecrease*(monster.Stats.Exp)*(1.0 + Randomizer.NextDouble()*20)/10.0));
                        if (dropOwner.GetBuffedValue(MapleBuffStat.Mesoup) != null)
                        {
                            var buffedValue = dropOwner.GetBuffedValue(MapleBuffStat.Mesoup);
                            if (buffedValue != null)
                                tempmeso = (int) ((double) tempmeso*buffedValue.Value/100.0);
                        }

                        int meso = tempmeso;

                        if (meso > 0)
                        {
                            MapleMonster dropMonster = monster;
                            MapleCharacter dropChar = dropOwner;
                            TimerManager.Instance.RunOnceTask(() =>
                            {
                                spawnMesoDrop(meso*mesoRate, dropPos, dropMonster, dropChar, explosive);
                            }, monster.Stats.GetAnimationTime("die1"));
                        }
                    }
                    else
                    {
                        IMapleItem idrop;
                        MapleInventoryType type = ii.GetInventoryType(drop);
                        if (type == MapleInventoryType.Equip)
                        {
                            Equip nEquip = ii.RandomizeStats((Equip) ii.GetEquipById(drop));
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

                        Console.WriteLine(
                            "Created as a drop from monster " + monster.ObjectId + " (" + monster.Id + ") at " +
                            dropPos.ToString() + " on map " + MapId);

                        MapleMapItem mdrop = new MapleMapItem(idrop, dropPos, monster, dropOwner);
                        IMapleMapObject dropMonster = monster;
                        MapleCharacter dropChar = dropOwner;
                        TimerManager.Instance.RunOnceTask(() =>
                        {
                                SpawnAndAddRangedMapObject(mdrop, mc=>
                                {
                                    Console.WriteLine("run spawnandadd");
                                    mc.Send(PacketCreator.DropItemFromMapObject(drop,
                                        mdrop.ObjectId,
                                        dropMonster.ObjectId,
                                        explosive ? 0 : dropChar.Id,
                                        dropMonster.Position,
                                        dropPos,
                                        1));
                                    activateItemReactors(mdrop);
                                } ,null);

                                TimerManager.Instance.RunOnceTask(()=> ExpireMapItemJob(mdrop), _dropLife);
                            
                        }, monster.Stats.GetAnimationTime("die1"));

                    }
                }
            }
        }

        private void activateItemReactors(MapleMapItem drop)
        {
            IMapleItem item = drop.Item;
            TimerManager tMan = TimerManager.Instance; //check for reactors on map that might use this item
            foreach (var o in Mapobjects.Values)
            {
                if (o.GetType() == MapleMapObjectType.Reactor)
                {
                    MapleReactor reactor = o as MapleReactor;
                    if (reactor == null)
                        continue;

                    if (reactor.ReactorType == 100)
                    {
                        if (reactor.ReactorItem.Item1 == item.ItemId && reactor.ReactorItem.Item2 <= item.Quantity)
                        {
                            Rectangle area = reactor.Area;

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


        public MapleMonster getMonsterByOid(int oid)
        {
            return
                Mapobjects.Values.FirstOrDefault(x => x.ObjectId == oid && x.GetType() == MapleMapObjectType.Monster) as
                    MapleMonster;
        }

        public MapleMonster getMonsterById(int id)
        {
            lock(Mapobjects) {
                foreach (var obj in Mapobjects.Values)
                {
                    if (obj.GetType() != MapleMapObjectType.Monster) continue;
                    if (((MapleMonster)obj).Id  == id)
                    {
                        return (MapleMonster)obj;
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
                        .Any(obj => ((MapleNpc)obj).Id == npcid);
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

        public void moveMonster(MapleMonster monster, Point reportedPos)
        {
            monster.Position = (reportedPos);
            lock (Characters)
            {
                foreach (MapleCharacter chr in Characters)
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
                        monster.GetController().stopControllingMonster(monster);
                    }
                    else
                    {
                        // controller is on the map, monster has an controller, everything is fine
                        return;
                    }
                }

                int mincontrolled = -1;
                MapleCharacter newController = null;
                lock (Characters)
                {
                    foreach (var chr in Characters)
                    {
                        if (!chr.IsHidden && (chr.getControlledMonsters().Count < mincontrolled || mincontrolled == -1))
                        {
                            mincontrolled = chr.getControlledMonsters().Count;
                            newController = chr;
                        }
                    }
                }
                if (newController != null)
                {
                    // was a new controller found? (if not no one is on the map)
                    if (monster.Stats.IsFirstAttack)
                    {
                        newController.controlMonster(monster, true);
                        monster.ControllerHasAggro = true;
                        monster.ControllerKnowsAboutAggro = true;
                    }
                    else
                    {
                        newController.controlMonster(monster, false);
                    }
                }
            }
        }

        public List<IMapleMapObject> GetMapObjectsInRange(Point from, double rangeSq, List<MapleMapObjectType> types)
        {
            List<IMapleMapObject> ret = new List<IMapleMapObject>();
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
                    UpdateMonsterController((MapleMonster)o);
                }
            }
            MapleCharacter chr = mapleClient.Player;

            if (chr != null)
            {
                foreach (
                    var o in GetMapObjectsInRange(chr.Position, MapleCharacter.MaxViewRangeSq, RangedMapobjectTypes))
                {
                    if (o.GetType() == MapleMapObjectType.Reactor)
                    {
                        if (((MapleReactor)o).IsAlive)
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
                mapobject.ObjectId = _runningOid;

                lock (Characters)
                {
                    foreach (
                        var chr in
                            Characters.Where(chr => condition == null || condition.CanSpawn(chr))
                                .Where(
                                    chr =>
                                        chr.Position.DistanceSquare(mapobject.Position) <= MapleCharacter.MaxViewRangeSq)
                        )
                    {
                        packetbakery(chr.Client);
                        chr.VisibleMapObjects.Add(mapobject);
                    }
                }

                if (Mapobjects.ContainsKey(_runningOid))
                    Mapobjects[_runningOid] = mapobject;
                else
                    Mapobjects.Add(_runningOid, mapobject);
                IncrementRunningOid();
            }
        }

        public void SpawnSummon(MapleSummon summon)
        {
            SpawnAndAddRangedMapObject(summon, (client) =>
            {
                int skillLevel = summon.Owner.getSkillLevel(SkillFactory.GetSkill(summon.SkillId));
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

        public void spawnMesoDrop(int meso, Point position, IMapleMapObject dropper, MapleCharacter owner, bool ffaLoot)
        {
            Point droppos = calcDropPos(position, position);
            MapleMapItem mdrop = new MapleMapItem(meso, droppos, dropper, owner);
            SpawnAndAddRangedMapObject(mdrop, client =>
            {
                client.Send(PacketCreator.DropMesoFromMapObject(meso, mdrop.ObjectId, dropper.ObjectId,
                    ffaLoot ? 0 : owner.Id, dropper.Position, droppos, 1));
            }, null);
            TimerManager.Instance.RunOnceTask(() => ExpireMapItemJob(mdrop), _dropLife);
        }

        private Point calcDropPos(Point initial, Point fallback)
        {
            Point ret = calcPointBelow(new Point(initial.X, initial.Y - 99));
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
                    foreach (MaplePet pet in chr.Pets)
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

            MapleStatEffect summonStat = chr.GetStatForBuff(MapleBuffStat.Summon);
            if (summonStat != null)
            {
                MapleSummon summon = chr.Summons[summonStat.GetSourceId()];
                summon.Position = chr.Position;
                chr.Map.SpawnSummon(summon);
                UpdateMapObjectVisibility(chr, summon);
            }

            _mapEffect?.SendStartData(chr.Client);

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
            if (_mapTimer != null)
            {
                chr.Client.Send(PacketCreator.GetClock(_mapTimer.GetTimeLeft()));
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

        public void removeMapObject(IMapleMapObject obj) => removeMapObject(obj.ObjectId);

        public void removeMapObject(int num)
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

            removeMapObject(chr.ObjectId);
            BroadcastMessage(PacketCreator.RemovePlayerFromMap(chr.Id));

            foreach (MapleMonster monster in chr.getControlledMonsters())
            {
                monster.SetController(null);
                monster.ControllerHasAggro = false;
                monster.ControllerKnowsAboutAggro = false;
                UpdateMonsterController(monster);
            }

            chr.LeaveMap();
            //chr.cancelMapTimeLimitTask();

            foreach (MapleSummon summon in chr.Summons.Values)
            {
                if (summon.IsPuppet())
                {
                    chr.CancelBuffStats(MapleBuffStat.Puppet);
                }
                else
                {
                    removeMapObject(summon);
                }
            }
        }

        public void MovePlayer(MapleCharacter player, Point newPosition)
        {
            player.Position = newPosition;
            IMapleMapObject[] visibleObjectsNow = player.VisibleMapObjects.ToArray();
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
                        new List<MapleMapObjectType>() { MapleMapObjectType.Reactor }).Any())
                {
                    MapleReactor reactor = GetReactorByOid(2408004);
                    if (reactor.State == 0)
                    {
                        reactor.HitReactor(player.Client);
                    }
                }
            }
        }

        public IMaplePortal getPortal(string portalname)
            => Portals.Values.FirstOrDefault(x => x.PortalName == portalname);

        public IMaplePortal getPortal(int portalId) => Portals[portalId];

        public MapleReactor GetReactorByOid(int oid)
        {
            IMapleMapObject mmo;
            if (!Mapobjects.TryGetValue(oid, out mmo))
                return null;

            if (mmo.GetType() == MapleMapObjectType.Reactor)
                return (MapleReactor)mmo;

            return null;
        }

        public MapleReactor GetReactorByName(string name)
        {
            lock (Mapobjects)
            {
                Mapobjects.Values.FirstOrDefault(
                    x => x.GetType() == MapleMapObjectType.Reactor && ((MapleReactor)x).ReactorName == name);
            }
            return null;
        }

        public void DestroyReactor(int oid)
        {
            MapleReactor reactor = GetReactorByOid(oid);
            TimerManager tMan = TimerManager.Instance;
            BroadcastMessage(PacketCreator.DestroyReactor(reactor));
            reactor.IsAlive = false;
            removeMapObject(reactor);
            reactor.IsTimerActive = false;
            if (reactor.Delay > 0)
            {
                tMan.RunOnceTask(() =>
                {
                    RespawnReactor(reactor);
                }, reactor.Delay);
            }
        }

        public void SpawnReactor(MapleReactor reactor)
        {
            reactor.Map = this;
            lock (Mapobjects)
            {
                SpawnAndAddRangedMapObject(reactor, client =>
                {
                    client.Send(reactor.MakeSpawnData());
                }, null);
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
                    ((MapleReactor)x).State = 0;
                    ((MapleReactor)x).IsTimerActive = false;
                    BroadcastMessage(PacketCreator.TriggerReactor((MapleReactor)x, 0));
                });
            }
        }

        public void SetReactorState()
        {
            lock (Mapobjects)
            {
                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    ((MapleReactor)x).State = 1;
                    BroadcastMessage(PacketCreator.TriggerReactor((MapleReactor)x, 1));
                });
            }
        }


        public void ShuffleReactors()
        {
            List<Point> points = new List<Point>();
            lock (Mapobjects)
            {

                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    points.Add(((MapleReactor)x).Position);
                });

                points.Shuffle();

                Mapobjects.Values.Where(x => x.GetType() == MapleMapObjectType.Reactor).ToList().ForEach(x =>
                {
                    ((MapleReactor)x).Position = points.Last();
                    points.Remove(points.Last());
                });
            }
        }

        public List<IMapleMapObject> getMapObjectsInRect(Rectangle box, List<MapleMapObjectType> types)
        {
            List<IMapleMapObject> ret = new List<IMapleMapObject>();
            lock (Mapobjects)
            {
                ret.AddRange(Mapobjects.Values.Where(l => types.Contains(l.GetType()) && box.Contains(l.Position)));
            }
            return ret;
        }

        public List<MapleCharacter> getPlayersInRect(Rectangle box, List<MapleCharacter> chr)
        {
            List<MapleCharacter> character = new List<MapleCharacter>();
            lock (Characters)
            {
                character.AddRange(Characters.Where(x => chr.Contains(x.Client.Player) && box.Contains(x.Position)));
            }
            return character;
        }

        public List<MapleCharacter> getPlayersInRect(Rectangle box)
        {
            List<MapleCharacter> character = new List<MapleCharacter>();
            lock (Characters)
            {
                character.AddRange(Characters.Where(x => box.Contains(x.Position)));
            }
            return character;
        }

        public void spawnMonsterWithEffect(MapleMonster monster, int effect, Point pos)
        {
            try
            {
                monster.Map = this;
                Point spos = new Point(pos.X, pos.Y - 1);
                spos = calcPointBelow(spos);
                spos.Y--;
                monster.Position = spos;
                if (MapId < 925020000 || MapId > 925030000)
                {
                    //monster.disableDrops();
                }
                lock (this.Mapobjects)
                {
                    SpawnAndAddRangedMapObject(monster, mc =>
                    {
                        mc.Send(PacketCreator.SpawnMonster(monster, true, (byte)effect));
                    }, null);
                    if (monster.HasBossHPBar)
                    {
                        BroadcastMessage(monster.BossHPBarPacket, monster.Position);
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
                    removeMapObject(mapitem);
                    mapitem.IsPickedUp = true;
                }
            }

        }

        private void ActivateItemReactor(MapleMapItem mapitem, MapleReactor reactor, MapleClient c)
        {
            Console.WriteLine("run Activate");
            if (mapitem != null && mapitem == Mapobjects[mapitem.ObjectId])
            {
                lock(mapitem)
                {
                    if (mapitem.IsPickedUp)
                    {
                        return;
                    }
                   BroadcastMessage(PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 0, 0), mapitem.Position);
                    removeMapObject(mapitem);
                    reactor.HitReactor(c);
                    reactor.IsTimerActive = true;
                    if (reactor.Delay > 0)
                    { //This shit is negative.. Fix?
                        TimerManager.Instance.RunOnceTask(()=>
                        {
                            reactor.State = 0;
                            BroadcastMessage(PacketCreator.TriggerReactor(reactor, 0));
                            Console.WriteLine("run reactor");
                        }, reactor.Delay);
                    }
                }
            }
        }
    

        public void respawn()
        {
            if (!Characters.Any())
            {
                return;
            }
            int numShouldSpawn = (_monsterSpawn.Count - SpawnedMonstersOnMap.Value)*(int) Math.Round(_monsterRate);
            if (numShouldSpawn > 0)
            {
                List<SpawnPoint> randomSpawn = new List<SpawnPoint>(_monsterSpawn);
                randomSpawn.Shuffle();
                int spawned = 0;

                foreach (SpawnPoint spawnPoint in randomSpawn)
                {
                    if (spawnPoint.ShouldSpawn())
                    {
                        spawnPoint.spawnMonster(this);
                        spawned++;
                    }
                    if (spawned >= numShouldSpawn)
                    {
                        break;
                    }
                }
            }
        }
    }

    

    interface ISpawnCondition
    {

        bool CanSpawn(MapleCharacter chr);
    }
}

