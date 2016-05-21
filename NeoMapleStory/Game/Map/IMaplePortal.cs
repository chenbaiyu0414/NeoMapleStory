using System.Drawing;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public enum PortalType
    {
        MapPortal = 2,
        DoorPortal = 6
    }

    public struct PortalStatus
    {
        public static bool Open { get; } = true;
        public static bool Closed { get; } = false;
    }

    public interface IMaplePortal
    {
        PortalType Type { get; }

        byte PortalId { get; }

        Point Position { get; }

        string PortalName { get; }

        string TargetName { get; }

        string ScriptName { get; set; }

        bool PortalStatus { get; set; }

        int TargetMapId { get; }

        bool PortalState { get; set; }

        void EnterPortal(MapleClient c);
    }
}