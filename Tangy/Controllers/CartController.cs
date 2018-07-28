using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty] public OrderDetailsViewModel OrderDetailsViewModel { get; set; }

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

                    orderItem.MenuItem.Description =
                        $"{(orderItem.MenuItem.Description.Length > 100 ? orderItem.MenuItem.Description.Substring(0, 99) : orderItem.MenuItem.Description)}...";
                }
            }

            OrderDetailsViewModel.OrderHeader.PickupTime = DateTime.Now;

            return View(OrderDetailsViewModel);
        }

        public async Task<IActionResult> IncrementCartItem(int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(c => c.Id == cartId);
            cart.Count++;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecrementCartItem(int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(c => c.Id == cartId);

            if (cart.Count == 1)
            {
                _db.ShoppingCarts.Remove(cart);
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCarts.Count(u => u.ApplicationUserId == cart.ApplicationUserId);
                HttpContext.Session.SetInt32("CartCount", count);
            }
            else
            {
                cart.Count--;

                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            OrderDetailsViewModel.OrderItems = _db.ShoppingCarts.Where(c => c.ApplicationUserId == userId).ToList();
            OrderDetailsViewModel.OrderHeader.OrderDate = DateTime.Now;
            OrderDetailsViewModel.OrderHeader.UserId = userId;
            OrderDetailsViewModel.OrderHeader.Status = StaticDetails.OrderStatus.Submitted;

            _db.OrderHeader.Add(OrderDetailsViewModel.OrderHeader);

            await _db.SaveChangesAsync();

            foreach (var cartItem in OrderDetailsViewModel.OrderItems)
            {
                cartItem.MenuItem = _db.MenuItem.FirstOrDefault(m => m.Id == cartItem.MenuItemId);

                var orderDetails = new OrderDetails
                {
                    MenuItemId = cartItem.MenuItemId,
                    OrderId = OrderDetailsViewModel.OrderHeader.Id,
                    Description = cartItem.MenuItem.Description,
                    Name = cartItem.MenuItem.Name,
                    Price = cartItem.MenuItem.Price,
                    Count = cartItem.Count
                };

                _db.OrderDetails.Add(orderDetails);
            }

            _db.ShoppingCarts.RemoveRange(OrderDetailsViewModel.OrderItems);

            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("CartCount", 0);

            return RedirectToAction("Confirm", "Order", new { id = OrderDetailsViewModel.OrderHeader.Id });
        }
    }
}