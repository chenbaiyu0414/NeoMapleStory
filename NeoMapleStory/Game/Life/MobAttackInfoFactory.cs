using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Mob;

namespace NeoMapleStory.Game.Life
{
    public class MobAttackInfoFactory
    {
        private static Dictionary<Tuple<int, int>, MobAttackInfo> mobAttacks = new Dictionary<Tuple<int, int>, MobAttackInfo>();
        private static IMapleDataProvider dataSource = MapleDataProviderFactory.GetDataProvider("Mob.wz");

        public static MobAttackInfo GetMobAttackInfo(MapleMonster mob, int attack)
        {
            MobAttackInfo ret;
            var key = Tuple.Create(mob.Id, attack);

            if (mobAttacks.TryGetValue(key, out ret))
            {
                return ret;
            }
            lock (mobAttacks)
            {
                if (!mobAttacks.TryGetValue(key, out ret))
                {
                    var mobData = dataSource.GetData((mob.Id + ".img").PadLeft(11, '0'));
                    if (mobData != null)
                    {
                        string linkedmob = MapleDataTool.GetString("link", mobData, "");

                        if (linkedmob != "")
                            mobData = dataSource.GetData((linkedmob + ".img").PadLeft(11, '0'));

                        var attackData = mobData.GetChildByPath($"attack{attack + 1}/info");
                        if (attackData != null)
                        {
                            var deadlyAttack = attackData.GetChildByPath("deadlyAttack");
                            int mpBurn = MapleDataTool.GetInt("mpBurn", attackData, 0);
                            int disease = MapleDataTool.GetInt("disease", attackData, 0);
                            int level = MapleDataTool.GetInt("level", attackData, 0);
                            int mpCon = MapleDataTool.GetInt("conMP", attackData, 0);
                            ret = new MobAttackInfo(mob.Id, attack)
                            {
                                IsDeadlyAttack = deadlyAttack != null,
                                MpBurn = mpBurn,
                                DiseaseSkill = (byte)disease,
                                DiseaseLevel = (byte)level,
                                MpCon = mpCon
                            };
                        }
                    }             
                    if (mobAttacks.ContainsKey(key))
                        mobAttacks[key] = ret;
                    else
                        mobAttacks.Add(key, ret);
                }
                return ret;
            }
        }
    }
}
