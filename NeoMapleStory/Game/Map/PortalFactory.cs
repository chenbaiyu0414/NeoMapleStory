using NeoMapleStory.Game.Data;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
    public class PortalFactory
    {
        private byte _mNextDoorPortal;

        public PortalFactory()
        {
            _mNextDoorPortal = 0x80;
        }

        public IMaplePortal MakePortal(PortalType type, IMapleData portal)
        {
            MapleGenericPortal ret = new MapleGenericPortal(type);
            LoadPortal(ret, portal);
            return ret;
        }

        private void LoadPortal(MapleGenericPortal myPortal, IMapleData portal)
        {
            myPortal.PortalName = MapleDataTool.GetString(portal.GetChildByPath("pn"));
            myPortal.TargetName = MapleDataTool.GetString(portal.GetChildByPath("tn"));
            myPortal.TargetMapId = MapleDataTool.GetInt(portal.GetChildByPath("tm"));
            int x = MapleDataTool.GetInt(portal.GetChildByPath("x"));
            int y = MapleDataTool.GetInt(portal.GetChildByPath("y"));
            myPortal.Position = new Point(x, y);
            string script = MapleDataTool.GetString("script", portal, null);
            if (script != null && script.Equals(""))
            {
                script = null;
            }
            myPortal.ScriptName = script;
            if (myPortal.Type == PortalType.DoorPortal)
            {
                myPortal.PortalId = _mNextDoorPortal;
                _mNextDoorPortal++;
            }
            else {
                myPortal.PortalId = byte.Parse(portal.Name);
            }
        }
    }
}
