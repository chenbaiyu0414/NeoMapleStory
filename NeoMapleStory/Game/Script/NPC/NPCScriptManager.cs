using System;
using System.Collections.Generic;
using CSScriptLibrary;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.NPC
{
    public class NPCScriptManager:AbstractScriptManager
    {
        private Dictionary<MapleClient, NPCConversationManager> cms =
            new Dictionary<MapleClient, NPCConversationManager>();

        private Dictionary<MapleClient, object> scripts = new Dictionary<MapleClient,object>();

        public static NPCScriptManager Instance { get; } = new NPCScriptManager();

        public void Start(MapleClient c, int npcId)
        {

            if (c.Character.GmLevel > 0)
            {
                c.Character.DropMessage($"与NPC:{npcId}成功建立对话!");
            }

            NPCConversationManager cm = new NPCConversationManager(c, npcId);

            if (cms.ContainsKey(c))
            {
                return;
            }

            cms.Add(c, cm);

            string code = GetScriptCode(ScriptType.NPC, npcId, c, cm);

            if (code == null || Instance == null)
            {
                cm.SendOk($"#b抱歉，我现在不能为您提供服务！#k\r\nNPC ID: { npcId }");
                cm.Close();
                return;
            }

            dynamic script = CSScript.LoadCode(code).CreateObject("*");
            script.Start(cm);

            Console.WriteLine(script);

            if (scripts.ContainsKey(c))
                scripts[c] = script;
            else
                scripts.Add(c, script);

            script.Start(cm);

            //log.info("发生了错误的Npc脚本运行,NPC ID： " + npcId);
            //dispose(c);
            //this.cms.remove(c);

        }

        //public void Start(string filename, MapleClient c, int npc, List<MaplePartyCharacter> chars)
        //{ 
        //    // CPQ start
        //    try
        //    {
        //        NPCConversationManager cm = new NPCConversationManager(c, npc, chars, 0);
        //        cm.Close();
        //        if (cms.containsKey(c))
        //        {
        //            return;
        //        }
        //        cms.put(c, cm);
        //        Invocable iv = getInvocable("npc/" + filename + ".js", c);
        //        NPCScriptManager npcsm = NPCScriptManager.getInstance();
        //        if (iv == null || NPCScriptManager.getInstance() == null || npcsm == null)
        //        {
        //            cm.Close();
        //            return;
        //        }
        //        engine.put("cm", cm);
        //        // NPCScript ns = iv.getInterface(NPCScript.class);
        //        scripts.put(c, ns);
        //        ns.start(chars);
        //    }
        //    catch (Exception e)
        //    {
        //        log.error("NPC脚本错误执行 " + filename);
        //        dispose(c);
        //        cms.remove(c);

        //    }
        //}

        //public void Start(MapleClient c, byte mode, byte type, int selection)
        //{
        //    INpcScript ns; 
        //    if (scripts.TryGetValue(c,out ns))
        //    {
        //        ns.Start(mode, type, selection);
        //    }
        //}

        public void Close(NPCConversationManager cm)
        {
            MapleClient c = cm.Client;
            cms.Remove(c);
            scripts.Remove(c);
            resetContext(ScriptType.NPC, cm.NpcId, c);
        }

        public void Close(MapleClient c)
        {
            NPCConversationManager npccm;
            if (cms.TryGetValue(c, out npccm))
            {
                Close(npccm);
            }
        }

        //public NPCConversationManager getCM(MapleClient c)
        //{
        //    return cms.get(c);
        //}
    }
}
