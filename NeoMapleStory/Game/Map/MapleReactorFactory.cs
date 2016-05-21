using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Map
{
    public class MapleReactorFactory
    {
        private static readonly IMapleDataProvider Data = MapleDataProviderFactory.GetDataProvider("Reactor.wz");

        private static readonly Dictionary<int, MapleReactorStats> ReactorStats =
            new Dictionary<int, MapleReactorStats>();

        public static MapleReactorStats GetReactor(int rid)
        {
            MapleReactorStats stats;
            if (!ReactorStats.TryGetValue(rid, out stats))
            {
                var infoId = rid;
                var reactorData = Data.GetData((infoId + ".img").PadLeft(11, '0'));
                var link = reactorData.GetChildByPath("info/link");
                if (link != null)
                {
                    infoId = MapleDataTool.ConvertToInt("info/link", reactorData);
                    stats = ReactorStats[infoId];
                }
                var activateOnTouch = reactorData.GetChildByPath("info/activateByTouch");
                var loadArea = false;
                if (activateOnTouch != null)
                    loadArea = MapleDataTool.GetInt("info/activateByTouch", reactorData, 0) != 0;
                if (stats == null)
                {
                    reactorData = Data.GetData((infoId + ".img").PadLeft(11, '0'));
                    var reactorInfoData = reactorData.GetChildByPath("0/event/0");
                    stats = new MapleReactorStats();
                    if (reactorInfoData != null)
                    {
                        var areaSet = false;
                        var i = 0;
                        while (reactorInfoData != null)
                        {
                            Tuple<int, int> reactItem = null;
                            var type = MapleDataTool.ConvertToInt("type", reactorInfoData);
                            if (type == 100)
                            {
                                //reactor waits for item
                                reactItem = Tuple.Create(MapleDataTool.ConvertToInt("0", reactorInfoData),
                                    MapleDataTool.ConvertToInt("1", reactorInfoData));
                                if (!areaSet || loadArea)
                                {
                                    //only set area of effect for item-triggered reactors once
                                    stats.Tl = MapleDataTool.GetPoint("lt", reactorInfoData);
                                    stats.Br = MapleDataTool.GetPoint("rb", reactorInfoData);
                                    areaSet = true;
                                }
                            }
                            var nextState = (byte) MapleDataTool.ConvertToInt("state", reactorInfoData);
                            stats.AddState((byte) i, type, reactItem, nextState);
                            i++;
                            reactorInfoData = reactorData.GetChildByPath(i + "/event/0");
                        }
                    }
                    else
                    {
                        //sit there and look pretty; likely a reactor such as Zakum/Papulatus doors that shows if player can enter
                        stats.AddState(0, 999, null, 0);
                    }

                    if (ReactorStats.ContainsKey(infoId))
                        ReactorStats[infoId] = stats;
                    else
                        ReactorStats.Add(infoId, stats);

                    if (rid != infoId)
                    {
                        if (ReactorStats.ContainsKey(rid))
                            ReactorStats[rid] = stats;
                        else
                            ReactorStats.Add(rid, stats);
                    }
                }
                else
                {
                    // stats exist at infoId but not rid; add to map
                    if (ReactorStats.ContainsKey(rid))
                        ReactorStats[rid] = stats;
                    else
                        ReactorStats.Add(rid, stats);
                }
            }
            return stats;
        }
    }
}