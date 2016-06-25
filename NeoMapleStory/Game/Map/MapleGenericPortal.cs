using System;
using System.Drawing;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Script.Portal;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleGenericPortal : IMaplePortal
    {
        public MapleGenericPortal(PortalType type)
        {
            Type = type;
        }

        public PortalType Type { get; }
        public byte PortalId { get; set; }
        public Point Position { get; set; }
        public string PortalName { get; set; }
        public string TargetName { get; set; }
        public string ScriptName { get; set; }
        public bool PortalStatus { get; set; }
        public int TargetMapId { get; set; }
        public bool PortalState { get; set; }

        public void EnterPortal(MapleClient c)
        {
            var player = c.Player;

            var distanceSq = Position.DistanceSquare(player.Position);
            if (distanceSq > 22500)
            {
                player.AntiCheatTracker.RegisterOffense(CheatingOffense.UsingFarawayPortal, "D" + Math.Sqrt(distanceSq));
            }

            var changed = false;
            if (ScriptName != null)
            {
                //if (!FourthJobQuestsPortalHandler.handlePortal(ScriptName, player))
                //{
                changed = PortalScriptManager.Instance.Execute(this, c);
                //}
            }
            else if (TargetMapId != 999999999)
            {
                MapleMap to;
                //if (player.getEventInstance() == null)
                //{
                to = c.ChannelServer.MapFactory.GetMap(TargetMapId);
                //}
                //else {
                //    to = player.getEventInstance().getMapInstance(TargetMapID);
                //}
                var pto = to.GetPortal(TargetName);
                if (pto == null)
                {
                    // fallback for missing portals - no real life case anymore - intresting for not implemented areas
                    pto = to.GetPortal(0);
                }
                player.ChangeMap(to, pto);
                    //late resolving makes this harder but prevents us from loading the whole world at once
                changed = true;
            }
            if (!changed)
            {
                c.Send(PacketCreator.EnableActions());
            }
        }
    }
}