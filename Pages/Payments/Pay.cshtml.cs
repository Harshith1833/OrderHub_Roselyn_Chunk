using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Models;
using OrderHub.Services;

namespace OrderHub.Pages.Payments;

public class PayModel : PageModel
{
    private readonly DataStore _store;
    public PayModel(DataStore store) => _store = store;

    public Order? Order { get; private set; }
    public string[] Methods => DataStore.PaymentMethods;

    [BindProperty] public int OrderId { get; set; }
    [BindProperty] public string Method { get; set; } = "Card";

    public IActionResult OnGet(int orderId)
    {
        Order = _store.GetOrder(orderId);
        if (Order.Status != OrderStatus.Pending)
        {
            TempData["Error"] = $"Order #{orderId} is already {Order.Status}.";
            return RedirectToPage("/Orders/Index");
        }
        OrderId = orderId;
        return Page();
    }

    public IActionResult OnPost()
    {
        var (ok, message) = _store.Pay(OrderId, Method);
        TempData[ok ? "Message" : "Error"] = message;
        // Payments -> Orders dependency: paying flips the order to Paid, which the Orders page and Dashboard reflect.
        return RedirectToPage(ok ? "/Orders/Index" : "/Payments/Index");
    }
}
