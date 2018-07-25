using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderHeader = await _db.OrderHeader
                    .SingleOrDefaultAsync(o => o.Id == id && o.UserId == userId),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()
            };


            return View(orderDetailsViewModel);
        }


        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderHeaders = await _db.OrderHeader.Where(u => u.UserId == userId).OrderByDescending(u => u.OrderDate).ToListAsync();

            var orderDetailsViewModels = new List<OrderDetailsViewModel>();

            foreach (var orderHeader in orderHeaders)
            {
                var orderDetailsViewModel = new OrderDetailsViewModel
                {
                    OrderHeader = orderHeader,
                    OrderDetails = await _db.OrderDetails
                            .Where(o => o.OrderId == orderHeader.Id).ToListAsync()
                };

                orderDetailsViewModels.Add(orderDetailsViewModel);
            }

            return View(orderDetailsViewModels);
        }


        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> ManageOrder()
        {
            var orderHeaders = await _db.OrderHeader
                .Where(o => o.Status == StaticDetails.OrderStatus.Submitted || o.Status == StaticDetails.OrderStatus.InProcess)
                .OrderByDescending(u => u.PickupTime).ToListAsync();

            var orderDetailsViewModels = new List<OrderDetailsViewModel>();

            foreach (var orderHeader in orderHeaders)
            {
                var orderDetailsViewModel = new OrderDetailsViewModel
                {
                    OrderHeader = orderHeader,
                    OrderDetails = await _db.OrderDetails
                        .Where(o => o.OrderId == orderHeader.Id).ToListAsync()
                };

                orderDetailsViewModels.Add(orderDetailsViewModel);
            }

            return View(orderDetailsViewModels);
        }


    }
}