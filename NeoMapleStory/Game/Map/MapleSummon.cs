using System;
using System.Drawing;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleSummon : AbstractAnimatedMapleMapObject
    {
        public MapleSummon(MapleCharacter owner, int skill, Point pos, SummonMovementType movementType)
        {
            Owner = owner;
            SkillId = skill;
            SkillLevel = owner.GetSkillLevel(SkillFactory.GetSkill(skill));
            if (SkillLevel == 0)
            {
                Console.WriteLine("Trying to create a summon for a char without the skill");
            }
            MovementType = movementType;
            Position = pos;
        }

        public MapleCharacter Owner { get; private set; }
        public int SkillLevel { get; }
        public int SkillId { get; }
        public int Hp { get; set; }
        public SummonMovementType MovementType { get; private set; }

        public bool IsPuppet() => SkillId == 3111002 || SkillId == 3211002 || SkillId == 5211001 || SkillId == 13111004;

        public bool IsSummon()
            =>
                SkillId == 2311006 || SkillId == 2321003 || SkillId == 2121005 || SkillId == 2221005 ||
                SkillId == 5211002;

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.RemoveSpecialMapObject(this, true));
        }

        public override void SendSpawnData(MapleClient client)
        {
            client.Send(PacketCreator.SpawnSpecialMapObject(this, SkillLevel, false));
        }

        public override MapleMapObjectType GetType() => MapleMapObjectType.Summon;
    }
}