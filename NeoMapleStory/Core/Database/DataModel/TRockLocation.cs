using System.ComponentModel.DataAnnotations;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class RockLocation
    {
        [Key]
        public int RockId { get; set; }

        public int CharacterId { get; set; }

        public int MapId { get; set; }

        public int Type { get; set; }
    }
}
