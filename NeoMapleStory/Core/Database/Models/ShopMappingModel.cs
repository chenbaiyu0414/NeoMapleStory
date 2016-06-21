using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("ShopMappings")]
    public class ShopMappingModel
    {
        [Key]
        public int ShopId { get; set; }

        public int Npcid { get; set; }
    }
}