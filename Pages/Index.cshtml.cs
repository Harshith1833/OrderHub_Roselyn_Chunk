using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Services;

namespace OrderHub.Pages;

public class IndexModel : PageModel
{
    private readonly DataStore _store;
    public IndexModel(DataStore store) => _store = store;

    public DashboardStats Stats { get; private set; } = new();

    public void OnGet() => Stats = _store.GetStats();
}
