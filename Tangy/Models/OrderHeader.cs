using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tangy.Areas.Identity.Data;

namespace Tangy.Models
{
    public class OrderHeader
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual TangyUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public double OrderTotal { get; set; }

        [Required]
        public DateTime PickupTime { get; set; }

        public string CouponCode { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
    }
}
