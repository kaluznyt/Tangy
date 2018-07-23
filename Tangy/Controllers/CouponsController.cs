using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser)]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var uploadedPicture = await RetrieveUploadedImage();
                coupon.Picture = uploadedPicture;
                _db.Coupon.Add(coupon);

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        private async Task<byte[]> RetrieveUploadedImage()
        {
            var uploadedFiles = HttpContext.Request.Form.Files;

            if (uploadedFiles?.Count > 0 && uploadedFiles[0]?.Length > 0)
            {
                using (var fileReadStream = uploadedFiles[0].OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    await fileReadStream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return BadRequest();
            }

            var couponFromDb = await _db.Coupon.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                var uploadedPicture = await RetrieveUploadedImage();

                if (uploadedPicture != null)
                {
                    couponFromDb.Picture = uploadedPicture;
                }

                couponFromDb.Name = coupon.Name;
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.IsActive = coupon.IsActive;
                couponFromDb.Type = coupon.Type;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> DeleteGet(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

