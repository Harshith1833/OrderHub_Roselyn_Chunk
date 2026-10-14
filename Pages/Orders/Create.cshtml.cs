using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Models;
using OrderHub.Services;

namespace OrderHub.Pages.Orders;

public class CreateModel : PageModel
{
    private readonly DataStore _store;
    public CreateModel(DataStore store) => _store = store;

    [BindProperty]
    public Order Order { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        ModelState.Remove("Order.Status");
        if (!ModelState.IsValid) return Page();

        var created = _store.AddOrder(Order);
        TempData["Message"] = $"Order #{created.Id} created. Complete the payment below.";
        // Orders -> Payments dependency: a new order goes straight to its payment page.
        return RedirectToPage("/Payments/Pay", new { orderId = created.Id });
    }
}
