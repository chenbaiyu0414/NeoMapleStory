using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class ShopItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; }

        public int ShopId { get; set; }

        public int ItemId { get; set; }
        public int Price { get; set; }
        public int Position { get; set; }
    }
}