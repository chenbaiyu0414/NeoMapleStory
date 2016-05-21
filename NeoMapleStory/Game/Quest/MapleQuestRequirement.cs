using System;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;

namespace NeoMapleStory.Game.Quest
{
    public enum MapleQuestRequirementType
    {
        Undefined = -1,
        Job = 0,
        Item = 1,
        Quest = 2,
        MinLevel = 3,
        MaxLevel = 4,
        EndDate = 5,
        Mob = 6,
        Npc = 7,
        FieldEnter = 8,
        Interval = 9,
        Script = 10,
        Pet = 11,
        MinPetTameness = 12,
        MonsterBook = 13,
        CompleteQuest = 14
    }

    public static class MapleQuestRequirementTypeExtension
    {
        public static MapleQuestRequirementType GetByWzName(string name)
        {
            if (name.Equals("job"))
            {
                return MapleQuestRequirementType.Job;
            }
            if (name.Equals("quest"))
            {
                return MapleQuestRequirementType.Quest;
            }
            if (name.Equals("item"))
            {
                return MapleQuestRequirementType.Item;
            }
            if (name.Equals("lvmin"))
            {
                return MapleQuestRequirementType.MinLevel;
            }
            if (name.Equals("lvmax"))
            {
                return MapleQuestRequirementType.MaxLevel;
            }
            if (name.Equals("end"))
            {
                return MapleQuestRequirementType.EndDate;
            }
            if (name.Equals("mob"))
            {
                return MapleQuestRequirementType.Mob;
            }
            if (name.Equals("npc"))
            {
                return MapleQuestRequirementType.Npc;
            }
            if (name.Equals("fieldEnter"))
            {
                return MapleQuestRequirementType.FieldEnter;
            }
            if (name.Equals("interval"))
            {
                return MapleQuestRequirementType.Interval;
            }
            if (name.Equals("startscript"))
            {
                return MapleQuestRequirementType.Script;
            }
            if (name.Equals("endscript"))
            {
                return MapleQuestRequirementType.Script;
            }
            if (name.Equals("pet"))
            {
                return MapleQuestRequirementType.Pet;
            }
            if (name.Equals("pettamenessmin"))
            {
                return MapleQuestRequirementType.MinPetTameness;
            }
            if (name.Equals("mbmin"))
            {
                return MapleQuestRequirementType.MonsterBook;
            }
            if (name.Equals("questComplete"))
            {
                return MapleQuestRequirementType.CompleteQuest;
            }
            return MapleQuestRequirementType.Undefined;
        }
    }

    public class MapleQuestRequirement
    {
        private readonly MapleQuest m_mQuest;


        public MapleQuestRequirement(MapleQuest quest, MapleQuestRequirementType type, IMapleData data)
        {
            Type = type;
            RequirData = data;
            m_mQuest = quest;
        }

        public MapleQuestRequirementType Type { get; }
        public IMapleData RequirData { get; }

