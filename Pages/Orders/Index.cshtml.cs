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

    [BindProperty(Name = "pageNumber", SupportsGet = true)]
    public int PageNo { get; set; } = 1;

    public List<Order> Orders { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; } = 1;

    public void OnGet()
    {
        var size = DataStore.OrdersPageSize;
        var (_, total) = _store.GetOrdersPage(Status, 1, size);
        TotalCount = total;
        TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)size));
        PageNo = Math.Min(Math.Max(PageNo, 1), TotalPages);
        Orders = _store.GetOrdersPage(Status, PageNo, size).items;
    }

    public IActionResult OnPostCancel(int id)
    {
        var (ok, message) = _store.CancelOrder(id);
        TempData[ok ? "Message" : "Error"] = message;
        return RedirectToPage();
    }
}
