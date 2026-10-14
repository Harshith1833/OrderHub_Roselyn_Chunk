using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Models;
using OrderHub.Services;

namespace OrderHub.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly DataStore _store;
    public IndexModel(DataStore store) => _store = store;

    [BindProperty(SupportsGet = true)]
    public OrderStatus? Status { get; set; }

    public List<Order> Orders { get; private set; } = new();

    public void OnGet() => Orders = _store.GetOrders(Status);

    public IActionResult OnPostCancel(int id)
    {
        var (ok, message) = _store.CancelOrder(id);
        TempData[ok ? "Message" : "Error"] = message;
        return RedirectToPage();
    }
}
