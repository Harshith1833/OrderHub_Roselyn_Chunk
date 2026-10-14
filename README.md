# OrderHub

Simple ASP.NET Core 8 (Razor Pages) app with three connected pages. No database or NuGet packages needed.

## Run
Requires the .NET 8 SDK (https://dotnet.microsoft.com/download).

    cd OrderHub
    dotnet run

Open the URL printed in the terminal (usually http://localhost:5000 or 5xxx).

## How the pages depend on each other
- **Orders -> Payments**: creating an order sends you straight to its payment page; each pending order has a "Pay" button.
- **Payments -> Orders**: paying creates a payment record and flips the order to *Paid*. Paid/cancelled orders can't be paid again, and paid orders can't be cancelled.
- **Dashboard**: reads from both (order counts by status, revenue from payments, outstanding amount, recent orders/payments).

Data is saved to `App_Data/data.json` (3 sample orders are seeded on first run). Delete that file to reset.

## Layout
- `Services/DataStore.cs`: all data + business rules
- `Models/`: Order, Payment
- `Pages/`: Index (Dashboard), Orders/, Payments/, Shared/_Layout
