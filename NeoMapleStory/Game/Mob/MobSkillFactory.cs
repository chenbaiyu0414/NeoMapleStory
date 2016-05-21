using System;
using System.Collections.Generic;
using System.Drawing;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Mob
{
    public class MobSkillFactory
    {
        private static readonly Dictionary<Tuple<byte, byte>, MobSkill> MobSkills =
            new Dictionary<Tuple<byte, byte>, MobSkill>();

        private static readonly IMapleDataProvider DataSource = MapleDataProviderFactory.GetDataProvider("Skill.wz");
        private static readonly IMapleData SkillRoot = DataSource.GetData("MobSkill.img");

        public static MobSkill GetMobSkill(byte skillId, byte level)
        {
            MobSkill ret;
            if (MobSkills.TryGetValue(Tuple.Create(skillId, level), out ret))
            {
                return ret;
            }
            lock (MobSkills)
            {
                if (!MobSkills.TryGetValue(Tuple.Create(skillId, level), out ret))
                {
                    var skillData = SkillRoot.GetChildByPath(skillId + "/level/" + level);
                    if (skillData != null)
                    {
                        var mpCon = MapleDataTool.GetInt(skillData.GetChildByPath("mpCon"), 0);
                        var toSummon = new List<int>();
                        for (var i = 0; i > -1; i++)
                        {
                            if (skillData.GetChildByPath(i.ToString()) == null)
                            {
                                break;
                            }
                            toSummon.Add(MapleDataTool.GetInt(skillData.GetChildByPath(i.ToString()), 0));
                        }
                        var effect = MapleDataTool.GetInt("summonEffect", skillData, 0);
                        var hp = MapleDataTool.GetInt("hp", skillData, 100);
                        var x = MapleDataTool.GetInt("x", skillData, 1);
                        var y = MapleDataTool.GetInt("y", skillData, 1);
                        var duration = MapleDataTool.GetInt("time", skillData, 0);
                        var cooltime = MapleDataTool.GetInt("interval", skillData, 0);
                        var iprop = MapleDataTool.GetInt("prop", skillData, 100);
                        var prop = (float) iprop/100;
                        var limit = MapleDataTool.GetInt("limit", skillData, 0);
                        var count = MapleDataTool.GetInt("count", skillData, 1);
                        var ltd = skillData.GetChildByPath("lt");
                        var rtd = skillData.GetChildByPath("rb");
                        var lt = Point.Empty;
                        var rb = Point.Empty;
                        if (ltd != null && rtd != null)
                        {
                            lt = (Point) ltd.Data;
                            rb = (Point) rtd.Data;
                        }
                        ret = new MobSkill(skillId, level);
                        ret.ToSummon.AddRange(toSummon);
                        ret.Cooltime = cooltime*1000;
                        ret.Duration = duration*1000;
                        ret.Hp = hp;
                        ret.MpCon = mpCon;
                        ret.SpawnEffect = effect;
                        ret.X = x;
                        ret.Y = y;
                        ret.Prop = prop;
                        ret.Limit = limit;
                        ret.SetLtRb(lt, rb);
                        ret.Count = count;
                    }
                    var key = Tuple.Create(skillId, level);
                    if (MobSkills.ContainsKey(key))
                        MobSkills[key] = ret;
                    else
                        MobSkills.Add(key, ret);
                }
                return ret;
            }
        }
    }
}