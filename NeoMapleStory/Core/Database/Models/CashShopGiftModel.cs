using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoMapleStory.Core.Database.Models
{
    [Table("CashShopGifts")]
    public class CashShopGiftModel
    {
        [Key]
        public Guid Id { get; set; }

        public int AId { get; set; }

        public string Sender { get; set; }

        public int ItemId { get; set; }

        public int Sn { get; set; }

        public short Quantity { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string Message { get; set; }

        public int RingUniqueId { get; set; }
    }
}
