using System.Collections.Generic;

namespace Tangy.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public List<ShoppingCart> OrderItems { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
    }
}
