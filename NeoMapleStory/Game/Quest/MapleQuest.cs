using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.Quest
{
    public class MapleQuest
    {
        private static readonly Dictionary<int, MapleQuest> Quests = new Dictionary<int, MapleQuest>();
        private static readonly IMapleDataProvider QuestData = MapleDataProviderFactory.GetDataProvider("Quest.wz");
        private static readonly IMapleData Actions = QuestData.GetData("Act.img");
        private static readonly IMapleData Requirements = QuestData.GetData("Check.img");
        private static readonly IMapleData Info = QuestData.GetData("QuestInfo.img");
        private readonly bool m_autoPreComplete;

        private readonly bool m_autoStart;
        private readonly bool m_repeatable;
        protected List<MapleQuestAction> CompleteActs;
        protected List<MapleQuestRequirement> CompleteReqs;
        protected List<MapleQuestAction> StartActs;
        protected List<MapleQuestRequirement> StartReqs;


        protected MapleQuest()
        {
            RelevantMobs = new List<int>();
        }

        private MapleQuest(int id)
        {
            QuestId = id;
            RelevantMobs = new List<int>();
            var startReqData = Requirements.GetChildByPath(id.ToString()).GetChildByPath("0");
            StartReqs = new List<MapleQuestRequirement>();
            if (startReqData != null)
            {
                foreach (var startReq in startReqData.Children)
                {
                    var type = MapleQuestRequirementTypeExtension.GetByWzName(startReq.Name);
                    if (type == MapleQuestRequirementType.Interval)
                    {
                        m_repeatable = true;
                    }
                    var req = new MapleQuestRequirement(this, type, startReq);
                    if (req.Type == MapleQuestRequirementType.Mob)
                    {
                        foreach (var mob in startReq.Children)
                        {
                            RelevantMobs.Add(MapleDataTool.GetInt(mob.GetChildByPath("id")));
                        }
                    }
                    StartReqs.Add(req);
                }
            }
            var completeReqData = Requirements.GetChildByPath(id.ToString()).GetChildByPath("1");
            CompleteReqs = new List<MapleQuestRequirement>();
            if (completeReqData != null)
            {
                foreach (var completeReq in completeReqData.Children)
                {
                    var req = new MapleQuestRequirement(this,
                        MapleQuestRequirementTypeExtension.GetByWzName(completeReq.Name), completeReq);
                    if (req.Type == MapleQuestRequirementType.Mob)
                    {
                        foreach (var mob in completeReq.Children)
                        {
                            RelevantMobs.Add(MapleDataTool.GetInt(mob.GetChildByPath("id")));
                        }
                    }
                    CompleteReqs.Add(req);
                }
            }
            // read acts
            var startActData = Actions.GetChildByPath(id.ToString()).GetChildByPath("0");
            StartActs = new List<MapleQuestAction>();
            if (startActData != null)
            {
                foreach (var startAct in startActData.Children)
                {
                    var questActionType = MapleQuestActionTypeExtension.GetByWzName(startAct.Name);
                    StartActs.Add(new MapleQuestAction(questActionType, startAct, this));
                }
            }
            var completeActData = Actions.GetChildByPath(id.ToString()).GetChildByPath("1");
            CompleteActs = new List<MapleQuestAction>();
            if (completeActData != null)
            {
                foreach (var completeAct in completeActData.Children)
                {
                    CompleteActs.Add(new MapleQuestAction(MapleQuestActionTypeExtension.GetByWzName(completeAct.Name),
                        completeAct, this));
                }
            }
            var questInfo = Info.GetChildByPath(id.ToString());
            m_autoStart = MapleDataTool.GetInt("autoStart", questInfo, 0) == 1;
            m_autoPreComplete = MapleDataTool.GetInt("autoPreComplete", questInfo, 0) == 1;
        }

        public int QuestId { get; set; }
        protected List<int> RelevantMobs { get; set; }

        public static MapleQuest GetInstance(int id)
        {
            MapleQuest ret;

            if (!Quests.TryGetValue(id, out ret))
            {
                ret = new MapleQuest(id);
                Quests.Add(id, ret);
            }
            return ret;
        }

        private bool CanStart(MapleCharacter c, int npcid)
        {
            if (c.GetQuest(this).Status != MapleQuestStatusType.NotStarted &&
                !(c.GetQuest(this).Status == MapleQuestStatusType.Completed && m_repeatable))
            {
                return false;
            }
            foreach (var r in StartReqs)
            {
                if (!r.Check(c, npcid))
                {
                    return false;
                }
            }
            return true;
        }

        public static void Remove(int questid)
        {
            if (Quests.ContainsKey(questid))
                Quests.Remove(questid);
        }

        public bool CanComplete(MapleCharacter c, int? npcid)
        {
            if (c.GetQuest(this).Status != MapleQuestStatusType.Started)
            {
                return false;
            }
            foreach (var r in CompleteReqs)
            {
                if (!r.Check(c, npcid))
                {
                    return false;
                }
            }
            return true;
        }

        public void Start(MapleCharacter c, int npc)
        {
            Start(c, npc, false);
        }

        public void Start(MapleCharacter c, int npc, bool force)
        {
            try
            {
                var arg1 = force && CheckNpcOnMap(c, npc);
                var arg2 = m_autoStart || CheckNpcOnMap(c, npc);
                var arg3 = CanStart(c, npc);
                if (arg1 || (arg2 && arg3))
                {
                    foreach (var action in StartActs)
                    {
                        action.Run(c, null);
                    }
                    var oldStatus = c.GetQuest(this);
                    var newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Started, npc);
                    newStatus.CompletionTime = oldStatus.CompletionTime;
                    newStatus.Forfeited = oldStatus.Forfeited;
                    c.UpdateQuest(newStatus);
                }
            }
            catch
            {
                c.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "发生错误,请截图后联系管理员."));
            }
        }

        public bool NullStartQuestData()
        {
            var nullStartReqData = false;
            var nullStartActData = false;
            try
            {
                foreach (var requirement in StartReqs)
                {
                    if (requirement.RequirData == null)
                    {
                        nullStartReqData = true;
                        break;
                    }
                }
                if (nullStartReqData)
                {
                    try
                    {
                        StartReqs.Clear();
                    }
                    catch
                    {
                        Console.WriteLine("Exception occured while clearing startReqs");
                    }
                }
                foreach (var action in StartActs)
                {
                    if (action.Data == null)
                    {
                        nullStartActData = true;
                        break;
                    }
                }
                if (nullStartActData)
                {
                    try
                    {
                        StartActs.Clear();
                    }
                    catch
                    {
                        Console.WriteLine("Exception occured while clearing startActs");
                    }
                }
            }
            catch
            {
                return true;
            }
            return nullStartReqData || nullStartActData;
        }

        public bool NullCompleteQuestData()
        {
            var nullCompleteReqData = false;
            var nullCompleteActData = false;
            try
            {
                foreach (var requirement in CompleteReqs)
                {
                    if (requirement.RequirData == null)
                    {
                        nullCompleteReqData = true;
                        break;
                    }
                }
                if (nullCompleteReqData)
                {
                    try
                    {
                        CompleteReqs.Clear();
                    }
                    catch
                    {
                        Console.WriteLine("Exception occured while clearing completeReqs");
                    }
                }
                foreach (var action in CompleteActs)
                {
                    if (action.Data == null)
                    {
                        nullCompleteActData = true;
                        break;
                    }
                }
                if (nullCompleteActData)
                {
                    try
                    {
                        CompleteActs.Clear();
                    }
                    catch
                    {
                        Console.WriteLine("Exception occured while clearing completeActs");
                    }
                }
            }
            catch
            {
                return true;
            }
            return nullCompleteReqData || nullCompleteActData;
        }

        public void Complete(MapleCharacter c, int npc)
        {
            Complete(c, npc, 0, false);
        }

        public void Complete(MapleCharacter c, int npc, bool force)
        {
            Complete(c, npc, 0, force);
        }

        public void Complete(MapleCharacter c, int npc, int selection, bool force)
        {
            try
            {
                var arg1 = m_autoPreComplete || CheckNpcOnMap(c, npc);
                var canbeComplete = CanComplete(c, npc);
                if (force || (arg1 && canbeComplete))
                {
                    foreach (var action in CompleteActs)
                    {
                        if (!action.Check(c))
                        {
                        }
                    }
                    foreach (var action in CompleteActs)
                    {
                        action.Run(c, selection);
                    }
                    var oldStatus = c.GetQuest(this);
                    var newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Completed, npc);
                    newStatus.Forfeited = oldStatus.Forfeited;
                    c.UpdateQuest(newStatus);
                }
                else
                {
                    c.DropMessage(PacketCreator.ServerMessageType.Popup, "你遇到任务错误.请稍后片刻。或请联系系统管理员");
                }
            }
            catch
            {
                c.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "系统发生错误.请联系系统管理员解决"));
            }
        }

        public void Forfeit(MapleCharacter c)
        {
            if (c.GetQuest(this).Status != MapleQuestStatusType.Started)
            {
                return;
            }
            var oldStatus = c.GetQuest(this);
            var newStatus = new MapleQuestStatus(this, MapleQuestStatusType.NotStarted);
            newStatus.Forfeited = oldStatus.Forfeited + 1;
            newStatus.CompletionTime = oldStatus.CompletionTime;
            c.UpdateQuest(newStatus);
        }

        public void ForceStart(MapleCharacter c, int npc)
        {
            var oldStatus = c.GetQuest(this);
            var newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Started, npc);
            newStatus.Forfeited = oldStatus.Forfeited;
            c.UpdateQuest(newStatus);
        }

        public void ForceComplete(MapleCharacter c, int npc)
        {
            var oldStatus = c.GetQuest(this);
            var newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Completed, npc);
            newStatus.Forfeited = oldStatus.Forfeited;
            c.UpdateQuest(newStatus);
        }

        private bool CheckNpcOnMap(MapleCharacter player, int npcid) => player.Map.ContainsNpc(npcid);

        public int GetQuestId() => QuestId;

        public List<int> GetRelevantMobs() => RelevantMobs;
    }
}