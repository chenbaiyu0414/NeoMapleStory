using System.Drawing;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Map
{
    public class PortalFactory
    {
        private byte m_mNextDoorPortal;

        public PortalFactory()
        {
            m_mNextDoorPortal = 0x80;
        }

        public IMaplePortal MakePortal(PortalType type, IMapleData portal)
        {
            var ret = new MapleGenericPortal(type);
            LoadPortal(ret, portal);
            return ret;
        }

        private void LoadPortal(MapleGenericPortal myPortal, IMapleData portal)
        {
            myPortal.PortalName = MapleDataTool.GetString(portal.GetChildByPath("pn"));
            myPortal.TargetName = MapleDataTool.GetString(portal.GetChildByPath("tn"));
            myPortal.TargetMapId = MapleDataTool.GetInt(portal.GetChildByPath("tm"));
            var x = MapleDataTool.GetInt(portal.GetChildByPath("x"));
            var y = MapleDataTool.GetInt(portal.GetChildByPath("y"));
            myPortal.Position = new Point(x, y);
            var script = MapleDataTool.GetString("script", portal, null);
            if (script != null && script.Equals(""))
            {
                script = null;
            }
            myPortal.ScriptName = script;
            if (myPortal.Type == PortalType.DoorPortal)
            {
                myPortal.PortalId = m_mNextDoorPortal;
                m_mNextDoorPortal++;
            }
            else
            {
                myPortal.PortalId = byte.Parse(portal.Name);
            }
        }
    }
}