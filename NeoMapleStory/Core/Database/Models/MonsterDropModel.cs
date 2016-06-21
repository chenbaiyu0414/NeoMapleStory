using System.ComponentModel.DataAnnotations;

namespace NeoMapleStory.Core.Database.Models
{
    public class MonsterDropModel
    {
        [Key]
        public int Id { get; set; }

        public int MonsterId { get; set; }

        public int ItemId { get; set; }

        public int Chance { get; set; }

        public int QuestId { get; set; }
    }
}
