using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoMapleStory.Game.Quest
{
    public enum MapleQuestActionType
    {
        Undefined = -1, Exp = 0, Item = 1, Nextquest = 2, Meso = 3, Quest = 4, Skill = 5, Fame = 6, Buff = 7, Petskill = 8
    }

    public static class MapleQuestActionTypeExtension
    {
        public static MapleQuestActionType GetByWzName(string name)
        {
            if (name.Equals("exp"))
            {
                return MapleQuestActionType.Exp;
            }
            else if (name.Equals("money"))
            {
                return MapleQuestActionType.Meso;
            }
            else if (name.Equals("item"))
            {
                return MapleQuestActionType.Item;
            }
            else if (name.Equals("skill"))
            {
                return MapleQuestActionType.Skill;
            }
            else if (name.Equals("nextQuest"))
            {
                return MapleQuestActionType.Nextquest;
            }
            else if (name.Equals("pop"))
            {
                return MapleQuestActionType.Fame;
            }
            else if (name.Equals("buffItemID"))
            {
                return MapleQuestActionType.Buff;
            }
            else {
                return MapleQuestActionType.Undefined;
            }
        }
    }

     public class MapleQuestAction
    {
        public MapleQuestActionType Type { get; private set; }
        public IMapleData Data { get; private set; }
        private readonly MapleQuest _quest;

        /** Creates a new instance of MapleQuestAction */
        public MapleQuestAction(MapleQuestActionType type, IMapleData data, MapleQuest quest)
        {
            this.Type = type;
            this.Data = data;
            this._quest = quest;
        }

        public bool Check(MapleCharacter c)
        {
            switch (Type)
            {
                case MapleQuestActionType.Meso:
                    int mesars = MapleDataTool.GetInt(Data);
                    if (c.Money.Value + mesars < 0)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        private bool CanGetItem(IMapleData item, MapleCharacter c)
        {
            if (item.GetChildByPath("gender") != null)
            {
                int gender = MapleDataTool.GetInt(item.GetChildByPath("gender"));
                if (gender != 2 && gender != (c.Gender ? 1 : 0))
                {
                    return false;
                }
            }
            if (item.GetChildByPath("job") != null)
            {
                int job = MapleDataTool.GetInt(item.GetChildByPath("job"));
                if (job < 100)
                {
                    if (MapleJob.GetBy5ByteEncoding(job).JobId / 100 != c.Job.JobId / 100)
                    {
                        return false;
                    }
                }
                else {
                    if (job != c.Job.JobId)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Run(MapleCharacter c, int? extSelection)
        {
            MapleQuestStatus status;
            switch (Type)
            {
                case MapleQuestActionType.Exp:
                    status = c.GetQuest(_quest);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                    {
                        break;
                    }
                    c.gainExp(MapleDataTool.GetInt(Data) * c.Client.ChannelServer.ExpRate, true, true);
                    break;
                case MapleQuestActionType.Item:
                    MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
                    Dictionary<int, int> props = new Dictionary<int, int>();
                    foreach (var iEntry in Data.Children)
                    {
                        if (iEntry.GetChildByPath("prop") != null && MapleDataTool.GetInt(iEntry.GetChildByPath("prop")) != -1 && CanGetItem(iEntry, c))
                        {
                            for (int i = 0; i < MapleDataTool.GetInt(iEntry.GetChildByPath("prop")); i++)
                            {
                                props.Add(props.Count, MapleDataTool.GetInt(iEntry.GetChildByPath("id")));
                            }
                        }
                    }
                    int selection = 0;
                    int extNum = 0;
                    if (props.Any())
                    {
                        props.TryGetValue((int)(Randomizer.NextDouble() * props.Count), out selection);
                    }
                    foreach (var iEntry in Data.Children)
                    {
                        if (!CanGetItem(iEntry, c))
                        {
                            continue;
                        }
                        if (iEntry.GetChildByPath("prop") != null)
                        {
                            if (MapleDataTool.GetInt(iEntry.GetChildByPath("prop")) == -1)
                            {
                                if (extSelection.HasValue && extSelection.Value != extNum++)
                                {
                                    continue;
                                }
                            }
                            else if (MapleDataTool.GetInt(iEntry.GetChildByPath("id")) != selection)
                            {
                                continue;
                            }
                        }
                        if (MapleDataTool.GetInt(iEntry.GetChildByPath("count"), 0) < 0)
                        { // remove items

                            int itemId = MapleDataTool.GetInt(iEntry.GetChildByPath("id"));
                            MapleInventoryType iType = ii.GetInventoryType(itemId);
                            short quantity = (short)(MapleDataTool.GetInt(iEntry.GetChildByPath("count"), 0) * -1);
                            try
                            {
                                //MapleInventoryManipulator.removeById(c.Client, iType, itemId, quantity, true, false);
                            }
                            catch
                            {
                                Console.WriteLine("Completing a quest without meeting the requirements");
                            }
                            c.Client.Send(PacketCreator.GetShowItemGain(itemId, (short)MapleDataTool.GetInt(iEntry.GetChildByPath("count"), 0), true));
                        }
                        else { // add items

                            int itemId = MapleDataTool.GetInt(iEntry.GetChildByPath("id"));
                            short quantity = (short)MapleDataTool.GetInt(iEntry.GetChildByPath("count"), 0);
                            StringBuilder logInfo = new StringBuilder(c.Name);
                            logInfo.Append(" received ");
                            logInfo.Append(quantity);
                            logInfo.Append(" as reward from a quest");
                           // MapleInventoryManipulator.addById(c.Client, itemId, quantity, logInfo.ToString(), null, -1);
                            c.Client.Send(PacketCreator.GetShowItemGain(itemId, quantity, true));
                        }
                    }
                    break;
                case MapleQuestActionType.Nextquest:
                    status = c.GetQuest(_quest);
                    int nextQuest = MapleDataTool.GetInt(Data);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                    {
                        break;
                    }
                    c.Client.Send(PacketCreator.UpdateQuestFinish((short)_quest.GetQuestId(), status.Npcid, (short)nextQuest));
                    MapleQuest.GetInstance(nextQuest).start(c, status.Npcid);
                    break;
                case MapleQuestActionType.Meso:
                    status = c.GetQuest(_quest);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                    {
                        break;
                    }
                    c.GainMeso(MapleDataTool.GetInt(Data), true, false, true);
                    break;
                case MapleQuestActionType.Quest:
                    foreach (var qEntry in Data)
                    {
                        int questid = MapleDataTool.GetInt(qEntry.GetChildByPath("id"));
                        int stat = MapleDataTool.GetInt(qEntry.GetChildByPath("state"), 0);
                        c.UpdateQuest(new MapleQuestStatus(MapleQuest.GetInstance(questid), (MapleQuestStatusType)stat));
                    }
                    break;
                case MapleQuestActionType.Skill:
                    foreach (var sEntry in Data)
                    {
                        int skillid = MapleDataTool.GetInt(sEntry.GetChildByPath("id"));
                        int skillLevel = MapleDataTool.GetInt(sEntry.GetChildByPath("skillLevel"));
                        int masterLevel = MapleDataTool.GetInt(sEntry.GetChildByPath("masterLevel"));
                        var skillObject = SkillFactory.GetSkill(skillid);
                        bool shouldLearn = false;
                        var applicableJobs = sEntry.GetChildByPath("job");
                        foreach (var applicableJob in applicableJobs)
                        {
                            MapleJob job = MapleJob.GetByJobId(MapleDataTool.GetShort(applicableJob));
                            if (c.Job == job)
                            {
                                shouldLearn = true;
                                break;
                            }
                        }
                        if (skillObject.IsBeginnerSkill)
                        {
                            shouldLearn = true;
                        }
                        skillLevel = Math.Max(skillLevel, c.getSkillLevel(skillObject));
                        masterLevel = Math.Max(masterLevel, skillObject.MaxLevel);
                        if (shouldLearn)
                        {
                            c.ChangeSkillLevel(skillObject, skillLevel, masterLevel);
                          c.DropMessage($"你已获得 { SkillFactory.GetSkillName(skillid) } 当前等级 { skillLevel } 最高等级 { masterLevel}");
                        }
                    }
                    break;
                case MapleQuestActionType.Fame:
                    status = c.GetQuest(_quest);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                    {
                        break;
                    }
                    c.Fame += MapleDataTool.GetShort(Data);
                    c.UpdateSingleStat(MapleStat.Fame, c.Fame);
                    int fameGain = MapleDataTool.GetInt(Data);
                    c.Client.Send(PacketCreator.GetShowFameGain(fameGain));
                    break;
                case MapleQuestActionType.Buff:
                    status = c.GetQuest(_quest);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                    {
                        break;
                    }
                    MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
                    //mii.GetItemEffect(MapleDataTool.GetInt(data)).applyTo(c);
                    break;
                case MapleQuestActionType.Petskill:
                    status = c.GetQuest(_quest);
                    if (status.Status == MapleQuestStatusType.NotStarted && status.Forfeited > 0)
                        break;
                    int flag = MapleDataTool.GetInt("petskill", Data);
                    //c.getPet(0).setFlag((byte)(c.getPet(0).getFlag() | InventoryConstants.Items.Flags.getFlagByInt(flag)));
                    break;
                default:
                    break;
            }
        }


        public override string ToString() => $"{Type} :  {Data}";

    }
}
