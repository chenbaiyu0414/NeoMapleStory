using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("InventoryItems")]
    public class InventoryItemModel
    {
        [Key]
        public Guid Id { get; set; }

        public int CId { get; set; }

        public int StorageId { get; set; }

        public int ItemId { get; set; }

        public byte InventoryType { get; set; }

        public byte Position { get; set; }

        public short Quantity { get; set; }

        public string Owner { get; set; } 

        public DateTime? ExpireDate { get; set; }

        public int UniqueId { get; set; }

        public byte PetSlot { get; set; }

        public virtual CharacterModel Character { get; set; }

        public virtual InventoryEquipmentModel InventoryEquipments { get; set; }
    }
}