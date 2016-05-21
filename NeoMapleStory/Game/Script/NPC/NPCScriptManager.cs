using System;
using System.Collections.Generic;
using System.Linq;
using CSScriptLibrary;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.NPC
{
    public class NpcScriptManager
    {
        private readonly Dictionary<MapleClient, NpcConversationManager> m_cms =
            new Dictionary<MapleClient, NpcConversationManager>();

        private readonly Dictionary<MapleClient, object> m_scripts = new Dictionary<MapleClient, object>();

        public static NpcScriptManager Instance { get; } = new NpcScriptManager();

        public void Start(MapleClient c, int npcId)
        {
            if (m_cms.ContainsKey(c))
                return;

            var cm = new NpcConversationManager(c, npcId);

            try
            {
                lock (m_cms)
                {
                    if (c.Player.GmLevel > 0)
                    {
                        c.Player.DropMessage($"与NPC:{npcId}成功建立对话!");
                    }

                    m_cms.Add(c, cm);

                    dynamic script = CSScript.Load($"Script//NPC//{npcId}.cs").CreateObject("*");

                    if (m_scripts.ContainsKey(c))
                        m_scripts[c] = script;
                    else
                        m_scripts.Add(c, script);

                    script.Start(cm);
                }
            }
            catch (Exception e)
            {
                cm.SendOk($"脚本不存在或者脚本错误，请与管理员联系。\r\n我的ID：#b{npcId}#k");
                cm.Close();
                Console.WriteLine(e);
            }
        }

        public void Choice(MapleClient c, byte isCountinue, byte hasInput, int selection)
        {
            try
            {
                lock (m_cms)
                {
                    dynamic ns;
                    if (m_scripts.TryGetValue(c, out ns))
                    {
                        ns.Choice(isCountinue, hasInput, selection);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Close(NpcConversationManager cm)
        {
            lock (m_cms)
            {
                var c = cm.Client;
                m_cms.Remove(c);
                m_scripts.Remove(c);
            }
        }

        public void Close(MapleClient c)
        {
            NpcConversationManager npccm;
            if (m_cms.TryGetValue(c, out npccm))
            {
                Close(npccm);
            }
        }

        public NpcConversationManager GetCm(MapleClient c)
        {
            return m_cms.FirstOrDefault(x => x.Key == c).Value;
        }
    }
}