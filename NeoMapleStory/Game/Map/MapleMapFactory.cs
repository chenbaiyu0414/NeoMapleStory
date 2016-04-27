using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Mob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NeoMapleStory.Game.Map
{
     public class MapleMapFactory
    {
        private readonly IMapleDataProvider _mSource;
        private readonly IMapleData _mNameData;

        public Dictionary<int, MapleMap> Maps { get; private set; } = new Dictionary<int, MapleMap>();
        public int ChannelId { get; set; }

        public MapleMapFactory(IMapleDataProvider source, IMapleDataProvider stringSource)
        {
            _mSource = source;
            _mNameData = stringSource.GetData("Map.img");
        }

        public bool DestroyMap(int mapid)
        {
            lock (Maps)
            {
                if (Maps.ContainsKey(mapid))
                {
                    return Maps.Remove(mapid);
                }
            }
            return false;
        }

        public MapleMap GetMap(int mapid)
        {
            int omapid = mapid;
            MapleMap map;
            if (!Maps.TryGetValue(omapid,out map))
            {
                lock (this)
                {
                    // check if someone else who was also synchronized has loaded the map already
                    if (Maps.ContainsKey(omapid))
                    {
                        return Maps[omapid];
                    }
                    string mapName = GetMapName(mapid);
                    IMapleData mapData = _mSource.GetData(mapName);
                    string link = MapleDataTool.GetString(mapData.GetChildByPath("info/link"), "");
                    if (!link.Equals(""))
                    {
                        mapName = GetMapName(int.Parse(link));
                        mapData = _mSource.GetData(mapName);
                    }
                    float monsterRate = 0;
                    IMapleData mobRate = mapData.GetChildByPath("info/mobRate");
                    if (mobRate != null)
                    {
                        monsterRate = (float)mobRate.Data;
                    }
                    map = new MapleMap(mapid, ChannelId, MapleDataTool.GetInt("info/returnMap", mapData), monsterRate);
                    map.OnFirstUserEnter=  MapleDataTool.GetString(mapData.GetChildByPath("info/onFirstUserEnter"), mapid.ToString());
                    map.OnUserEnter= MapleDataTool.GetString(mapData.GetChildByPath("info/onUserEnter"), mapid.ToString());
                    map.TimeMobId=  MapleDataTool.GetInt(mapData.GetChildByPath("info/timeMob/id"), -1);
                    map.TimeMobMessage= MapleDataTool.GetString(mapData.GetChildByPath("info/timeMob/message"), "");

                    PortalFactory portalFactory = new PortalFactory();
                    foreach (IMapleData portal in mapData.GetChildByPath("portal"))
                    {
                        var portalresult = portalFactory.MakePortal((PortalType)MapleDataTool.GetInt(portal.GetChildByPath("pt")), portal);
                        map.Portals.Add(portalresult.PortalId,portalresult);
                    }
                    List<MapleFoothold> allFootholds = new List<MapleFoothold>();

                    Point lBound = new Point();
                    Point uBound = new Point();
                    foreach (IMapleData footRoot in mapData.GetChildByPath("foothold"))
                    {
                        foreach (IMapleData footCat in footRoot)
                        {
                            foreach (IMapleData footHold in footCat)
                            {
                                int x1 = MapleDataTool.GetInt(footHold.GetChildByPath("x1"));
                                int y1 = MapleDataTool.GetInt(footHold.GetChildByPath("y1"));
                                int x2 = MapleDataTool.GetInt(footHold.GetChildByPath("x2"));
                                int y2 = MapleDataTool.GetInt(footHold.GetChildByPath("y2"));
                                MapleFoothold fh = new MapleFoothold(new Point(x1, y1), new Point(x2, y2), int.Parse(footHold.Name));
                                fh.PrevFootholdId = MapleDataTool.GetInt(footHold.GetChildByPath("prev"));
                                fh.NextFootholdId = MapleDataTool.GetInt(footHold.GetChildByPath("next"));

                                if (fh.Point1.X < lBound.X)
                                {
                                    lBound.X = fh.Point1.X;
                                }
                                if (fh.Point2.X > uBound.X)
                                {
                                    uBound.X = fh.Point2.X;
                                }
                                if (fh.Point1.Y < lBound.Y)
                                {
                                    lBound.Y = fh.Point1.Y;
                                }
                                if (fh.Point2.Y > uBound.Y)
                                {
                                    uBound.Y = fh.Point2.Y;
                                }
                                allFootholds.Add(fh);
                            }
                        }
                    }

                    MapleFootholdTree fTree = new MapleFootholdTree(lBound, uBound);
                    foreach (MapleFoothold fh in allFootholds)
                    {
                        fTree.Insert(fh);
                    }
                    map.Footholds= fTree ;

                    // load areas (EG PQ platforms)
                    if (mapData.GetChildByPath("area") != null)
                    {
                        foreach (IMapleData area in mapData.GetChildByPath("area"))
                        {
                            int x1 = MapleDataTool.GetInt(area.GetChildByPath("x1"));
                            int y1 = MapleDataTool.GetInt(area.GetChildByPath("y1"));
                            int x2 = MapleDataTool.GetInt(area.GetChildByPath("x2"));
                            int y2 = MapleDataTool.GetInt(area.GetChildByPath("y2"));
                            Rectangle mapArea = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                            map.Areas.Add(mapArea);
                        }
                    }

                    // load life data (npc, monsters)
                    foreach (IMapleData life in mapData.GetChildByPath("life"))
                    {
                        string id = MapleDataTool.GetString(life.GetChildByPath("id"));
                        string type = MapleDataTool.GetString(life.GetChildByPath("type"));
                        AbstractLoadedMapleLife myLife = loadLife(life, id, type);
                        var mapleMonster = myLife as MapleMonster;
                        if (mapleMonster != null)
                        {
                            MapleMonster monster = mapleMonster;
                            int mobTime = MapleDataTool.GetInt("mobTime", life, 0);
                            if (mobTime == -1)
                            { 
                                //does not respawn, force spawn once
                               map.spawnMonster(monster);
                            }
                            else {
                               map.addMonsterSpawn(monster, mobTime);
                            }
                        }
                        else {
                            map.AddMapObject(myLife);
                        }
                    }

                    //load reactor data
                    if (mapData.GetChildByPath("reactor") != null)
                    {
                        foreach (IMapleData reactor in mapData.GetChildByPath("reactor"))
                        {
                            string id = MapleDataTool.GetString(reactor.GetChildByPath("id"));
                            if (id != null)
                            {
                                MapleReactor newReactor = LoadReactor(reactor, id);
                                map.SpawnReactor(newReactor);
                            }
                        }
                    }

                    //try
                    //{
                        map.MapName= MapleDataTool.GetString("mapName", _mNameData.GetChildByPath(GetMapStringName(omapid)), "");
                        map.StreetName= MapleDataTool.GetString("streetName", _mNameData.GetChildByPath(GetMapStringName(omapid)), "");
                    //}
                    //catch 
                    //{
                        //map.MapName = "";
                        //map.StreetName = "";
                    //}

                    map.Clock = mapData.GetChildByPath("clock") != null;
                    map.Everlast= mapData.GetChildByPath("everlast") != null;
                    map.Town = mapData.GetChildByPath("town") != null;
                    map.AllowShops= MapleDataTool.GetInt(mapData.GetChildByPath("info/personalShop"), 0) == 1;
                    map.DecHp= MapleDataTool.ConvertToInt("decHP", mapData, 0);
                    map.ProtectItem= MapleDataTool.ConvertToInt("protectItem", mapData, 0);
                    map.ForcedReturnMapId = MapleDataTool.GetInt(mapData.GetChildByPath("info/forcedReturn"), 999999999);
                    map.OnFirstUserEnter= MapleDataTool.GetString(mapData.GetChildByPath("info/onFirstUserEnter"), "");
                    map.OnUserEnter = MapleDataTool.GetString(mapData.GetChildByPath("info/onUserEnter"), "");

                    map.HasBoat = mapData.GetChildByPath("shipObj") != null;


                    map.FieldLimit= MapleDataTool.GetInt(mapData.GetChildByPath("info/fieldLimit"), 0);
                    map.TimeLimit= MapleDataTool.ConvertToInt("timeLimit", mapData.GetChildByPath("info"), -1);
                    map.FieldType = MapleDataTool.ConvertToInt("info/fieldType", mapData, 0);
                    Maps.Add(omapid, map);

                    //try
                    //{
                    //    PreparedStatement ps = DatabaseConnection.getConnection().prepareStatement("SELECT * FROM spawns WHERE mid = ?");
                    //    ps.setInt(1, omapid);
                    //    ResultSet rs = ps.executeQuery();
                    //    while (rs.next())
                    //    {
                    //        int id = rs.getInt("idd");
                    //        int f = rs.getInt("f");
                    //        bool hide = false;
                    //        string type = rs.getString("type");
                    //        int fh = rs.getInt("fh");
                    //        int cy = rs.getInt("cy");
                    //        int rx0 = rs.getInt("rx0");
                    //        int rx1 = rs.getInt("rx1");
                    //        int x = rs.getInt("x");
                    //        int y = rs.getInt("y");
                    //        int mobTime = rs.getInt("mobtime");
                    //        AbstractLoadedMapleLife myLife = loadLife(id, f, hide, fh, cy, rx0, rx1, x, y, type);
                    //        if (type.equals("n"))
                    //        {
                    //            map.addMapObject(myLife);
                    //        }
                    //        else if (type.equals("m"))
                    //        {
                    //            MapleMonster monster = (MapleMonster)myLife;
                    //            map.addMonsterSpawn(monster, mobTime);
                    //        }
                    //    }
                    //}
                    //catch (SQLException e)
                    //{
                    //    log.info(e.toString());
                    //}
                }
            }
            return map;
        }

        public bool IsMapLoaded(int mapId)
        {
            return Maps.ContainsKey(mapId);
        }

        private AbstractLoadedMapleLife loadLife(IMapleData life, string id, string type)
        {
            AbstractLoadedMapleLife myLife = MapleLifeFactory.GetLife(int.Parse(id), type);
            myLife.Cy = MapleDataTool.GetInt(life.GetChildByPath("cy"));
            IMapleData dF = life.GetChildByPath("f");

            if (dF != null)
            {
                myLife.F = MapleDataTool.GetInt(dF);
            }
            myLife.Fh = MapleDataTool.GetInt(life.GetChildByPath("fh"));
            myLife.Rx0 = MapleDataTool.GetInt(life.GetChildByPath("rx0"));
            myLife.Rx1 = MapleDataTool.GetInt(life.GetChildByPath("rx1"));
            int x = MapleDataTool.GetInt(life.GetChildByPath("x"));
            int y = MapleDataTool.GetInt(life.GetChildByPath("y"));
            myLife.Position = new Point(x, y);

            int hide = MapleDataTool.GetInt("hide", life, 0);
            if (hide == 1)
            {
                myLife.IsHide = true;
            }
            else if (hide > 1)
            {
                Console.WriteLine("Hide > 1 ({0})", hide);
            }
            return myLife;
        }

        private AbstractLoadedMapleLife loadLife(int id, int f, bool hide, int fh, int cy, int rx0, int rx1, int x, int y, string type)
        {
            AbstractLoadedMapleLife myLife = MapleLifeFactory.GetLife(id, type);
            myLife.Cy = cy;
            myLife.F = f;
            myLife.Fh = fh;
            myLife.Rx0 = rx0;
            myLife.Rx1 = rx1;
            myLife.Position = new Point(x, y);
            myLife.IsHide = hide;
            return myLife;
        }

        private MapleReactor LoadReactor(IMapleData reactor, string id)
        {
            MapleReactor myReactor = new MapleReactor(MapleReactorFactory.getReactor(int.Parse(id)), int.Parse(id));

            int x = MapleDataTool.GetInt(reactor.GetChildByPath("x"));
            int y = MapleDataTool.GetInt(reactor.GetChildByPath("y"));
            myReactor.Position = new Point(x, y);

            myReactor.Delay = MapleDataTool.GetInt(reactor.GetChildByPath("reactorTime"))*1000;
            myReactor.State = 0;
            myReactor.ReactorName = MapleDataTool.GetString(reactor.GetChildByPath("name"), "");

            return myReactor;
        }

        private string GetMapName(int mapid)
        {
            string mapName = mapid.ToString().PadLeft(9, '0');
            StringBuilder builder = new StringBuilder("Map/Map");
            int area = mapid / 100000000;
            builder.Append(area);
            builder.Append("/");
            builder.Append(mapName);
            builder.Append(".img");

            mapName = builder.ToString();
            return mapName;
        }

        private string GetMapStringName(int mapid)
        {
            StringBuilder builder = new StringBuilder();
            if (mapid < 100000000)
            {
                builder.Append("maple");
            }
            else if (mapid >= 100000000 && mapid < 200000000)
            {
                builder.Append("victoria");
            }
            else if (mapid >= 200000000 && mapid < 300000000)
            {
                builder.Append("ossyria");
            }
            else if (mapid >= 540000000 && mapid < 541010110)
            {
                builder.Append("singapore");
            }
            else if (mapid >= 600000000 && mapid < 620000000)
            {
                builder.Append("MasteriaGL");
            }
            else if (mapid >= 670000000 && mapid < 682000000)
            {
                builder.Append("weddingGL");
            }
            else if (mapid >= 682000000 && mapid < 683000000)
            {
                builder.Append("HalloweenGL");
            }
            else if (mapid >= 800000000 && mapid < 900000000)
            {
                builder.Append("jp");
            }
            else {
                builder.Append("etc");
            }
            builder.Append("/" + mapid);
            return builder.ToString();
        }
    }
}
