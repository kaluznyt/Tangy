using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .Where(user => user.Id != this.User.FindFirstValue(ClaimTypes.NameIdentifier))
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}