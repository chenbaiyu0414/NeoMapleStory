using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.DataModel
{
    [Table("Characters")]
     public class CharacterModel
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(16)]
        public string CharacterName { get; set; }

        public short JobId { get; set; }

        public bool IsMarried { get; set; }

        public int GmLevel { get; set; }

        public int MapId { get; set; } = 10000;

        public byte InitialSpawnPoint { get; set; }

        public int PartyId { get; set; }

        public int World { get; set; }

        public bool Gender { get; set; }

        public int Face { get; set; }

        public int Hair { get; set; }

        public short RemainingAp { get; set; }

        public short RemainingSp { get; set; }

        public short Fame { get; set; }

        public byte EquipSlots { get; set; }
        public byte UseSlots { get; set; }
        public byte SetupSlots { get; set; }
        public byte EtcSlots { get; set; }
        public byte CashSlots { get; set; }

        public int AllianceRank { get; set; }
        public int BookCover { get; set; }
        public short Hp { get; set; }
        public bool Incs { get; set; }
        public bool Inmts { get; set; }

        public byte Level { get; set; }
        public int? Maplemount { get; set; }
        public short Maxhp { get; set; }
        public short Maxmp { get; set; }
        public short Mp { get; set; }
        public int Team { get; set; }
        public int TotalCp { get; set; }



        public int Exp { get; set; }

        public int Money { get; set; }

        public byte SkinColorId { get; set; }


        public short Localmaxhp { get; set; }
        public short Localmaxmp { get; set; }
        public short Localdex { get; set; }
        public short Localint { get; set; }
        public short Localstr { get; set; }
        public short Localluk { get; set; }

        public short Dex { get; set; }
        public short Int { get; set; }
        public short Luk { get; set; }
        public short Str { get; set; }
        public int Magic { get; set; }
        public int Watk { get; set; }
        public double SpeedMod { get; set; }
        public double JumpMod { get; set; }
        public int HpApUsed { get; set; }
        public int MpApUsed { get; set; }
        public int SpwanPoint { get; set; }
        public int AutoHpPot { get; set; }
        public int AutoMpPot { get; set; }

        public int MessengerId { get; set; }
        public int MessengerPosition { get; set; }

        public int Partnerid { get; set; }
        public bool CanTalk { get; set; }
        public int ZakumLvl { get; set; }
        public int MarriageQuestLevel { get; set; }
        public int BossPoints { get; set; }
        public int BossRepeats { get; set; }
        public long NextBq { get; set; }
        public bool PlayerNpc { get; set; }
        public bool Muted { get; set; }
        public DateTime? UnmuteTime { get; set; }
        public int DojoPoints { get; set; }
        public int LastDojoStage { get; set; }
        public bool FinishedDojoTutorial { get; set; }
        public int VanquisherStage { get; set; }
        public int VanquisherKills { get; set; }
        public int Warning { get; set; }

    }
}
