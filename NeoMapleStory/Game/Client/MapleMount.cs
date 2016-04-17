using NeoMapleStory.Core.TimeManager;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.Client
{
    public class MapleMount
    {
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

        private static MapleCharacter _mOwner;
        private string _mTokenSource;

        public MapleMount(MapleCharacter owner, int id, int skillid)
        {
            ItemId = id;
            SkillId = skillid;
            Tiredness = 0;
            Level = 1;
            Exp = 0;
            _mOwner = owner;
            Active = true;
        }

        public void StartTask()
        {
            _mTokenSource = TimerManager.Instance.RegisterJob(IncreaseTirednessJob, 60, 60);         
        }

        public void CancelTask()
        {
            TimerManager.Instance.CancelJob(_mTokenSource);
        }

        public void IncreaseTirednessJob()
        {
            Tiredness++;
            _mOwner.Map.BroadcastMessage(PacketCreator.UpdateMount(_mOwner.Id, this, false));

            if (Tiredness > 100)
                _mOwner.DispelSkill(_mOwner.JobType * 20000000 + 1004);
        }

    }
}
