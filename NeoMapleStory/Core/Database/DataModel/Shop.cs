using System.ComponentModel.DataAnnotations;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        public int Npcid { get; set; }
    }
}