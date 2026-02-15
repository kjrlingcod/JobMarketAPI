# JobMarketApp – Marketplace Jobs API (2-tier, Dapper)

A simple marketplace API where **Customers** create Jobs, **Contractors** submit JobOffers, and Customers accept offers.
Built as a **2-tier** C# solution:
- **REST API** project (Controllers + Services + DTOs)
- **Persistence** project (Dapper repositories + models)

Includes:
- Input validation
- Pagination + search endpoints
- In-memory caching for frequently accessed “accounts”
- Unit tests for the service layer
- Database seeding for Customers and Contractors

---

## Tech Stack
- .NET 8 Web API
- Dapper (persistence layer)
- SQL Server
- xUnit + Moq (unit testing)
- AutoMapper
- IMemoryCache (cache-aside pattern)

---

## Solution Structure
- JobMarketApp.API -> Controllers, Services, DTOs, AutoMapper
- JobMarketApp.Persistence -> Dapper Repositories, Models, DB connection factory, seed scripts
- JobMarketApp.Tests -> Unit tests (Services)

## Configuration
Update your connection string in:
- `JobMarketApp.API/appsettings.json`
- `JobMarketApp.API/appsettings.Development.json`

Example:
```json
{
  "DatabaseName": "JobMarketDB",
  "ConnectionStrings": {
    "MasterConnection": "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;",
    "DefaultConnection": "Server=localhost;Database=JobMarketDB;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
````

---

## How to Run the Application

### Prerequisites
- .NET SDK 8.0+
- SQL Server (LocalDB or SQL Server)

---

### Run using Visual Studio
Open the solution in Visual Studio and click the **https** run button.  
Swagger will open automatically.