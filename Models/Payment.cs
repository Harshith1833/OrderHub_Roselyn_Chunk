namespace OrderHub.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }          // every payment belongs to an order
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Card";
    public string Reference { get; set; } = "";
    public DateTime PaidAt { get; set; } = DateTime.Now;
}
