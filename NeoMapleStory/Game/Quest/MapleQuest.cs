using NeoMapleStory.Game.Data;
using NeoMapleStory.Packet;
using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.Quest
{
     public class MapleQuest
    {
        private static readonly Dictionary<int, MapleQuest> Quests = new Dictionary<int, MapleQuest>();

        public int QuestId { get; set; }
        protected List<int> RelevantMobs { get; set; }
        protected List<MapleQuestRequirement> StartReqs;
        protected List<MapleQuestRequirement> CompleteReqs;
        protected List<MapleQuestAction> StartActs;
        protected List<MapleQuestAction> CompleteActs;

        private readonly bool _autoStart;
        private readonly bool _autoPreComplete;
        private readonly bool _repeatable;
        private static readonly IMapleDataProvider QuestData = MapleDataProviderFactory.GetDataProvider("Quest.wz");
        private static readonly IMapleData Actions = QuestData.GetData("Act.img");
        private static readonly IMapleData Requirements = QuestData.GetData("Check.img");
        private static readonly IMapleData Info = QuestData.GetData("QuestInfo.img");


        protected MapleQuest()
        {
            RelevantMobs = new List<int>();
        }

        private MapleQuest(int id)
        {
            QuestId = id;
            RelevantMobs = new List<int>();
            IMapleData startReqData = Requirements.GetChildByPath(id.ToString()).GetChildByPath("0");
            StartReqs = new List<MapleQuestRequirement>();
            if (startReqData != null)
            {
                foreach (var startReq in startReqData.Children)
                {
                    MapleQuestRequirementType type = MapleQuestRequirementTypeExtension.GetByWzName(startReq.Name);
                    if (type==MapleQuestRequirementType.Interval)
                    {
                        _repeatable = true;
                    }
                    MapleQuestRequirement req = new MapleQuestRequirement(this, type, startReq);
                    if (req.Type==  MapleQuestRequirementType.Mob)
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
                    MapleQuestRequirement req = new MapleQuestRequirement(this, MapleQuestRequirementTypeExtension.GetByWzName(completeReq.Name), completeReq);
                    if (req.Type== MapleQuestRequirementType.Mob)
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
                    MapleQuestActionType questActionType = MapleQuestActionTypeExtension.GetByWzName(startAct.Name);
                    StartActs.Add(new MapleQuestAction(questActionType, startAct, this));
                }
            }
            var completeActData = Actions.GetChildByPath(id.ToString()).GetChildByPath("1");
            CompleteActs = new List<MapleQuestAction>();
            if (completeActData != null)
            {
                foreach (var completeAct in completeActData.Children)
                {
                    CompleteActs.Add(new MapleQuestAction(MapleQuestActionTypeExtension.GetByWzName(completeAct.Name), completeAct, this));
                }
            }
            var questInfo = Info.GetChildByPath(id.ToString());
            _autoStart = MapleDataTool.GetInt("autoStart", questInfo, 0) == 1;
            _autoPreComplete = MapleDataTool.GetInt("autoPreComplete", questInfo, 0) == 1;
        }

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
            if (c.GetQuest(this).Status != MapleQuestStatusType.NotStarted && !(c.GetQuest(this).Status == MapleQuestStatusType.Completed && _repeatable))
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

        public static void Remove(int questid) {
            if (Quests.ContainsKey(questid))
                Quests.Remove(questid);
        }
        public bool CanComplete(MapleCharacter c, int? npcid)
        {
            if (c.GetQuest(this).Status != MapleQuestStatusType.Started)
            {
                return false;
            }
            foreach (MapleQuestRequirement r in CompleteReqs)
            {
                if (!r.Check(c, npcid))
                {
                    return false;
                }
            }
            return true;
        }

        public void start(MapleCharacter c, int npc)
        {
            start(c, npc, false);
        }

        public void start(MapleCharacter c, int npc, bool force)
        {
            try
            {
                bool arg1 = force && CheckNpcOnMap(c, npc);
                bool arg2 = _autoStart || CheckNpcOnMap(c, npc);
                bool arg3 = CanStart(c, npc);
                if (arg1 || (arg2 && arg3))
                {

                    foreach (var action in StartActs)
                    {
                        action.Run(c, null);
                    }
                    MapleQuestStatus oldStatus = c.GetQuest(this);
                    MapleQuestStatus newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Started, npc);
                    newStatus.CompletionTime= oldStatus.CompletionTime;
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
            bool nullStartReqData = false;
            bool nullStartActData = false;
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
            bool nullCompleteReqData = false;
            bool nullCompleteActData = false;
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

        public void complete(MapleCharacter c, int npc)
        {
            complete(c, npc, 0, false);
        }

        public void complete(MapleCharacter c, int npc, bool force)
        {
            complete(c, npc, 0, force);
        }

        public void complete(MapleCharacter c, int npc, int selection, bool force)
        {
            try
            {
                bool arg1 = _autoPreComplete || CheckNpcOnMap(c, npc);
                bool canbeComplete = CanComplete(c, npc);
                if (force || (arg1 && canbeComplete))
                {

                    foreach (var action in CompleteActs)
                    {
                        if (!action.Check(c))
                        {
                            continue;
                        }
                    }
                    foreach (var action in CompleteActs)
                    {
                        action.Run(c, selection);
                    }
                    MapleQuestStatus oldStatus = c.GetQuest(this);
                    MapleQuestStatus newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Completed, npc);
                    newStatus.Forfeited= oldStatus.Forfeited;
                    c.UpdateQuest(newStatus);
                }
                else {
                    c.DropMessage(PacketCreator.ServerMessageType.Popup, "你遇到任务错误.请稍后片刻。或请联系系统管理员");
                }
            }
            catch
            {
                c.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup , "系统发生错误.请联系系统管理员解决" ));
            }
        }

        public void Forfeit(MapleCharacter c)
        {
            if (c.GetQuest(this).Status!= MapleQuestStatusType.Started)
            {
                return;
            }
            MapleQuestStatus oldStatus = c.GetQuest(this);
            MapleQuestStatus newStatus = new MapleQuestStatus(this, MapleQuestStatusType.NotStarted);
            newStatus.Forfeited= oldStatus.Forfeited + 1;
            newStatus.CompletionTime = oldStatus.CompletionTime;
            c.UpdateQuest(newStatus);
        }

        public void ForceStart(MapleCharacter c, int npc)
        {
            MapleQuestStatus oldStatus = c.GetQuest(this);
            MapleQuestStatus newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Started, npc);
            newStatus.Forfeited = oldStatus.Forfeited ;
            c.UpdateQuest(newStatus);
        }

        public void ForceComplete(MapleCharacter c, int npc)
        {
            MapleQuestStatus oldStatus = c.GetQuest(this);
            MapleQuestStatus newStatus = new MapleQuestStatus(this, MapleQuestStatusType.Completed, npc);
            newStatus.Forfeited = oldStatus.Forfeited;
            c.UpdateQuest(newStatus);
        }

        private bool CheckNpcOnMap(MapleCharacter player, int npcid) => player.Map.ContainsNpc(npcid);

        public int GetQuestId() => QuestId;

        public List<int> GetRelevantMobs() => RelevantMobs;
    }
}
