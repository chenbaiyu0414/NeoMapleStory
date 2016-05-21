using NeoMapleStory.Core;
using NeoMapleStory.Packet;
using Quartz;

namespace NeoMapleStory.Game.Client
{
    public class MapleMount
    {
        private static MapleCharacter m_mOwner;
        private TriggerKey m_mTokenSource;

        public MapleMount(MapleCharacter owner, int id, int skillid)
        {
            ItemId = id;
            SkillId = skillid;
            Tiredness = 0;
            Level = 1;
            Exp = 0;
            m_mOwner = owner;
            Active = true;
        }

        public int Id
        {
            get { return ItemId < 1932001 ? ItemId - 1901999 : (ItemId == 1932000 ? 4 : 5); }
        }

        public int ItemId { get; set; }
        public int SkillId { get; set; }

        public int Tiredness
        {
            get { return Tiredness; }
            set { Tiredness = value < 0 ? 0 : value; }
        }

        public int Exp { get; set; }
        public int Level { get; set; } = 1;
        public bool Active { get; set; }

        public void StartTask()
        {
            m_mTokenSource = TimerManager.Instance.RepeatTask(IncreaseTirednessJob, 60*1000, 60*1000);
        }

        public void CancelTask()
        {
            TimerManager.Instance.CancelTask(m_mTokenSource);
        }

        public void IncreaseTirednessJob()
        {
            Tiredness++;
            m_mOwner.Map.BroadcastMessage(PacketCreator.UpdateMount(m_mOwner.Id, this, false));

            if (Tiredness > 100)
                m_mOwner.DispelSkill(m_mOwner.JobType*20000000 + 1004);
        }
    }
}