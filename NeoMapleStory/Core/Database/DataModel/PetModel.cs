using System.ComponentModel.DataAnnotations;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class PetModel
    {
        [Key]
        public int PetId { get; set; }

        public string PetName { get; set; }
        public short Closeness { get; set; } = 0;
        public byte Level { get; set; } = 1;
        public byte Fullness { get; set; } = 100;
        public int UniqueId { get; set; }
    }
}