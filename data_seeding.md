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