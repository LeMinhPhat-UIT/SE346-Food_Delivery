# Food Delivery Backend

This folder contains the backend for the Food Delivery project. It is a microservice-based backend with an API gateway, .NET services, Node.js/TypeScript services, PostgreSQL databases, Redis instances, and RabbitMQ for event messaging.

## Project Layout

```text
Backend/
  Services/
    ApiGateway/              .NET YARP reverse proxy
    AuthenticationService/   .NET auth, JWT, roles, OTP verification
    UserService/             .NET users, merchants, shippers, onboarding requests
    AddressService/          .NET address and location data
    FileService/             .NET Firebase-backed file upload/service access
    NotificationService/     .NET notifications, email, assignment hub
    DeliveryService/         .NET delivery tracking, assignments, SignalR hubs
    CatalogService/          Node.js catalog API with Prisma
    OrderService/            Node.js order/cart/payment API with Prisma and Redis
    ReportService/           Node.js reporting API with Prisma
    WalletService/           Node.js wallet/top-up API with Prisma
    ChatService/             Node.js chat API with Prisma
  SharedLibs/
    Messaging/               RabbitMQ abstractions/contracts/shared publisher code
    Utils/                   Shared utilities such as OpenAPI transformers
  Infrastructure/            RabbitMQ and database env templates
  postman/                   Postman collections for manual API testing
```

## Service URLs

The API gateway is the main entry point:

| Component | Default URL |
| --- | --- |
| API Gateway | `http://localhost:8080` |
| AuthenticationService | `http://localhost:8081` |
| UserService | `http://localhost:8082` |
| NotificationService | `http://localhost:8083` |
| DeliveryService | `http://localhost:8084` |
| CatalogService | `http://localhost:8085` |
| OrderService | `http://localhost:8086` |
| FileService | `http://localhost:8087` |
| ReportService | `http://localhost:8088` |
| WalletService | `http://localhost:8089` |
| AddressService | `http://localhost:8091` recommended |
| ChatService | `http://localhost:8090` |
| RabbitMQ management | `http://localhost:15672` |

Gateway routes are configured in `Services/ApiGateway/yarp.json`. Common route prefixes include `/api/auth`, `/api/users`, `/api/merchants`, `/api/shippers`, `/api/catalog`, `/api/orders`, `/api/deliveries`, `/api/notifications`, `/api/wallets`, `/api/reports`, `/api/chats`, and `/api/files`.

## Prerequisites

- Docker Desktop with Docker Compose v2.
- .NET SDK 10 if running .NET services outside Docker.
- Node.js 20 and npm if running Node services outside Docker.
- Firebase service account JSON at `firebase-auth.json` for FileService, NotificationService, and DeliveryService features that use Firebase.
- Optional: Postman for collections under `postman/`.

## Environment Setup

From this `Backend` directory, create local env files from the committed examples:

```powershell
Copy-Item .env.example .env

Get-ChildItem Infrastructure -Filter *.env.example | ForEach-Object {
    Copy-Item $_.FullName ($_.FullName -replace '\.example$', '') -Force
}

Get-ChildItem Services -Directory | ForEach-Object {
    $example = Join-Path $_.FullName '.env.example'
    $target = Join-Path $_.FullName '.env'
    if (Test-Path $example) {
        Copy-Item $example $target -Force
    }
}

New-Item -ItemType File -Path Services\WalletService\.env -Force
```

Then edit the generated `.env` files as needed:

- Keep the same JWT issuer, audience, and key across services that validate tokens.
- Replace placeholders for Firebase, Supabase, SMTP, OpenRouteService, and VNPay before testing those flows.
- Do not commit `.env` files or `firebase-auth.json`; they are intentionally ignored.
- If you copy from `.env.example`, avoid a port collision between AddressService and ChatService. The recommended local setup is `ADDRESS_SERVICE_HOST_PORT=8091` and `CHAT_SERVICE_HOST_PORT=8090`.

## Run With Docker

Start the full backend stack:

```powershell
docker compose up --build -d
```

Check container status:

```powershell
docker compose ps
```

Follow logs for one service:

```powershell
docker compose logs -f apigateway
docker compose logs -f authentication-service
```

Stop the stack while keeping database volumes:

```powershell
docker compose down
```

Reset local databases and Redis volumes:

```powershell
docker compose down -v
```

The .NET services apply EF Core migrations on startup. Node services use Prisma; Docker builds generate Prisma clients, and services such as OrderService run database setup during container startup.

## Run A Service Locally

Docker is the easiest way to run the full system. For local debugging, keep required infrastructure running with Docker, then run a service directly.

.NET service example:

```powershell
dotnet build Backend.slnx
dotnet run --project Services\AuthenticationService\AuthenticationService.csproj
```

Node service example:

```powershell
Set-Location Services\OrderService
npm install
npx prisma generate
npm run dev
```

When running a service outside Docker, update its connection strings or environment variables to use host ports, for example `localhost:5433` for `authentication-db` instead of `authentication-db:5432`.

## API Documentation And Testing

- .NET services expose OpenAPI in Development, usually at `/openapi/v1.json`, with Scalar UI available through the mapped Scalar route.
- Node services include Swagger UI dependencies; check each service's routes if you need direct service documentation.
- Postman collections are available in `postman/`, including `full-backend-sequential.collection.json` and `catalog-order-minimal.collection.json`.
- `api-endpoint-tests.http` contains HTTP requests useful for quick manual checks.

## Data And Messaging

- PostgreSQL is used by Authentication, User, Address, Notification, Delivery, Order, Report, and Wallet services.
- Redis is used by Delivery and Order services.
- RabbitMQ is used for cross-service events such as user creation, OTP verification, merchant/shipper request reviews, notifications, and delivery/order workflows.
- Seed data exists in several services to support local development and demos.

## Troubleshooting

- If Docker reports a missing env file, re-run the environment setup commands above.
- If a port is already in use, edit the root `.env` host port and restart the stack.
- If Firebase-dependent services fail at startup, verify `firebase-auth.json` exists at the backend root and the service env file points to `/app/secrets/firebase-auth.json`.
- If a database schema looks stale after changing migrations, run `docker compose down -v` and start again.
- If a Node service cannot find Prisma client types, run `npx prisma generate` in that service folder.
