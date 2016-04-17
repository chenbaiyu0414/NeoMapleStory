using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NeoMapleStory.Game.Inventory;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class InventoryItem
    {
        [Key]
        public int InventoryItemId { get; set; }

        [ForeignKey("CharacterId")]
        public virtual CharacterModel CharacterInfo { get; set; }
        public int CharacterId { get; set; }

        public int ItemId { get; set; }
        public MapleInventoryType InventoryType { get; set; }
        public byte Position { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "varchar")]
        public string Owner { get; set; } = "";


        public TimeSpan? ExpireDate { get; set; }

        public int UniqueId { get; set; }

        public int PetSlot { get; set; }
    }
}
