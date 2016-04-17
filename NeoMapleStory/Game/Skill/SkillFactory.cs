using NeoMapleStory.Game.Data;
using System.Collections.Generic;

namespace NeoMapleStory.Game.Skill
{
    public class SkillFactory
    {
        private static readonly Dictionary<int, ISkill> Skills = new Dictionary<int, ISkill>();
        private static readonly IMapleDataProvider Datasource = MapleDataProviderFactory.GetDataProvider("Skill.wz");
        private static readonly IMapleData StringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Skill.img");

        public static ISkill GetSkill(int id)
        {
            ISkill ret;
            if (Skills.TryGetValue(id, out ret))
            {
                return ret;
            }
            lock (Skills)
            { // see if someone else that's also synchronized has loaded the skill by now
                if (!Skills.TryGetValue(id, out ret))
                {
                    int job = id / 10000;
                    IMapleData skillroot = Datasource.GetData(job.ToString().PadLeft(3, '0') + ".img");
                    IMapleData skillData = skillroot.GetChildByPath("skill/" + id.ToString().PadLeft(7, '0'));
                    if (skillData != null)
                    {
                        ret = Skill.LoadFromData(id, skillData);
                    }
                    Skills.Add(id, ret);
                }
                return ret;
            }
        }

        public static string GetSkillName(int id)
        {
            string strId = id.ToString().PadLeft(7, '0');
            IMapleData skillroot = StringData.GetChildByPath(strId);
            if (skillroot != null)
            {
                return MapleDataTool.GetString(skillroot.GetChildByPath("name"), "");
            }
            return null;
        }
    }
}
