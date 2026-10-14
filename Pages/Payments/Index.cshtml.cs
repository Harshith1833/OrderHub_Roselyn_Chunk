using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Models;
using OrderHub.Services;

namespace OrderHub.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly DataStore _store;
    public IndexModel(DataStore store) => _store = store;

    public List<Order> Unpaid { get; private set; } = new();
    public List<Payment> History { get; private set; } = new();

    public void OnGet()
    {
        Unpaid = _store.GetOrders(OrderStatus.Pending);
        History = _store.GetPayments();
    }
}
