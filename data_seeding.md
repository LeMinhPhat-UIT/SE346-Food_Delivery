# Data Seeding

UserService now seeds a minimal default dataset on database creation via `Seeding.InitializeData` in:

- `Backend/Services/UserService/Persistences/Seeding.cs`
- `Backend/Services/UserService/Persistences/UserDbContext.cs`

## Seeded records

- 2 users (`Seeded Customer`, `Seeded Merchant Owner`)
- 2 user addresses (one default address per user)
- 1 approved merchant linked to `Seeded Merchant Owner`
- 1 merchant store address linked to that merchant

## Notes

- Seeding runs when UserService starts and `EnsureCreatedAsync` creates the database.
- All seeded IDs are fixed GUIDs to keep local environments deterministic.
