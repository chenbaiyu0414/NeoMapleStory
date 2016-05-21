using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Mob;

namespace NeoMapleStory.Game.Life
{
    public class MobAttackInfoFactory
    {
        private static readonly Dictionary<Tuple<int, int>, MobAttackInfo> MobAttacks =
            new Dictionary<Tuple<int, int>, MobAttackInfo>();

        private static readonly IMapleDataProvider DataSource = MapleDataProviderFactory.GetDataProvider("Mob.wz");

        public static MobAttackInfo GetMobAttackInfo(MapleMonster mob, int attack)
        {
            MobAttackInfo ret;
            var key = Tuple.Create(mob.Id, attack);

            if (MobAttacks.TryGetValue(key, out ret))
            {
                return ret;
            }
            lock (MobAttacks)
            {
                if (!MobAttacks.TryGetValue(key, out ret))
                {
                    var mobData = DataSource.GetData((mob.Id + ".img").PadLeft(11, '0'));
                    if (mobData != null)
                    {
                        var linkedmob = MapleDataTool.GetString("link", mobData, "");

                        if (linkedmob != "")
                            mobData = DataSource.GetData((linkedmob + ".img").PadLeft(11, '0'));

                        var attackData = mobData.GetChildByPath($"attack{attack + 1}/info");
                        if (attackData != null)
                        {
                            var deadlyAttack = attackData.GetChildByPath("deadlyAttack");
                            var mpBurn = MapleDataTool.GetInt("mpBurn", attackData, 0);
                            var disease = MapleDataTool.GetInt("disease", attackData, 0);
                            var level = MapleDataTool.GetInt("level", attackData, 0);
                            var mpCon = MapleDataTool.GetInt("conMP", attackData, 0);
                            ret = new MobAttackInfo(mob.Id, attack)
                            {
                                IsDeadlyAttack = deadlyAttack != null,
                                MpBurn = mpBurn,
                                DiseaseSkill = (byte) disease,
                                DiseaseLevel = (byte) level,
                                MpCon = mpCon
                            };
                        }
                    }
                    if (MobAttacks.ContainsKey(key))
                        MobAttacks[key] = ret;
                    else
                        MobAttacks.Add(key, ret);
                }
                return ret;
            }
        }
    }
}