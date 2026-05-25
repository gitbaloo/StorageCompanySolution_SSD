# Storage Company Showcase API

This is a showcase-ready ASP.NET Core Web API for a storage unit rental company.

The project demonstrates:

- A customer-to-business storage rental flow
- A SOLID-friendly separation between API, Core, and Infrastructure
- Mock in-memory data instead of a real database
- Scalar UI for testing all endpoints
- Entities for customers, facilities, storage units, reservations, rentals, payments, invoices, access codes, and support requests

## Target framework

The solution targets **.NET 10** because Visual Studio 2026 is expected to be used with .NET 10-era projects.

## Solution structure

```text
StorageCompanySolution
│
├── StorageCompany.Api
│   ├── Controllers
│   ├── Requests
│   ├── Responses
│   ├── Middleware
│   ├── Extensions
│   ├── Program.cs
│   └── appsettings.json
│
├── StorageCompany.Core
│   ├── Entities
│   ├── Enums
│   ├── Interfaces
│   │   ├── Repositories
│   │   └── Services
│   ├── Services
│   ├── Exceptions
│   └── Validators
│
├── StorageCompany.Infrastructure
│   ├── Data
│   └── Repositories
│
├── StorageCompany.Tests
│   └── CoreTests
│
└── docs
    ├── api-endpoints.md
    └── database-schema.md
```

## How to run

1. Unzip the folder.
2. Open `StorageCompanySolution.sln` in Visual Studio 2026.
3. Restore NuGet packages.
4. Set `StorageCompany.Api` as the startup project.
5. Run the API.
6. Scalar should open automatically at:

```text
https://localhost:7042/scalar
```

You can also run it from the terminal:

```bash
dotnet restore
dotnet run --project StorageCompany.Api
```
### Secrets

#### Encryption key secret

1. Generate a random key

```bash
openssl rand -base64 32
```

2. Set the secret

```bash
dotnet user-secrets set "AppOptions:EncryptionKey" "your-random-key-here"
```
Or

```bash
dotnet user-secrets set "AppOptions:EncryptionKey" "your-random-key-here" --project StorageCompany.Api
```

## Useful seeded demo IDs

These IDs are useful when testing POST requests in Scalar.

### Customers

```text
Anna Jensen  = 10000000-0000-0000-0000-000000000001
Peter Nielsen = 10000000-0000-0000-0000-000000000002
```

### Facilities

```text
Copenhagen = 20000000-0000-0000-0000-000000000001
Aarhus     = 20000000-0000-0000-0000-000000000002
Odense     = 20000000-0000-0000-0000-000000000003
```

### Available storage units

```text
CPH-A101 = 40000000-0000-0000-0000-000000000001
CPH-C010 = 40000000-0000-0000-0000-000000000003
AAR-A014 = 40000000-0000-0000-0000-000000000004
AAR-D001 = 40000000-0000-0000-0000-000000000005
```

### Existing active rental

```text
Rental = 50000000-0000-0000-0000-000000000001
Access code = 123456
```

## Example Scalar test flow

### 1. View available units

```http
GET /api/storage-units/available
```

### 2. Create a reservation

```http
POST /api/reservations
```

Example body:

```json
{
  "customerId": "10000000-0000-0000-0000-000000000002",
  "storageUnitId": "40000000-0000-0000-0000-000000000004",
  "moveInDateUtc": "2026-05-01T00:00:00Z"
}
```

### 3. Convert reservation to rental

```http
POST /api/rentals/from-reservation
```

Example body:

```json
{
  "reservationId": "paste-reservation-id-here"
}
```

### 4. Get rental access code

```http
GET /api/rentals/{rentalId}/access-code
```

### 5. Generate invoice

```http
POST /api/invoices
```

Example body:

```json
{
  "rentalId": "paste-rental-id-here",
  "dueDateUtc": "2026-05-15T00:00:00Z"
}
```

### 6. Create mock payment

```http
POST /api/payments
```

Example body:

```json
{
  "rentalId": "paste-rental-id-here",
  "invoiceId": "paste-invoice-id-here",
  "amount": 299,
  "paymentMethod": "Card"
}
```

## Notes

This is intentionally not connected to a real database. The `StorageCompany.Infrastructure` project contains an in-memory mock database so the API can be demonstrated immediately.

A future real database version could replace the in-memory repositories with Entity Framework Core repositories without changing the Core business services.
