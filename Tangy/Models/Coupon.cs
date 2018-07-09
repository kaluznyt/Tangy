using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models
{
    public class Coupon
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public CouponType Type { get; set; }

        public enum CouponType { Percent = 0, Dollar = 1 }

        [Required]
        public double Discount { get; set; }
        [Required]
        public double MinimumAmount { get; set; }
        public byte[] Picture { get; set; }
        public bool IsActive { get; set; }
    }
}
