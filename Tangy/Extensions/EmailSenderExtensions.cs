using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Tangy.Utility;

namespace Tangy.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public static Task SendOrderStatusAsync(this IEmailSender emailSender, string email, string orderNumber, string status)
        {
            var subject = string.Empty;
            var message = string.Empty;

            if (status == StaticDetails.OrderStatus.Cancelled)
            {
                subject = "Order Cancelled";
                message = "Order Number " + orderNumber + " has been Cancelled.";
            }

            if (status == StaticDetails.OrderStatus.Submitted)
            {
                subject = "Order Created successfully";
                message = "Order Number " + orderNumber + " has been Created successfully.";
            }

            if (status == StaticDetails.OrderStatus.Ready)
            {
                subject = "Order Ready for Pickup";
                message = "Order Number " + orderNumber + " Order Ready for Pickup.";
            }

            return emailSender.SendEmailAsync(email, subject, message);

        }
    }
}
