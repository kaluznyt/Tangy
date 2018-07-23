using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
                Coupons = await _db.Coupon.Where(c=>c.IsActive).ToListAsync()
            };

            return View(indexViewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _db.MenuItem.Include(m => m.Category)
                .Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();

            var shoppingCart = new ShoppingCart
            {
                MenuItem = menuItem,
                MenuItemId = menuItem.Id,
            };

            return View(shoppingCart);

        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