        public bool Check(MapleCharacter c, int? npcid)
        {
            if (RequirData == null)
            {
                return true;
            }
            switch (Type)
            {
                case MapleQuestRequirementType.Job:
                    foreach (var jobEntry in RequirData.Children)
                    {
                        var jobid = MapleDataTool.GetShort(jobEntry, -1);
                        if (jobid == -1)
                        {
                            return true;
                        }
                        if (c.Job == MapleJob.GetByJobId(jobid) || c.GmLevel > 0)
                        {
                            return true;
                        }
                    }
                    return false;
                case MapleQuestRequirementType.Quest:
                    foreach (var questEntry in RequirData.Children)
                    {
                        var qid = MapleDataTool.GetInt(questEntry.GetChildByPath("id"), -1);
                        if (qid == -1)
                        {
                            return true;
                        }
                        var q = c.GetQuest(MapleQuest.GetInstance(qid));
                        if (q == null &&
                            MapleDataTool.GetInt(questEntry.GetChildByPath("state"), 0) ==
                            (int) MapleQuestStatusType.NotStarted)
                        {
                            continue;
                        }
                        if (q == null || (int) q.Status != MapleDataTool.GetInt(questEntry.GetChildByPath("state"), 0))
                        {
                            return false;
                        }
                    }
                    return true;
                case MapleQuestRequirementType.Item:
                    var ii = MapleItemInformationProvider.Instance;
                    foreach (var itemEntry in RequirData.Children)
                    {
                        var itemId = MapleDataTool.GetInt(itemEntry.GetChildByPath("id"), -1);
                        if (itemId == -1)
                        {
                            return true;
                        }
                        short quantity = 0;
                        var iType = ii.GetInventoryType(itemId);
                        foreach (var item in c.Inventorys[iType.Value].ListById(itemId))
                        {
                            quantity += item.Quantity;
                        }
                        if (quantity < MapleDataTool.GetInt(itemEntry.GetChildByPath("count"), 0) ||
                            MapleDataTool.GetInt(itemEntry.GetChildByPath("count"), 0) <= 0 && quantity > 0)
                        {
                            return false;
                        }
                    }
                    return true;
                case MapleQuestRequirementType.MinLevel:
                    return c.Level >= MapleDataTool.GetInt(RequirData, 1);
                case MapleQuestRequirementType.MaxLevel:
                    return c.Level <= MapleDataTool.GetInt(RequirData, 200);
                case MapleQuestRequirementType.EndDate:
                    var timeStr = MapleDataTool.GetString(RequirData, null);
                    if (timeStr == null)
                    {
                        return true;
                    }
                    var cal = new DateTime(int.Parse(timeStr.Substring(0, 4)), int.Parse(timeStr.Substring(4, 6)),
                        int.Parse(timeStr.Substring(6, 8)), int.Parse(timeStr.Substring(8, 10)), 0, 0);
                    return cal.GetTimeMilliseconds() > DateTime.Now.GetTimeMilliseconds();
                case MapleQuestRequirementType.Mob:
                    foreach (var mobEntry in RequirData.Children)
                    {
                        var mobId = MapleDataTool.GetInt(mobEntry.GetChildByPath("id"), -1);
                        var killReq = MapleDataTool.GetInt(mobEntry.GetChildByPath("count"), 1);
                        if (mobId == -1)
                        {
                            return true; // let the thing slide I guess
                        }
                        if (c.GetQuest(m_mQuest).GetMobKills(mobId) < killReq)
                        {
                            return false;
                        }
                    }
                    return true;
                case MapleQuestRequirementType.MonsterBook:
                    //return c.getMonsterBook().getTotalCards() >= MapleDataTool.GetInt(getData());
                    return false;
                case MapleQuestRequirementType.Npc:
                    return npcid == null || npcid == MapleDataTool.GetInt(RequirData);
                case MapleQuestRequirementType.FieldEnter:
                    if (RequirData == null)
                    {
                        return true;
                    }
                    var zeroField = RequirData.GetChildByPath("0");
                    if (zeroField != null)
                    {
                        return MapleDataTool.GetInt(zeroField) == c.Map.MapId;
                    }
                    return false;
                case MapleQuestRequirementType.Interval:
                    return c.GetQuest(m_mQuest).Status != MapleQuestStatusType.Completed ||
                           c.GetQuest(m_mQuest).CompletionTime <=
                           DateTime.Now.GetTimeMilliseconds() - MapleDataTool.GetInt(RequirData)*60*1000;
                //case  MapleQuestRequirementType.PET:
                //case  MapleQuestRequirementType.MIN_PET_TAMENESS:
                case MapleQuestRequirementType.CompleteQuest:
                    if (c.GetNumQuest() >= MapleDataTool.GetInt(RequirData))
                    {
                        return true;
                    }
                    return false;
                default:
                    return true;
            }
        }

        public override string ToString() => $"{Type} {RequirData} {m_mQuest}";
        //        sb.Append("TYPE: ");
        //        StringBuilder sb = new StringBuilder();
        //    {
        //    try
        //{


        //public string getDebug(MapleCharacter c)
        //        sb.Append(type);
        //        sb.Append(" ");
        //        if (data == null)
        //        {
        //            sb.Append("DATA EMPTY! (2)");
        //            return sb.ToString();
        //        }
        //    outSwitch:
        //        switch (type)
        //        {
        //            case MapleQuestRequirementType.JOB:
        //                foreach (var jobEntry : getData().getChildren())
        //                {
        //                    int jobid = MapleDataTool.getInt(jobEntry);

        //                    if (c.getJob().equals(MapleJob.getById(jobid)) || c.isGM())
        //                    {
        //                        sb.append("CHECK: TRUE");
        //                        goto outSwitch;
        //                    }
        //                }
        //                sb.append("CHECK: FALSE (JOB: ");
        //                sb.append(c.getJob().getId());
        //                sb.append(") ");
        //                break;

        //            case MapleQuestRequirementType.QUEST:
        //                for (MapleData questEntry : getData().getChildren())
        //                {
        //                    int qid = MapleDataTool.getInt(questEntry.getChildByPath("id"), -1);
        //                    if (qid == -1)
        //                    {
        //                        sb.append("QID_IS_NEGATIVE_1_PASS ");
        //                        continue;
        //                    }
        //                    int rqs = MapleDataTool.getInt(questEntry.getChildByPath("state"), 0);
        //                    MapleQuestStatus q = c.getQuest(MapleQuest.getInstance(qid));
        //                    if (q == null && MapleQuestStatus.Status.getById(rqs).equals(MapleQuestStatus.Status.NOT_STARTED))
        //                    {
        //                        sb.append("QID_");
        //                        sb.append(qid);
        //                        sb.append("_NOTSTARTED_PASS ");
        //                        continue;
        //                    }
        //                    if (q == null || !q.getStatus().equals(MapleQuestStatus.Status.getById(rqs)))
        //                    {
        //                        sb.append("QID_");
        //                        sb.append(qid);
        //                        sb.append("_");
        //                        sb.append(q.getStatus().getId());
        //                        sb.append("_NOTEQUAL_");

        //                        sb.append(rqs);
        //                        sb.append("_FAIL");
        //                        break outSwitch;

