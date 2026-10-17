// Configures and runs a .NET Core web application using Razor Pages, 
// registering a thread-safe, shared 'DataStore' as a singleton lifecycle service 
// to manage state across the Dashboard, Orders, and Payments modules.

using OrderHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
// One shared store for the whole app: Dashboard, Orders and Payments all read/write through it.
builder.Services.AddSingleton<DataStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
