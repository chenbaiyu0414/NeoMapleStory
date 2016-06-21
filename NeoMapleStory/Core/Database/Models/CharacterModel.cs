using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("Characters")]
    public class CharacterModel
    {
        [Key]
        public int Id { get; set; }

        public virtual AccountModel Account { get; set; }

        public int AId { get; set; }

        public byte WorldId { get; set; }

        public string Name { get; set; }

        public short JobId { get; set; }

        public byte GmLevel { get; set; }

        public bool Gender { get; set; }

        public byte Level { get; set; }

        public int Meso { get; set; }

        public int Exp { get; set; }

        public short Dex { get; set; }

        public short Int { get; set; }

        public short Luk { get; set; }

        public short Str { get; set; }

        public short Hp { get; set; }
               
        public short MaxHp { get; set; }
               
        public short Mp { get; set; }
               
        public short MaxMp { get; set; }

        public int Face { get; set; }

        public int Hair { get; set; }

        public byte Skin { get; set; }

        public short Fame { get; set; }

        public int MapId { get; set; }

        public byte SpawnPoint { get; set; }

        public int? PartyId { get; set; }

        public short RemainingAp { get; set; }

        public short RemainingSp { get; set; }

        public bool IsMarried { get; set; }

        public int AutoHpPot { get; set; }

        public int AutoMpPot { get; set; }

        public byte EquipSlots { get; set; }

        public byte UseSlots { get; set; }

        public byte SetupSlots { get; set; }

        public byte EtcSlots { get; set; }

        public byte CashSlots { get; set; }

        public virtual ICollection<InventoryItemModel> InventoryItems { get; set; }
    }
}