        //                    }
        //                }
        //                sb.append("CHECK: TRUE");
        //                break;
        //            case MapleQuestRequirementType.ITEM:
        //                MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();
        //                for (MapleData itemEntry : getData().getChildren())
        //                {
        //                    int itemId = MapleDataTool.getInt(itemEntry.getChildByPath("id"), -1);
        //                    if (itemId == -1)
        //                    {
        //                        sb.append("ITEMID_NEGATIVE_1_PASS ");
        //                        continue;
        //                    }
        //                    short quantity = 0;
        //                    MapleInventoryType iType = ii.getInventoryType(itemId);
        //                    for (IItem item : c.getInventory(iType).listById(itemId))
        //                    {
        //                        quantity += item.getQuantity();
        //                    }
        //                    if (quantity < MapleDataTool.getInt(itemEntry.getChildByPath("count"), 0) || MapleDataTool.getInt(itemEntry.getChildByPath("count"), 0) <= 0 && quantity > 0)
        //                    {
        //                        sb.append("NOT_ENOUGH_OF_");
        //                        sb.append(itemId);
        //                        sb.append("_NEED");
        //                        sb.append(MapleDataTool.getInt(itemEntry.getChildByPath("count"), -1));
        //                        sb.append("_HAVE");
        //                        sb.append(quantity);
        //                        break outSwitch;
        //                    }
        //                }
        //                sb.append("CHECK: TRUE");
        //                break;
        //            case MapleQuestRequirementType.MIN_LEVEL:
        //                sb.append("MIN_LEVEL");
        //                sb.append(MapleDataTool.getInt(getData(), -1));
        //                sb.append("_HAVELEVEL");
        //                sb.append(c.getLevel());
        //                break;
        //            case MapleQuestRequirementType.MAX_LEVEL:
        //                sb.append("MAX_LEVEL");
        //                sb.append(MapleDataTool.getInt(getData(), -1));
        //                sb.append("_HAVELEVEL");
        //                sb.append(c.getLevel());
        //                break;
        //            case MapleQuestRequirementType.END_DATE:
        //                string timeStr = MapleDataTool.getString(getData(), null);
        //                if (timeStr == null)
        //                {
        //                    sb.append("TIMESTR_NULL_PASS");
        //                    break;
        //                }
        //                Calendar cal = Calendar.getInstance();
        //                cal.set(int.parseInt(timeStr.substring(0, 4)), int.parseInt(timeStr.substring(4, 6)), int.parseInt(timeStr.substring(6, 8)), int.parseInt(timeStr.substring(8, 10)), 0);
        //                sb.append("REQ_PASS_");
        //                sb.append(cal.getTimeInMillis() > System.currentTimeMillis());
        //                break;
        //            case MapleQuestRequirementType.MOB:
        //                for (MapleData mobEntry : getData().getChildren())
        //                {
        //                    int mobId = MapleDataTool.getInt(mobEntry.getChildByPath("id"), -1);
        //                    int killReq = MapleDataTool.getInt(mobEntry.getChildByPath("count"), 1);
        //                    if (mobId == -1)
        //                    {
        //                        sb.append("MOBID_NEGATIVE_1 ");
        //                        continue;
        //                    }
        //                    if (c.getQuest(quest).getMobKills(mobId) < killReq)
        //                    {
        //                        sb.append("NEED");
        //                        sb.append(killReq);
        //                        sb.append("HAVE");
        //                        sb.append(c.getQuest(quest).getMobKills(mobId));
        //                        sb.append("OF");
        //                        sb.append(mobId);
        //                        break outSwitch;
        //                    }
        //                }
        //                sb.append("CHECK: TRUE");
        //                break;
        //            case MapleQuestRequirementType.NPC:
        //                sb.append(MapleDataTool.getInt(getData(), -1));
        //                break;
        //            case MapleQuestRequirementType.FIELD_ENTER:
        //                if (getData() == null)
        //                {
        //                    sb.append("DATANULL");
        //                    break;
        //                }
        //                MapleData zeroField = getData().getChildByPath("0");
        //                if (zeroField != null)
        //                {
        //                    sb.append(MapleDataTool.getInt(zeroField) == c.getMapId());
        //                    sb.append("AT");
        //                    sb.append(c.getMapId());
        //                    sb.append("NEED");
        //                    sb.append(MapleDataTool.getInt(zeroField));
        //                    break;
        //                }
        //                sb.append("ZEROFIELD_NULL");
        //                break;
        //            //case  MapleQuestRequirementType.INTERVAL:
        //            //return !c.getQuest(quest).getStatus().equals(MapleQuestStatus.Status.COMPLETED) || c.getQuest(quest).getCompletionTime() <= System.currentTimeMillis() - MapleDataTool.getInt(getData()) * 60 * 1000;
        //            //case  MapleQuestRequirementType.PET:
        //            //case  MapleQuestRequirementType.MIN_PET_TAMENESS:
        //            default:
        //                sb.append("DEFAULT");
        //                break;
        //        }
        //        return sb.toString();
        //    }
        //    catch (Throwable t)
        //    {
        //        return getType().toString() + "EXCEPTION";
        //    }

        //}
    }
}