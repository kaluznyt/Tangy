using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;
using Tangy.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tangy.Controllers.API
{
    [Route("api/[controller]")]
    public class CouponAPIController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponAPIController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(double orderTotal, string couponCode = null)
        {
            if (couponCode == null)
            {
                return BadRequest(orderTotal);
            }

            var coupon = _db.Coupon.FirstOrDefault(c => c.Name == couponCode);

            if (coupon == null)
            {
                return BadRequest(orderTotal);
            }

            if (coupon.MinimumAmount > orderTotal)
            {
                return BadRequest(orderTotal);
            }

            if (coupon.Type == Coupon.CouponType.Dollar)
            {
                orderTotal = orderTotal - coupon.Discount;
                return Ok(orderTotal);
            }

            if (coupon.Type == Coupon.CouponType.Percent)
            {
                orderTotal = orderTotal - (orderTotal * coupon.Discount / 100);
                return Ok(orderTotal);
            }

            return NotFound();
        }
    }
}
