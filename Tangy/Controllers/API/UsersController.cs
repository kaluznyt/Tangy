using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;

namespace Tangy.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Get(string query = null)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var users = _db.ApplicationUsers.Where(u => u.Email.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                    .Select(u =>
                    new
                    {
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName
                    });

                return Ok(users);
            }

            return Ok();
        }
    }
}