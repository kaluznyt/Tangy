using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser)]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemViewModel { get; set; }

        public MenuItemsController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemViewModel = new MenuItemViewModel()
            {
                Category = _db.Category.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var menuItems = _db.MenuItem
                            .Include(m => m.Category)
                            .Include(m => m.SubCategory);
            return View(await menuItems.ToListAsync());
        }

        [HttpGet, ActionName("Create")]
        public IActionResult CreateGet()
        {
            return View(MenuItemViewModel);
        }


        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(MenuItemViewModel);
            }

            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            _db.MenuItem.Add(MenuItemViewModel.MenuItem);

            await _db.SaveChangesAsync();

            await RetrieveUserUploadedImageOrSetDefault();

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task RetrieveUserUploadedImageOrSetDefault()
        {
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemViewModel.MenuItem.Id);
            var webRootPath = _hostingEnvironment.WebRootPath;

            if (files?.Count > 0 && files[0]?.Length > 0)
            {
                var uploadPath = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream =
                    new FileStream(Path.Combine(uploadPath, $"{MenuItemViewModel.MenuItem.Id}{extension}"),
                        FileMode.Create))
                {
                    await files[0].CopyToAsync(filestream);
                }

                menuItemFromDb.Image = $@"\images\{MenuItemViewModel.MenuItem.Id}{extension}";
            }
            else
            {
                menuItemFromDb.Image = StaticDetails.DefaultFoodImage;
            }
        }

        private async Task RetrieveUserUploadedImageAndReplaceExistingOne(int menuItemId)
        {
            await DeleteExistingUserImage(menuItemId);

            await RetrieveUserUploadedImageOrSetDefault();
        }

        private async Task DeleteExistingUserImage(int menuItemId)
        {
            var menuItemFromDb =
                await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == menuItemId);

            string webRootPath = _hostingEnvironment.WebRootPath;

            var uploads = Path.Combine(webRootPath, "images");

            if (System.IO.File.Exists(menuItemFromDb.Image))
            {
                System.IO.File.Delete(menuItemFromDb.Image);
            }
        }

        public async Task<JsonResult> GetSubCategory(int categoryId)
        {
            var subCategories = await (from subCategory in _db.SubCategory
                                       where subCategory.CategoryId == categoryId
                                       select subCategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(p => p.Id == id);

            MenuItemViewModel.SubCategory = await _db.SubCategory
                    .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId)
                    .ToListAsync();

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id)
        {
            if (id != MenuItemViewModel.MenuItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

                    var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemViewModel.MenuItem.Id);

                    menuItemFromDb.Name = MenuItemViewModel.MenuItem.Name;
                    menuItemFromDb.Price = MenuItemViewModel.MenuItem.Price;
                    menuItemFromDb.Description = MenuItemViewModel.MenuItem.Description;
                    menuItemFromDb.SubCategoryId = MenuItemViewModel.MenuItem.SubCategoryId;
                    menuItemFromDb.CategoryId = MenuItemViewModel.MenuItem.CategoryId;
                    menuItemFromDb.Spicyness = MenuItemViewModel.MenuItem.Spicyness;

                    await RetrieveUserUploadedImageAndReplaceExistingOne(MenuItemViewModel.MenuItem.Id);

                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {

                }
            }

            MenuItemViewModel.SubCategory = _db.SubCategory
                .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId)
                .ToList();

            return View(MenuItemViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);
        }


        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> DeleteGet(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var menuItem = await _db.MenuItem.FindAsync(id);

            if (menuItem != null)
            {
                RemoveUserImagePreserveDefault(menuItem);

                _db.MenuItem.Remove(menuItem);

                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void RemoveUserImagePreserveDefault(MenuItem menuItem)
        {
            if (IsUserImage(menuItem))
            {
                var wwwroot = _hostingEnvironment.WebRootPath;
                var imagePath = Path.Combine(wwwroot, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }

        private static bool IsUserImage(MenuItem menuItem)
        {
            return menuItem.Image != StaticDetails.DefaultFoodImage;
        }
    }
}