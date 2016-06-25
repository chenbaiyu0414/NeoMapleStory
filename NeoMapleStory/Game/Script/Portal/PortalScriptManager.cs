using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSScriptLibrary;
using NeoMapleStory.Server;
using NeoMapleStory.Game.Map;

namespace NeoMapleStory.Game.Script.Portal
{
    public class PortalScriptManager
    {
        public static PortalScriptManager Instance => new PortalScriptManager();
        private readonly Dictionary<string, IPortalScript> m_scripts = new Dictionary<string, IPortalScript>();

        private IPortalScript GetPortalScript(string scriptName,MapleClient client)
        {
            if (m_scripts.ContainsKey(scriptName))
            {
                return m_scripts[scriptName];
            }

            var scriptPath= $"Script//Portal//{scriptName}.cs";
            if (!File.Exists(scriptPath))
            {
                if (m_scripts.ContainsKey(scriptName))
                    m_scripts[scriptName] = null;
                else
                    m_scripts.Add(scriptName, null);

                return null;
            }

            try
            {
                IPortalScript script = CSScript.Load(scriptPath).CreateObject("*").AlignToInterface<IPortalScript>();
                if (m_scripts.ContainsKey(scriptName))
                    m_scripts[scriptName] = script;
                else
                    m_scripts.Add(scriptName, script);

                return script;
            }
            catch(Exception e)
            {
                if (client.Player.IsGm)
                {
                    client.Player.DropMessage(Packet.PacketCreator.ServerMessageType.LightBlueText, $"执行传送门脚本:{scriptName}时出错");
                }
                Console.WriteLine(e);
            }
            return null;
        }

        public bool Execute(IMaplePortal portal, MapleClient c)
        {
            var script = GetPortalScript(portal.ScriptName,c);

            if (script != null && !c.Player.BlockedPortals.Contains(portal.ScriptName))
            {
                return script.Enter(new PortalPlayerInteraction(c, portal));
            }
            return false;
        }

        public void Clear()=> m_scripts.Clear();
    }
}
