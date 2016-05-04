using System;
using System.Collections.Generic;
using System.Drawing;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Mob
{
    public class MobSkillFactory
    {
        private static Dictionary<Tuple<byte, byte>, MobSkill> mobSkills = new Dictionary<Tuple<byte, byte>, MobSkill>();
        private static IMapleDataProvider dataSource = MapleDataProviderFactory.GetDataProvider("Skill.wz");
        private static IMapleData skillRoot = dataSource.GetData("MobSkill.img");

        public static MobSkill getMobSkill(byte skillId, byte level)
        {
            MobSkill ret;
            if (mobSkills.TryGetValue(Tuple.Create(skillId, level), out ret))
            {
                return ret;
            }
            lock (mobSkills)
            {
                if (!mobSkills.TryGetValue(Tuple.Create(skillId, level), out ret))
                {
                    var skillData = skillRoot.GetChildByPath(skillId + "/level/" + level);
                    if (skillData != null)
                    {
                        int mpCon = MapleDataTool.GetInt(skillData.GetChildByPath("mpCon"), 0);
                        List<int> toSummon = new List<int>();
                        for (int i = 0; i > -1; i++)
                        {
                            if (skillData.GetChildByPath(i.ToString()) == null)
                            {
                                break;
                            }
                            toSummon.Add(MapleDataTool.GetInt(skillData.GetChildByPath(i.ToString()), 0));
                        }
                        int effect = MapleDataTool.GetInt("summonEffect", skillData, 0);
                        int hp = MapleDataTool.GetInt("hp", skillData, 100);
                        int x = MapleDataTool.GetInt("x", skillData, 1);
                        int y = MapleDataTool.GetInt("y", skillData, 1);
                        int duration = MapleDataTool.GetInt("time", skillData, 0);
                        int cooltime = MapleDataTool.GetInt("interval", skillData, 0);
                        int iprop = MapleDataTool.GetInt("prop", skillData, 100);
                        float prop = (float)iprop / 100;
                        int limit = MapleDataTool.GetInt("limit", skillData, 0);
                        int count = MapleDataTool.GetInt("count", skillData, 1);
                        var ltd = skillData.GetChildByPath("lt");
                        var rtd = skillData.GetChildByPath("rb");
                        Point lt = Point.Empty;
                        Point rb = Point.Empty;
                        if (ltd != null && rtd != null)
                        {
                            lt = (Point)ltd.Data;
                            rb = (Point)rtd.Data;
                        }
                        ret = new MobSkill(skillId, level);
                        ret.toSummon.AddRange(toSummon);
                        ret.cooltime = cooltime * 1000;
                        ret.duration = duration * 1000;
                        ret.hp = hp;
                        ret.mpCon = mpCon;
                        ret.spawnEffect = effect;
                        ret.x = x;
                        ret.y = y;
                        ret.prop = prop;
                        ret.limit = limit;
                        ret.SetLtRb(lt, rb);
                        ret.count = count;
                    }
                    var key = Tuple.Create(skillId, level);
                    if (mobSkills.ContainsKey(key))
                        mobSkills[key] = ret;
                    else
                        mobSkills.Add(key, ret);
                }
                return ret;
            }
        }
    }
}
