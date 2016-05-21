using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.DataModel
{
    public class RingModel
    {
        [Key]
        public int Id { get; set; }

        public int RingId { get; set; }

        public int PartnerRingId { get; set; }

        public int PartnerCharacterId { get; set; }

        [Column(TypeName = "varchar")]
        public string PartnerName { get; set; }
    }
}