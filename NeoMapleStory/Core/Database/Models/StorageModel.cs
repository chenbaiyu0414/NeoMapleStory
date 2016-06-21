using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("Storages")]
    public class StorageModel
    {
        [Key]
        public int Id { get; set; }

        public int AId { get; set; }

        public byte Slots { get; set; }

        public int Meso { get; set; }
    }
}
