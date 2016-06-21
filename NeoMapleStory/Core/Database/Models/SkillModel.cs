using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("Skills")]
    public class SkillModel
    {
        [Key]
        public int Id { get; set; }

        public int CId { get; set; }

        public int SkillId { get; set; }

        public byte Level { get; set; }

        public byte MasterLevel { get; set; }
    }
}
