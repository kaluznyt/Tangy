using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tangy.Models
{
    public class OrderDetails
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId ")]
        public MenuItem MenuItem { get; set; }

        [Required]
        public double Price { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
