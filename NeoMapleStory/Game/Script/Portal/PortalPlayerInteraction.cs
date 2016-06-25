using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.Portal
{
    public class PortalPlayerInteraction : AbstractPlayerInteraction
    {
        public IMaplePortal Portal { get; }

        public PortalPlayerInteraction(MapleClient c,IMaplePortal portal) : base(c)
        {
            Portal = portal;
        }

        public void CreateMapMonitor(int mapId, bool closePortal, int reactorMap, int reactor)
        {
            if (closePortal)
            {
                Portal.PortalStatus = PortalStatus.Closed;
            }
            if (reactor > -1)
            {
                var r = Client.ChannelServer.MapFactory.GetMap(reactorMap).GetReactorByOid(reactor);
                r.State = 1;
                Client.ChannelServer.MapFactory.GetMap(reactorMap).BroadcastMessage(PacketCreator.TriggerReactor(r, 1));
            }
            //new MapMonitor(c.getChannelServer().getMapFactory().getMap(mapId), closePortal ? portal : null, c.getChannel(), r);
        }

        public bool IsMonster(IMapleMapObject o) => o.GetType() == MapleMapObjectType.Monster;

        public void BlockPortal()=> Player.BlockPortal(Portal.ScriptName);

        public void UnblockPortal()=> Player.UnblockPortal(Portal.ScriptName);

    }
}
