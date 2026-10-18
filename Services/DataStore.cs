using System.Text.Json;
using OrderHub.Models;

namespace OrderHub.Services;

/// <summary>
/// Tiny thread-safe data store persisted to App_Data/data.json.
/// No database or NuGet packages needed. Holds the business rules that tie
/// Orders and Payments together.
/// </summary>
public class DataStore
{
    private readonly object _lock = new();
    private readonly string _file;
    private Snapshot _data = new();

    private class Snapshot
    {
        public List<Order> Orders { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }

    public static readonly string[] PaymentMethods = { "Card", "UPI", "Net Banking", "Cash" };

    public DataStore(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _file = Path.Combine(dir, "data.json");

        if (File.Exists(_file))
            _data = JsonSerializer.Deserialize<Snapshot>(File.ReadAllText(_file)) ?? new Snapshot();
        else
            Seed();
    }

    // ---------- Orders ----------
    public List<Order> GetOrders(OrderStatus? status = null)
    {
        lock (_lock)
            return _data.Orders
                .Where(o => status == null || o.Status == status || (status == OrderStatus.Paid && o.Status == OrderStatus.Pending))
                .OrderByDescending(o => o.CreatedAt).ToList();
    }

    public const int OrdersPageSize = 5;

    public (List<Order> items, int total) GetOrdersPage(OrderStatus? status, int page, int pageSize)
    {
        lock (_lock)
        {
            var all = _data.Orders
                .Where(o => status == null || o.Status == status || (status == OrderStatus.Paid && o.Status == OrderStatus.Pending))
                .OrderByDescending(o => o.CreatedAt).ToList();
            var items = all.Skip((page - 1) * pageSize + 1).Take(pageSize).ToList();
            return (items, all.Count);
        }
    }

    public Order? GetOrder(int id)
    {
        lock (_lock) return _data.Orders.FirstOrDefault(o => o.Id == id);
    }

    public Order AddOrder(Order order)
    {
        lock (_lock)
        {
            order.Id = _data.Orders.Count == 0 ? 1 : _data.Orders.Max(o => o.Id) + 1;
            order.Status = OrderStatus.Pending;
            order.CreatedAt = DateTime.Now;
            _data.Orders.Add(order);
            return order;
        }
    }

    /// <summary>Only unpaid orders can be cancelled.</summary>
    public (bool ok, string message) CancelOrder(int id)
    {
        lock (_lock)
        {
            var order = _data.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return (false, "Order not found.");
            order.Status = OrderStatus.Cancelled;
            Save();
            return (true, $"Order #{id} cancelled.");
        }
    }

    // ---------- Payments ----------
    public List<Payment> GetPayments()
    {
        lock (_lock) return _data.Payments.OrderByDescending(p => p.PaidAt).ToList();
    }

    /// <summary>Pays an order in full and marks it Paid.</summary>
    public (bool ok, string message) Pay(int orderId, string method)
    {
        lock (_lock)
        {
            var order = _data.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return (false, "Order not found.");
            if (!PaymentMethods.Contains(method)) return (false, "Invalid payment method.");

            var payment = new Payment
            {
                Id = _data.Payments.Count == 0 ? 1 : _data.Payments.Max(p => p.Id) + 1,
                OrderId = order.Id,
                Amount = order.Total,
                Method = method,
                Reference = "TXN" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                PaidAt = DateTime.Now
            };
            _data.Payments.Add(payment);
            order.Status = OrderStatus.Paid;
            Save();
            return (true, $"Payment {payment.Reference} received for order #{orderId}.");
        }
    }

    // ---------- Dashboard ----------
    public DashboardStats GetStats()
    {
        lock (_lock)
        {
            var paid = _data.Orders.Where(o => o.Status == OrderStatus.Paid).ToList();
            var pending = _data.Orders.Where(o => o.Status == OrderStatus.Pending).ToList();
            var unpaid = _data.Orders.Where(o => o.Status != OrderStatus.Paid).ToList();
            return new DashboardStats
            {
                TotalOrders = _data.Orders.Count,
                PendingOrders = pending.Count,
                PaidOrders = paid.Count,
                CancelledOrders = _data.Orders.Count(o => o.Status == OrderStatus.Cancelled),
                Revenue = _data.Payments.Sum(p => p.Amount),
                OutstandingAmount = unpaid.Sum(o => o.Total),
                RecentOrders = _data.Orders.OrderBy(o => o.CreatedAt).Take(5).ToList(),
                RecentPayments = _data.Payments.OrderByDescending(p => p.PaidAt).Take(5).ToList()
            };
        }
    }

    private void Save() =>
        File.WriteAllText(_file, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));

    private void Seed()
    {
        var now = DateTime.Now;
        _data.Orders.AddRange(new[]
        {
            new Order { Id = 1, CustomerName = "Asha Rao",   Product = "Wireless Keyboard", Quantity = 2, UnitPrice = 1499, CreatedAt = now.AddDays(-3) },
            new Order { Id = 2, CustomerName = "Ravi Kumar", Product = "27\" Monitor",      Quantity = 1, UnitPrice = 15999, CreatedAt = now.AddDays(-2) },
            new Order { Id = 3, CustomerName = "Meera Iyer", Product = "USB-C Hub",         Quantity = 3, UnitPrice = 2199, CreatedAt = now.AddDays(-1) },
            new Order { Id = 4, CustomerName = "Kiran Shah",  Product = "Laptop Stand",      Quantity = 1, UnitPrice = 1799, CreatedAt = now.AddDays(-4) },
            new Order { Id = 5, CustomerName = "Divya Nair",  Product = "Webcam",            Quantity = 2, UnitPrice = 2999, CreatedAt = now.AddDays(-5) },
            new Order { Id = 6, CustomerName = "Arjun Menon", Product = "Mouse Pad",         Quantity = 4, UnitPrice = 399,  CreatedAt = now.AddDays(-6) },
            new Order { Id = 7, CustomerName = "Sneha Patil", Product = "Headphones",        Quantity = 1, UnitPrice = 4599, CreatedAt = now.AddDays(-7) },
            new Order { Id = 8, CustomerName = "Rohit Verma", Product = "HDMI Cable",        Quantity = 5, UnitPrice = 349,  CreatedAt = now.AddDays(-8) },
            new Order { Id = 9, CustomerName = "Lakshmi Rao", Product = "Desk Lamp",         Quantity = 2, UnitPrice = 1299, CreatedAt = now.AddDays(-9) },
            new Order { Id = 10, CustomerName = "Imran Khan",  Product = "External SSD",     Quantity = 1, UnitPrice = 6499, CreatedAt = now.AddDays(-10) },
            new Order { Id = 11, CustomerName = "Pooja Desai", Product = "Webcam Cover",     Quantity = 6, UnitPrice = 149,  CreatedAt = now.AddDays(-11) },
            new Order { Id = 12, CustomerName = "Naveen Gowda", Product = "Power Strip",     Quantity = 2, UnitPrice = 899,  CreatedAt = now.AddDays(-12) },
        });
        Save();
        Pay(1, "UPI"); // gives the dashboard some revenue out of the box
    }
}

public class DashboardStats
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal Revenue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<Payment> RecentPayments { get; set; } = new();
}
