using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
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
        const int PageSize = 2;

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
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderHeaders = await _db.OrderHeader.Where(u => u.UserId == userId)
                                        .OrderByDescending(u => u.OrderDate)
                                        .Skip((productPage - 1) * PageSize)
                                        .Take(PageSize)
                                        .ToListAsync();

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

            var orderListViewModel = new OrderListViewModel
            {
                Orders = orderDetailsViewModels,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = productPage, 
                    ItemsPerPage = PageSize,
                    TotalItems = await _db.OrderHeader.CountAsync(u => u.UserId == userId)
                }
              
            };

            return View(orderListViewModel);
        }


        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> ManageOrder()
        {
            var orderDetailsViewModel = await OrderDetailsViewModels(o =>
                o.Status == StaticDetails.OrderStatus.Submitted || o.Status == StaticDetails.OrderStatus.InProcess);

            return View(orderDetailsViewModel);
        }

        private async Task<List<OrderDetailsViewModel>> OrderDetailsViewModels(Func<OrderHeader, bool> orderFilter)
        {
            var orderHeaders = await _db.OrderHeader
                .Where(o => orderFilter(o))
                .OrderByDescending(u => u.PickupTime)
                .ToListAsync();

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


        public async Task<IActionResult> OrderPickup(string searchOrder = null, string searchPhone = null,
            string searchEmail = null)
        {
            TangyUser user;

            Func<OrderHeader, bool> searchQuery = null;

            if (!string.IsNullOrWhiteSpace(searchPhone) || !string.IsNullOrWhiteSpace(searchEmail) ||
                !string.IsNullOrWhiteSpace(searchOrder))
            {
                user = _db.ApplicationUsers.FirstOrDefault(u =>
                    (!string.IsNullOrWhiteSpace(searchEmail) && (string.Equals(u.Email, searchEmail,
                         StringComparison.InvariantCultureIgnoreCase))) &&
                    (!string.IsNullOrWhiteSpace(searchPhone) && (string.Equals(u.PhoneNumber, searchPhone,
                         StringComparison.InvariantCultureIgnoreCase))));

                searchQuery = o => (string.IsNullOrWhiteSpace(searchOrder) || (o.Id.ToString() == searchOrder)) &&
                                   (user == null || (user != null && o.UserId == user.Id));
            }

            searchQuery = searchQuery ?? (o => o.Status == StaticDetails.OrderStatus.Ready);

            var orderPickupViewModel = await OrderDetailsViewModels(searchQuery);

            return View(orderPickupViewModel);
        }

        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> OrderPickupDetails(int orderId)
        {
            var orderDetailsViewModels = await OrderDetailsViewModels(o => o.Id == orderId);
            var orderDetailsViewModel = orderDetailsViewModels.FirstOrDefault();

            if (orderDetailsViewModel != null)
            {
                orderDetailsViewModel.OrderHeader.ApplicationUser =
                    _db.ApplicationUsers.Find(orderDetailsViewModel.OrderHeader.UserId);

                return View(orderDetailsViewModel);
            }

            return View(null);
        }

        [HttpPost]
        [ActionName("OrderPickupDetails")]
        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> OrderPickupDetailsPost(int orderId)
        {
            await ChangeOrderStatus(orderId, StaticDetails.OrderStatus.Completed);

            return RedirectToAction("OrderPickup", "Order");
        }

        [Authorize(Roles = StaticDetails.AdminEndUser)]
        public async Task<IActionResult> DownloadOrderDetails()
        {
            var downloadOrderDetailsVM = new DownloadOrderDetailsViewModel();
            downloadOrderDetailsVM.FromDate = DateTime.Today.AddDays(-7);
            downloadOrderDetailsVM.ToDate = DateTime.Today.AddDays(1).AddMinutes(-1);

            return View(downloadOrderDetailsVM);
        }

        [HttpPost]
        [ActionName(nameof(DownloadOrderDetails))]
        [Authorize(Roles = StaticDetails.AdminEndUser)]

        public async Task<IActionResult> DownloadOrderDetailsPost(DownloadOrderDetailsViewModel downloadOrderDetailsVM)
        {
            var orders = await _db.OrderHeader.Where(o => o.OrderDate >= downloadOrderDetailsVM.FromDate
                                                        && o.OrderDate <= downloadOrderDetailsVM.ToDate)
                                .ToListAsync();

            var csv = string.Join(Environment.NewLine, orders.Select(o => $"\"{o.Id}\",\"{o.OrderDate}\",\"{o.OrderTotal}\",\"{o.Status}\"").ToList());

            return File(Encoding.ASCII.GetBytes(csv), "text/csv", "OrderDetails.csv");
        }
    }
}