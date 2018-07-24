using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;

namespace Tangy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public OrderDetailsViewModel OrderDetailsViewModel { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            OrderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderHeader = new OrderHeader()
            };

            OrderDetailsViewModel.OrderHeader.OrderTotal = 0;

            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var cart = _db.ShoppingCarts.Where(c => c.ApplicationUserId == userId).ToList();

            OrderDetailsViewModel.OrderItems = cart;

            foreach (var orderItem in OrderDetailsViewModel.OrderItems)
            {
                orderItem.MenuItem = _db.MenuItem.FirstOrDefault(m => m.Id == orderItem.MenuItemId);

                if (orderItem.MenuItem != null)
                {
                    OrderDetailsViewModel.OrderHeader.OrderTotal += (orderItem.MenuItem.Price * orderItem.Count);

                    orderItem.MenuItem.Description = $"{(orderItem.MenuItem.Description.Length > 100 ? orderItem.MenuItem.Description.Substring(0, 99) : orderItem.MenuItem.Description)}...";
                }
            }

            OrderDetailsViewModel.OrderHeader.PickupTime = DateTime.Now;

            return View(OrderDetailsViewModel);
        }
    }
}