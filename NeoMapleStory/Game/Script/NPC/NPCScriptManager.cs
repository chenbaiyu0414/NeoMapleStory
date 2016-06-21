#define debug
using System;
using System.Collections.Generic;
using System.Linq;
using CSScriptLibrary;
using NeoMapleStory.Server;
using System.IO;

namespace NeoMapleStory.Game.Script.NPC
{
    public class NpcScriptManager
    {
        private readonly Dictionary<MapleClient, NpcConversationManager> m_cms =
            new Dictionary<MapleClient, NpcConversationManager>();

        private readonly Dictionary<MapleClient, object> m_scripts = new Dictionary<MapleClient, object>();

        private readonly Dictionary<MapleClient, int> m_status = new Dictionary<MapleClient, int>();

        public static NpcScriptManager Instance { get; } = new NpcScriptManager();

        public void Start(MapleClient c, int npcId)
        {
            lock (m_cms)
            {
                if (m_cms.ContainsKey(c))
                    return;
            }

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
                    m_status.Add(c, 0);

#if debug
                    CSScript.KeepCompilingHistory = false;
#else
                    CSScript.KeepCompilingHistory = true;
#endif
                    string scriptPath = $"Script//NPC//{npcId}.cs";

                    if (File.Exists(scriptPath))
                    {
                        dynamic script = CSScript.Load($"Script//NPC//{npcId}.cs").CreateObject("*");

                        if (m_scripts.ContainsKey(c))
                            m_scripts[c] = script;
                        else
                            m_scripts.Add(c, script);

                        script.Start(cm);
                    }
                    else
                    {
                        cm.SendOk($"脚本不存在或者脚本错误，请与管理员联系。\r\n我的ID：#b{npcId}#k");
                        cm.Close();
                    }
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
            lock (m_cms)
            {
                var cm = m_cms[c];
                var status = m_status[c];

                try
                {
                    dynamic ns;
                    if (m_scripts.TryGetValue(c, out ns))
                    {
                        if (isCountinue == 0xFF)
                        {
                            cm.Close();
                        }
                        else
                        {
                            if (status >= 0 && isCountinue == 0)
                            {
                                cm.Close();
                                return;
                            }
                            if (isCountinue == 1)
                            {
                                status++;
                            }
                            else
                            {
                                status--;
                            }

                            m_status[c] = status;
                            ns.Choice(status, hasInput, selection);
                        }
                    }
                }
                catch (Exception e)
                {
                    cm.Close();
                    Console.WriteLine(e);
                }
            }
        }

        public void Close(NpcConversationManager cm)
        {
            lock (m_cms)
            {
                var c = cm.Client;
                m_cms.Remove(c);
                m_scripts.Remove(c);
                m_status.Remove(c);
            }
        }

        public void Close(MapleClient c)
        {
            lock (m_cms)
            {
                NpcConversationManager npccm;
                if (m_cms.TryGetValue(c, out npccm))
                {
                    Close(npccm);
                }
            }
        }

        public NpcConversationManager GetCm(MapleClient c)
        {
            return m_cms.FirstOrDefault(x => x.Key == c).Value;
        }
    }
    //public class Script
    //{
    //    private NPCConversationManager cm;
    //    public void Start(NPCConversationManager cm)
    //    {
    //        this.cm = cm;
    //        Choice(0, 0);
    //    }
    //    public void Choice(int status, int selection)
    //    {
    //        switch (status)
    //        {
    //            case 0:
    //                cm.SendSimple("你看看#L0#选择1#l#L1#啦啦#l");
    //                break;
    //            case 1:
    //                switch(selection)
    //                {
    //                    case 0:
    //                        cm.SendOk("Good");
    //                        break;
    //                    case 1:
    //                        cm.SendOk("Nice");
    //                        break;
    //                }
    //                cm.Close();
    //                break;
    //        }
    //    }
    //}
}