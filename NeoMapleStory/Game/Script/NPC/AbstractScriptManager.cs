using System.IO;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.NPC
{
    public abstract class AbstractScriptManager
    {
        public enum ScriptType
        {
            NPC
        }

        protected string GetScriptCode(ScriptType scriptType, int scriptId, MapleClient c, NPCConversationManager cm)
        {
            string scriptPath = $"Script//{scriptType}//{scriptId}.cs";

            if (!File.Exists(scriptPath))
                return null;

            return File.ReadAllText(scriptPath);
        }

        protected void resetContext(ScriptType scriptType, int scriptId, MapleClient c)
        {
            string scriptPath = $"Script//{scriptType.ToString()}//{scriptId}.cs";
            //c.removeScriptEngine(path);
        }
    }
}
