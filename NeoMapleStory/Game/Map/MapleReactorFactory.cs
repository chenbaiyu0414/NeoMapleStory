using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Map
{
    public class MapleReactorFactory
    {
        private static IMapleDataProvider data = MapleDataProviderFactory.GetDataProvider("Reactor.wz");
        private static Dictionary<int, MapleReactorStats> reactorStats = new Dictionary<int, MapleReactorStats>();

        public static MapleReactorStats getReactor(int rid)
        {
            MapleReactorStats stats;
            if (!reactorStats.TryGetValue(rid, out stats))
            {
                int infoId = rid;
                IMapleData reactorData = data.GetData((infoId + ".img").PadLeft(11, '0'));
                IMapleData link = reactorData.GetChildByPath("info/link");
                if (link != null)
                {
                    infoId = MapleDataTool.ConvertToInt("info/link", reactorData);
                    stats = reactorStats[infoId];
                }
                IMapleData activateOnTouch = reactorData.GetChildByPath("info/activateByTouch");
                bool loadArea = false;
                if (activateOnTouch != null)
                    loadArea = MapleDataTool.GetInt("info/activateByTouch", reactorData, 0) != 0;
                if (stats == null)
                {
                    reactorData = data.GetData((infoId + ".img").PadLeft(11, '0'));
                    IMapleData reactorInfoData = reactorData.GetChildByPath("0/event/0");
                    stats = new MapleReactorStats();
                    if (reactorInfoData != null)
                    {
                        bool areaSet = false;
                        int i = 0;
                        while (reactorInfoData != null)
                        {
                            Tuple<int, int> reactItem = null;
                            int type = MapleDataTool.ConvertToInt("type", reactorInfoData);
                            if (type == 100)
                            { //reactor waits for item
                                reactItem = Tuple.Create(MapleDataTool.ConvertToInt("0", reactorInfoData), MapleDataTool.ConvertToInt("1", reactorInfoData));
                                if (!areaSet || loadArea)
                                { //only set area of effect for item-triggered reactors once
                                    stats.Tl = MapleDataTool.GetPoint("lt", reactorInfoData);
                                    stats.Br = MapleDataTool.GetPoint("rb", reactorInfoData);
                                    areaSet = true;
                                }
                            }
                            byte nextState = (byte)MapleDataTool.ConvertToInt("state", reactorInfoData);
                            stats.AddState((byte)i, type, reactItem, nextState);
                            i++;
                            reactorInfoData = reactorData.GetChildByPath(i + "/event/0");
                        }
                    }
                    else
                    { //sit there and look pretty; likely a reactor such as Zakum/Papulatus doors that shows if player can enter
                        stats.AddState(0, 999, null, 0);
                    }

                    if (reactorStats.ContainsKey(infoId))
                        reactorStats[infoId] = stats;
                    else
                        reactorStats.Add(infoId, stats);

                    if (rid != infoId)
                    {
                        if (reactorStats.ContainsKey(rid))
                            reactorStats[rid] = stats;
                        else
                            reactorStats.Add(rid, stats);
                    }
                }
                else
                { 
                    // stats exist at infoId but not rid; add to map
                    if (reactorStats.ContainsKey(rid))
                        reactorStats[rid] = stats;
                    else
                        reactorStats.Add(rid, stats);
                }
            }
            return stats;
        }
    }
}
