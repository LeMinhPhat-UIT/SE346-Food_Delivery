# Data Seeding

The backend now seeds default data for all database-backed services except CatalogService and OrderService.

## AuthenticationService

Seed setup:

- `Backend/Services/AuthenticationService/Persistences/SeedData.cs`
- `Backend/Services/AuthenticationService/Persistences/AuthenticationDbContext.cs`

Seeded records:

- 4 roles (`Customer`, `Merchant`, `Shipper`, `Admin`)
- 1 admin account (`admin@fooddelivery.local`) assigned to `Admin`

## UserService

Seed setup:

- `Backend/Services/UserService/Persistences/Seeding.cs`
- `Backend/Services/UserService/Persistences/UserDbContext.cs`

Seeded records:

- 2 users (`Seeded Customer`, `Seeded Merchant Owner`)
- 2 user addresses (one default address per user)
- 1 approved merchant linked to `Seeded Merchant Owner`
- 1 merchant store address linked to that merchant

## DeliveryService

Seed setup:

- `Backend/Services/DeliveryService/Persistences/SeedData.cs`
- `Backend/Services/DeliveryService/Persistences/DeliveryDbContext.cs`
- `Backend/Services/DeliveryService/Program.cs` (`EnsureCreatedAsync`)

Seeded records:

- 1 delivery tracking
- 1 shipper assignment
- 1 shipper location history record
- 1 shipper availability record

## NotificationService

Seed setup:

- `Backend/Services/NotificationService/Persistences/SeedData.cs`
- `Backend/Services/NotificationService/Persistences/NotificationDbContext.cs`
- `Backend/Services/NotificationService/Program.cs` (`EnsureCreatedAsync`)

Seeded records:

- 1 notification
- 1 user device

## Notes

- Seeding runs during model creation and is applied when each service initializes its database with `EnsureCreatedAsync`.
- Seeded IDs are fixed GUIDs so local environments remain deterministic.
Add database seeding support for every microservice except the Catalog Service and Order Service.

Requirements:

Create a dedicated seeding file/class inside the Persistences folder of each applicable service.
The seeding implementation should automatically populate the database with initial development/test data when the application starts (if the database is empty).
Seeded data must be:
logically consistent,
realistic for the domain of that service,
properly related through foreign keys/references,
varied enough to support testing and demo scenarios.
Avoid duplicate or meaningless placeholder data.
Follow the existing architecture, coding conventions, dependency injection patterns, and naming conventions already used in the solution.
Ensure the seeding process is idempotent (running multiple times must not create duplicate records).
Keep the code clean, maintainable, and separated from business logic.
If enums, roles, statuses, or predefined entities exist, seed them appropriately as well.
Register and invoke the seeding process in the correct startup/application initialization flow for each service.

Do not modify the Catalog Service or Order Service.
