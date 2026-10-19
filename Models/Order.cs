using System.ComponentModel.DataAnnotations;

namespace OrderHub.Models;

public enum OrderStatus { Pending, Paid, Cancelled }

public class Order
{
    public int Id { get; set; }

    [Display(Name = "Customer")]
    public string? CustomerName { get; set; } = "";

    [Required]
    public string Product { get; set; } = "";

    public int Quantity { get; set; } = 1;

    [Range(0.01, 1000000)]
    [Display(Name = "Unit price")]
    public decimal UnitPrice { get; set; }

    public decimal Total => Math.Round(Quantity * UnitPrice);

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
