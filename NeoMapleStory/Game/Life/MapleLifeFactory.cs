using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Mob;
using System;
using System.Collections.Generic;

namespace NeoMapleStory.Game.Life
{
     public class MapleLifeFactory
    {
        private static readonly IMapleDataProvider MData = MapleDataProviderFactory.GetDataProvider("Mob.wz");
        private static readonly IMapleDataProvider MStringDataWz = MapleDataProviderFactory.GetDataProvider("String.wz");
        private static readonly IMapleData MMobStringData = MStringDataWz.GetData("Mob.img");
        private static readonly IMapleData MNpcStringData = MStringDataWz.GetData("Npc.img");
        private static readonly Dictionary<int, MapleMonsterStats> MMonsterStats = new Dictionary<int, MapleMonsterStats>();

        public static AbstractLoadedMapleLife GetLife(int id, string type)
        {
            if (type.ToLower()== "n")
            {
                return GetNpc(id);
            }
            else if (type.ToLower()== "m")
            {
                return GetMonster(id);
            }
            else {
                Console.WriteLine("Unknown Life type: {0}", type);
                return null;
            }
        }

        public static MapleMonster GetMonster(int mid)
        {
            MapleMonsterStats stats;
            if (!MMonsterStats.TryGetValue(mid ,out stats))
            {
                IMapleData monsterData = MData.GetData((mid + ".img").PadLeft(11, '0'));
                if (monsterData == null)
                {
                    return null;
                }
                IMapleData monsterInfoData = monsterData.GetChildByPath("info");

                stats = new MapleMonsterStats();
                stats.Hp= MapleDataTool.ConvertToInt("maxHP", monsterInfoData);
                stats.Mp= MapleDataTool.ConvertToInt("maxMP", monsterInfoData, 0);
                stats.Exp= MapleDataTool.ConvertToInt("exp", monsterInfoData, 0);
                stats.Level=MapleDataTool.ConvertToInt("level", monsterInfoData);
                stats.RemoveAfter=MapleDataTool.ConvertToInt("removeAfter", monsterInfoData, 0);
                stats.IsBoss= MapleDataTool.ConvertToInt("boss", monsterInfoData, 0) > 0;
                stats.IsFfaLoot = MapleDataTool.ConvertToInt("publicReward", monsterInfoData, 0) > 0;
                stats.IsUndead= MapleDataTool.ConvertToInt("undead", monsterInfoData, 0) > 0;
                stats.Name= MapleDataTool.GetString(mid + "/name", MMobStringData, "MISSINGNO");
                stats.BuffToGive= MapleDataTool.ConvertToInt("buff", monsterInfoData, -1);
                stats.IsExplosive= MapleDataTool.ConvertToInt("explosiveReward", monsterInfoData, 0) > 0;

                IMapleData firstAttackData = monsterInfoData.GetChildByPath("firstAttack");
                int firstAttack = 0;
                if (firstAttackData != null)
                {
                    if (firstAttackData.GetType() == MapleDataType.Float)
                    {
                        firstAttack = (int)Math.Round(MapleDataTool.GetFloat(firstAttackData));
                    }
                    else {
                        firstAttack = MapleDataTool.GetInt(firstAttackData);
                    }
                }

                stats.IsFirstAttack= firstAttack > 0 ;
                stats.DropPeriod= MapleDataTool.ConvertToInt("dropItemPeriod", monsterInfoData, 0);

                if (stats.IsBoss || mid == 8810018 || mid == 8810026)
                {
                    IMapleData hpTagColor = monsterInfoData.GetChildByPath("hpTagColor");
                    IMapleData hpTagBgColor = monsterInfoData.GetChildByPath("hpTagBgcolor");

                    if (hpTagBgColor == null || hpTagColor == null)
                    {
                       Console.WriteLine($"Monster { stats.Name } ({ mid }) flagged as boss without boss HP bars.");
                        stats.TagColor = 0;
                        stats.TagBgColor = 0;
                    }
                    else {
                        stats.TagColor=(byte) MapleDataTool.ConvertToInt("hpTagColor", monsterInfoData);
                        stats.TagBgColor=(byte)MapleDataTool.ConvertToInt("hpTagBgcolor", monsterInfoData);
                    }
                }

                foreach (IMapleData idata in monsterData)
                {
                    if (!idata.Name.Equals("info"))
                    {
                        int delay = 0;
                        idata.Children.ForEach(pic => delay += MapleDataTool.ConvertToInt("delay", pic, 0));
                        stats.SetAnimationTime(idata.Name, delay);
                    }
                }

                IMapleData reviveInfo = monsterInfoData.GetChildByPath("revive");
                if (reviveInfo != null)
                {
                    List<int> revives = new List<int>();
                    foreach (IMapleData data in reviveInfo)
                    {
                        revives.Add(MapleDataTool.GetInt(data));
                    }
                    stats.Revives = revives;
                }

                DecodeElementalString(stats, MapleDataTool.GetString("elemAttr", monsterInfoData, ""));

                IMapleData monsterSkillData = monsterInfoData.GetChildByPath("skill");
                if (monsterSkillData != null)
                {
                    int i = 0;
                    var skills = new List<Tuple<byte, byte>>();
                    while (monsterSkillData.GetChildByPath(i.ToString()) != null)
                    {
                        skills.Add(new Tuple<byte, byte>((byte)MapleDataTool.GetInt(i + "/skill", monsterSkillData, 0), (byte)MapleDataTool.GetInt(i + "/level", monsterSkillData, 0)));
                        i++;
                    }
                    stats.SetSkills(skills);
                }
                IMapleData banishData = monsterInfoData.GetChildByPath("ban");
                if (banishData != null)
                {
                    stats.Banish= new BanishInfo(MapleDataTool.GetString("banMsg", banishData), MapleDataTool.GetInt("banMap/0/field", banishData, -1), MapleDataTool.GetString("banMap/0/portal", banishData, "sp"));
                }
                MMonsterStats.Add(mid, stats);
            }
            MapleMonster ret = new MapleMonster(mid, stats);
            return ret;
        }

        public static void DecodeElementalString(MapleMonsterStats stats, string elemAttr)
        {
            for (int i = 0; i < elemAttr.Length; i += 2)
                stats.SetEffectiveness(Element.GetByChar(elemAttr[i]), ElementalEffectiveness.GetByNumber(elemAttr[i + 1]));
        }

        public static MapleNpc GetNpc(int nid)
        {
            return new MapleNpc(nid, new MapleNpcStats(MapleDataTool.GetString(nid + "/name", MNpcStringData, "MISSINGNO")));
        }
    }
}
