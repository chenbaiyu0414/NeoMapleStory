using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoMapleStory.Core.Database.Models
{
    public class PetModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public byte Level { get; set; }

        public short Closeness { get; set; }

        public byte Fullness { get; set; }

        public int UniqueId { get; set; }
    }
}
