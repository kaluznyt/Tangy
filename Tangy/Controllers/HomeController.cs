using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;

namespace Tangy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var indexViewModel = new IndexViewModel
            {
                MenuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Categories = _db.Category.OrderBy(c => c.DisplayOrder),
                Coupons = await _db.Coupon.Where(c => c.IsActive).ToListAsync()
            };

            return View(indexViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var shoppingCart = await RetrieveShoppingCartFromDatabase(id);

            return View(shoppingCart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;

            if (ModelState.IsValid)
            {
                var userId = this.User.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = userId.Value;

                var existingCart = await _db.ShoppingCarts
                    .Where(c => c.ApplicationUserId == userId.Value && c.MenuItemId == shoppingCart.MenuItemId)
                    .FirstOrDefaultAsync();

                if (existingCart == null)
                {
                    _db.ShoppingCarts.Add(shoppingCart);
                }
                else
                {
                    shoppingCart.Count = existingCart.Count + shoppingCart.Count;
                }

                await _db.SaveChangesAsync();

                var cartItemsCount = _db.ShoppingCarts
                    .Count(c => c.ApplicationUserId == shoppingCart.ApplicationUserId);

                HttpContext.Session.SetInt32("CartCount", cartItemsCount);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var cart = await RetrieveShoppingCartFromDatabase(shoppingCart.MenuItemId);

                return View(cart);
            }
        }

        private async Task<ShoppingCart> RetrieveShoppingCartFromDatabase(int id)
        {
            var menuItem = await _db.MenuItem.Include(m => m.Category)
                .Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();

            var shoppingCart = new ShoppingCart
            {
                MenuItem = menuItem,
                MenuItemId = menuItem.Id,
            };
            return shoppingCart;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
