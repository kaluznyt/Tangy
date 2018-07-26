﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Areas.Identity.Data;
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
            var orderDetailsViewModel = await OrderDetailsViewModels(o => o.Status == StaticDetails.OrderStatus.Submitted || o.Status == StaticDetails.OrderStatus.InProcess);

            return View(orderDetailsViewModel);
        }

        private async Task<List<OrderDetailsViewModel>> OrderDetailsViewModels(Func<OrderHeader, bool> orderFilter)
        {
            var orderHeaders = await _db.OrderHeader
                .Where(o => orderFilter(o))
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

            return orderDetailsViewModels;
        }


        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            await ChangeOrderStatus(orderId, StaticDetails.OrderStatus.InProcess);

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            await ChangeOrderStatus(orderId, StaticDetails.OrderStatus.Cancelled);

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            await ChangeOrderStatus(orderId, StaticDetails.OrderStatus.Ready);

            return RedirectToAction("ManageOrder", "Order");
        }

        private async Task ChangeOrderStatus(int orderId, string status)
        {
            var order = await _db.OrderHeader.FindAsync(orderId);
            order.Status = status;

            await _db.SaveChangesAsync();
        }


        public async Task<IActionResult> OrderPickup(string searchOrder = null, string searchPhone = null, string searchEmail = null)
        {
            TangyUser user;

            Func<OrderHeader, bool> searchQuery = null;

            if (!string.IsNullOrWhiteSpace(searchPhone) || !string.IsNullOrWhiteSpace(searchEmail) || !string.IsNullOrWhiteSpace(searchOrder))
            {
                user = _db.ApplicationUsers.FirstOrDefault(u =>
                    (!string.IsNullOrWhiteSpace(searchEmail) && (string.Equals(u.Email, searchEmail, StringComparison.InvariantCultureIgnoreCase))) &&
                    (!string.IsNullOrWhiteSpace(searchPhone) && (string.Equals(u.PhoneNumber, searchPhone, StringComparison.InvariantCultureIgnoreCase))));

                searchQuery = o => (string.IsNullOrWhiteSpace(searchOrder) || (o.Id.ToString() == searchOrder)) &&
                                   (user == null || (user != null && o.UserId == user.Id));
            }

            searchQuery = searchQuery ?? (o => o.Status == StaticDetails.OrderStatus.Ready);

            var orderPickupViewModel = await OrderDetailsViewModels(searchQuery);

            return View(orderPickupViewModel);
        }
    }
}