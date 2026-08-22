# 🛒 Enterprise E-Commerce & Order Management System

A full-stack enterprise-style e-commerce application built with:

- ASP.NET Core (.NET 10)
- React.js
- PostgreSQL
- Entity Framework Core
- JWT Authentication
- Razorpay
- Apache Kafka
- Docker
- Azure / AWS concepts
- Unit & Integration Testing

The project is inspired by modern marketplace applications such as Flipkart and is intended as a practical enterprise .NET learning and portfolio project.

> 🚧 Current Status: Development is temporarily paused. The repository preserves the working state so development can continue later.

---

# 📌 Main Features

Current functionality includes:

- User registration and login
- JWT authentication
- Role-based authorization
- Admin permissions
- Customer profile
- Product management
- Category management
- Inventory management
- Shopping cart
- Checkout
- GST-aware pricing
- HSN codes
- CGST / SGST / IGST
- Shipping charges
- Order management
- Razorpay payments
- Payment verification
- Order history
- Admin reports
- Inventory reports
- Kafka notification architecture
- Unit tests
- Integration tests

---

# 🏗️ Project Architecture

```text
EnterpriseECommerce
│
├── client
│   └── React Frontend
│
├── src
│   ├── EnterpriseECommerce.Domain
│   ├── EnterpriseECommerce.Application
│   ├── EnterpriseECommerce.Infrastructure
│   ├── EnterpriseECommerce.API
│   └── EnterpriseECommerce.NotificationService
│
├── tests
│   ├── EnterpriseECommerce.UnitTests
│   └── EnterpriseECommerce.IntegrationTests
│
└── EnterpriseECommerce.slnx
```

---

# 🧰 Technology Stack

## Backend

```text
C#
ASP.NET Core
.NET 10
ASP.NET Core Web API
Entity Framework Core
LINQ
Dependency Injection
Repository Pattern
Service Layer
Clean Architecture concepts
```

## Frontend

```text
React.js
JavaScript
HTML
CSS
Axios
React Router
Vite
```

## Database

```text
PostgreSQL
Entity Framework Core Migrations
```

## Authentication

```text
JWT Bearer Authentication
Role-Based Authorization
BCrypt Password Hashing
Permission-Based Admin Access
```

## Payments

```text
Razorpay
Payment verification
Payment status tracking
```

## Messaging

```text
Apache Kafka
Notification Service
```

## DevOps / Cloud

```text
Docker
Docker Compose
CI/CD roadmap
Azure
AWS
AWS ECS roadmap
```

---

# 🇮🇳 GST & Pricing

Product prices are treated as GST-inclusive.

Example:

```text
Selling Price = ₹1,180
GST Rate      = 18%

Taxable Value = ₹1,000
GST Amount    = ₹180

Customer Price = ₹1,180
```

GST is extracted from the product price and is not added twice.

For intra-state transactions:

```text
CGST + SGST
```

For inter-state transactions:

```text
IGST
```

Products also support:

```text
HSN Code
GST Rate
GST-inclusive Selling Price
```

---

# 🚚 Shipping

Checkout supports structured delivery information:

```text
Address
City
State
GST State Code
PIN Code
```

Shipping charges can be configured through backend configuration.

Example:

```json
"Commerce": {
  "SellerState": "Maharashtra",
  "SellerStateCode": "27",
  "DefaultShippingCharge": 40,
  "FreeShippingThreshold": 500
}
```

Example logic:

```text
Order >= ₹500
→ FREE SHIPPING

Order < ₹500
→ ₹40 Shipping
```

---

# 💳 Payment Flow

```text
Cart
  ↓
Checkout
  ↓
Server calculates final price
  ↓
Create Order
  ↓
Create Payment
  ↓
Create Razorpay Order
  ↓
Open Razorpay Checkout
  ↓
Customer Payment
  ↓
Verify Razorpay Signature
  ↓
Payment Success
  ↓
Order Updated
```

Never expose Razorpay secret keys in frontend code.

---

# 🚀 HOW TO RUN THIS PROJECT ON A NEW LAPTOP

This section is intentionally detailed so the project can be restarted later even after a long break.

---

# 1. Install Required Software

Install the following before cloning the project.

## Git

Download and install Git.

Check:

```powershell
git --version
```

---

## .NET 10 SDK

Install the .NET 10 SDK.

Check:

```powershell
dotnet --version
```

Expected version should start with:

```text
10.
```

---

## Node.js

Install the latest supported LTS version of Node.js.

Check:

```powershell
node --version
npm --version
```

---

## PostgreSQL

Install PostgreSQL.

You may also install:

```text
pgAdmin
```

for managing the database visually.

Check PostgreSQL service is running before starting the API.

---

## Docker Desktop

Recommended for:

```text
Kafka
PostgreSQL
future full-stack container setup
```

Check:

```powershell
docker --version
docker compose version
```

Make sure Docker Desktop is running.

---

# 2. Clone the Repository

Open PowerShell or Command Prompt.

```powershell
git clone YOUR_GITHUB_REPOSITORY_URL
```

Example:

```powershell
git clone https://github.com/YOUR_USERNAME/EnterpriseECommerce.git
```

Enter the project:

```powershell
cd EnterpriseECommerce
```

---

# 3. Restore .NET Packages

Run:

```powershell
dotnet restore
```

Then:

```powershell
dotnet build EnterpriseECommerce.slnx
```

Expected result:

```text
Build succeeded.
```

---

# 4. Install EF Core CLI Tool

If Entity Framework CLI is not installed:

```powershell
dotnet tool install --global dotnet-ef
```

Check:

```powershell
dotnet ef --version
```

If already installed but outdated:

```powershell
dotnet tool update --global dotnet-ef
```

---

# 5. Configure PostgreSQL

Create a PostgreSQL database.

Example:

```text
Database Name:
EnterpriseECommerce
```

Example connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EnterpriseECommerce;Username=postgres;Password=YOUR_PASSWORD"
}
```

Do not commit real passwords to GitHub.

---

# 6. Configure Application Secrets

The GitHub repository should NOT contain production secrets.

You will need to configure values such as:

```text
PostgreSQL password
JWT Secret
Razorpay Key ID
Razorpay Key Secret
Kafka settings
Email credentials
Azure credentials
AWS credentials
```

Recommended methods:

```text
appsettings.Development.json
Environment Variables
dotnet user-secrets
Azure Key Vault
AWS Secrets Manager
```

---

# 7. Example Backend Configuration

A development configuration may look similar to:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EnterpriseECommerce;Username=postgres;Password=YOUR_PASSWORD"
  },

  "Jwt": {
    "SecretKey": "YOUR_LONG_JWT_SECRET",
    "Issuer": "EnterpriseECommerce",
    "Audience": "EnterpriseECommerceClient",
    "ExpirationMinutes": 60
  },

  "Razorpay": {
    "KeyId": "YOUR_RAZORPAY_KEY_ID",
    "KeySecret": "YOUR_RAZORPAY_KEY_SECRET"
  },

  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "OrderEventsTopic": "order-events",
    "PaymentEventsTopic": "payment-events",
    "OrderStatusEventsTopic": "order-status-events"
  },

  "Commerce": {
    "SellerState": "Maharashtra",
    "SellerStateCode": "27",
    "DefaultShippingCharge": 40,
    "FreeShippingThreshold": 500
  }
}
```

Important:

```text
DO NOT push real secrets to GitHub.
```

---

# 8. Apply Database Migrations

After PostgreSQL is running and the connection string is configured:

```powershell
dotnet ef database update `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

This creates the database tables using existing migrations.

---

# 9. If You Need to Create a New Migration

Only run this when domain/database models have changed:

```powershell
dotnet ef migrations add MigrationName `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

Then:

```powershell
dotnet ef database update `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

---

# 10. Run the ASP.NET API

From the project root:

```powershell
dotnet run --project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

ASP.NET will show something similar to:

```text
Now listening on:
http://localhost:xxxx
https://localhost:xxxx
```

Swagger can usually be opened at:

```text
https://localhost:xxxx/swagger
```

Use the actual port shown in your terminal.

---

# 11. Run the React Frontend

Open a SECOND terminal.

Navigate to the frontend:

```powershell
cd EnterpriseECommerce
cd client
```

Install packages:

```powershell
npm install
```

Run:

```powershell
npm run dev
```

Vite will usually show:

```text
http://localhost:5173
```

Open that URL in your browser.

---

# 12. Verify Frontend API URL

Check your frontend API configuration.

Example location:

```text
client/src/api/apiClient.js
```

Make sure the base URL points to the ASP.NET API.

Example:

```javascript
const apiClient = axios.create({
    baseURL: 'https://localhost:xxxx/api'
})
```

Use the actual backend port shown when running the API.

---

# 13. Start Kafka

The Notification Service requires Kafka.

If Kafka is running through Docker, start the required containers.

Example:

```powershell
docker compose up -d
```

Then verify:

```powershell
docker ps
```

You should see Kafka-related containers running.

---

# 14. Run Notification Service

Open another terminal:

```powershell
dotnet run --project src\EnterpriseECommerce.NotificationService\EnterpriseECommerce.NotificationService.csproj
```

Expected log:

```text
NotificationService subscribed to Kafka topics:
order-events
payment-events
order-status-events
```

If you see:

```text
1/1 brokers are down
```

Kafka is not running or the bootstrap server configuration is incorrect.

---

# 15. Run Tests

Run all tests:

```powershell
dotnet test
```

Or build everything:

```powershell
dotnet build EnterpriseECommerce.slnx
```

---

# ✅ NORMAL DEVELOPMENT STARTUP ORDER

When returning to the project later, use this sequence.

```text
1. Start Docker Desktop
2. Start PostgreSQL
3. Start Kafka
4. Start ASP.NET API
5. Start NotificationService
6. Start React frontend
```

Commands:

### Terminal 1

```powershell
dotnet run --project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

### Terminal 2

```powershell
cd client
npm run dev
```

### Terminal 3

```powershell
dotnet run --project src\EnterpriseECommerce.NotificationService\EnterpriseECommerce.NotificationService.csproj
```

---

# 🐳 DOCKER

Docker is planned to make setup easier.

Instead of manually installing and starting:

```text
PostgreSQL
Kafka
Kafka dependencies
```

Docker can manage these services.

---

# Docker Development Goal

The future architecture should look like:

```text
Docker Compose
│
├── PostgreSQL
├── Kafka
├── API
├── NotificationService
└── React Frontend
```

Eventually the entire project should start using:

```powershell
docker compose up -d
```

---

# Example Future docker-compose.yml

This is an example structure and may need changes based on the final project configuration.

```yaml
services:

  postgres:
    image: postgres:latest
    container_name: ecommerce-postgres
    environment:
      POSTGRES_DB: EnterpriseECommerce
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - ecommerce_postgres_data:/var/lib/postgresql/data

  kafka:
    image: apache/kafka:latest
    container_name: ecommerce-kafka
    ports:
      - "9092:9092"

volumes:
  ecommerce_postgres_data:
```

Later this can also contain:

```text
API
React
Notification Service
```

---

# Docker Commands

Start containers:

```powershell
docker compose up -d
```

See containers:

```powershell
docker ps
```

See all containers:

```powershell
docker ps -a
```

Stop containers:

```powershell
docker compose down
```

Stop and remove volumes:

```powershell
docker compose down -v
```

⚠️ Warning:

```text
docker compose down -v
```

can delete persistent database data stored in Docker volumes.

Do not run it unless you intentionally want to delete that data.

---

# Docker Logs

View all Docker Compose logs:

```powershell
docker compose logs
```

Follow logs:

```powershell
docker compose logs -f
```

Kafka logs:

```powershell
docker logs ecommerce-kafka
```

PostgreSQL logs:

```powershell
docker logs ecommerce-postgres
```

---

# 🗄️ IMPORTANT: DATABASE DATA VS MIGRATIONS

GitHub stores:

```text
Source code
EF migrations
Configuration examples
Docker files
```

GitHub does NOT automatically store your local PostgreSQL database data.

For example, this data may not exist after cloning on another laptop:

```text
Users
Products
Categories
Orders
Payments
Inventory history
```

Running:

```powershell
dotnet ef database update
```

recreates the database structure, but not necessarily your old runtime data.

---

# Backup Existing PostgreSQL Database

If you want to preserve the current database data, create a backup before moving computers.

Example using pg_dump:

```powershell
pg_dump -U postgres -d EnterpriseECommerce -F c -f EnterpriseECommerce.backup
```

Restore later:

```powershell
pg_restore -U postgres -d EnterpriseECommerce EnterpriseECommerce.backup
```

Exact commands may vary depending on PostgreSQL installation and authentication.

---

# Recommended Future Seed Data

The project should eventually contain automatic seed data for:

```text
Admin role
User role
Default permissions
Main Admin
Example Categories
Example Products
```

Then a completely fresh database can be prepared automatically.

---

# 🔐 NEVER PUSH THESE TO GITHUB

Do not commit:

```text
Real PostgreSQL passwords
JWT Secret
Razorpay Secret
AWS Access Key
AWS Secret Key
Azure secrets
Email passwords
Private certificates
Production connection strings
```

---

# Recommended .gitignore Entries

Your `.gitignore` should include appropriate entries such as:

```gitignore
bin/
obj/

node_modules/

.env
.env.*
!.env.example

appsettings.Development.json
appsettings.Local.json

*.user
*.suo

.vs/

.vscode/

coverage/

dist/
```

Be careful before ignoring `appsettings.Development.json` if you intentionally want to commit a safe placeholder version.

---

# `.env.example`

A future `.env.example` file can contain:

```env
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DATABASE=EnterpriseECommerce
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=CHANGE_ME

JWT_SECRET=CHANGE_ME

RAZORPAY_KEY_ID=CHANGE_ME
RAZORPAY_KEY_SECRET=CHANGE_ME

KAFKA_BOOTSTRAP_SERVERS=localhost:9092

SELLER_STATE=Maharashtra
SELLER_STATE_CODE=27
```

Commit:

```text
.env.example
```

Do NOT commit:

```text
.env
```

---

# 🧯 TROUBLESHOOTING

## Problem: dotnet command not found

Install the .NET SDK.

Then verify:

```powershell
dotnet --version
```

---

## Problem: npm command not found

Install Node.js.

Verify:

```powershell
node --version
npm --version
```

---

## Problem: PostgreSQL connection failed

Check:

```text
PostgreSQL service is running
Database exists
Username is correct
Password is correct
Port is correct
Connection string is correct
```

Default PostgreSQL port:

```text
5432
```

---

## Problem: PendingModelChangesWarning

If EF Core reports:

```text
The model for context 'AppDbContext' has pending changes
```

create a migration:

```powershell
dotnet ef migrations add MigrationName `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

Then:

```powershell
dotnet ef database update `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

---

## Problem: Kafka says brokers are down

Example:

```text
1/1 brokers are down
localhost:9092
```

Check:

```powershell
docker ps
```

Make sure Kafka is running.

Then verify configuration:

```text
localhost:9092
```

---

## Problem: React cannot connect to API

Check:

```text
API is running
API URL is correct
API port matches apiClient.js
CORS is configured
HTTPS certificate is trusted
```

---

## Problem: HTTPS development certificate

Run:

```powershell
dotnet dev-certs https --trust
```

Then restart the API.

---

## Problem: React dependencies missing

Run:

```powershell
cd client
npm install
```

Then:

```powershell
npm run dev
```

---

## Problem: Build errors after pulling changes

Run:

```powershell
dotnet clean
dotnet restore
dotnet build EnterpriseECommerce.slnx
```

For React:

```powershell
cd client
npm install
npm run dev
```

---

# 🔄 HOW TO CONTINUE DEVELOPMENT LATER

When returning to this project in the future:

```powershell
git pull origin main
```

Then:

```powershell
dotnet restore
dotnet build EnterpriseECommerce.slnx
```

Frontend:

```powershell
cd client
npm install
```

Check migrations:

```powershell
dotnet ef database update `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

Then start the application normally.

---

# 📤 HOW TO PUSH NEW CHANGES

Check changes:

```powershell
git status
```

Stage:

```powershell
git add .
```

Commit:

```powershell
git commit -m "feat: describe the changes"
```

Push:

```powershell
git push origin main
```

---

# 📥 HOW TO PULL LATEST CHANGES

```powershell
git pull origin main
```

---

# 🌿 CREATE A FEATURE BRANCH

Recommended for future development:

```powershell
git checkout -b feature/product-images
```

Work on the feature.

Then:

```powershell
git add .
git commit -m "feat: add product image support"
git push -u origin feature/product-images
```

---

# 📊 ADMIN FUNCTIONALITY

The admin section supports / is designed for:

```text
Product Management
Category Management
Inventory
Orders
Users & Admins
Permissions
Reports
Sales Reports
Inventory Reports
Payment Reports
```

---

# 📦 INVENTORY

Inventory functionality includes:

```text
Current Stock
Low Stock
Out of Stock
Increase Stock
Decrease Stock
Inventory Value
Product-wise inventory
Category-wise inventory
```

---

# 📈 REPORTS

Current / planned reports include:

```text
Sales Report
Order Report
Payment Report
Product Report
Top Products
Inventory Report
Category-wise Inventory
Product-wise Inventory
GST Report
Revenue Report
```

Export formats include or are planned for:

```text
CSV
Excel
PDF
```

---

# 📨 KAFKA NOTIFICATION SERVICE

Kafka topics currently used / planned include:

```text
order-events
payment-events
order-status-events
```

The Notification Service consumes events separately from the main API.

This demonstrates an event-driven / microservice-style architecture.

---

# 🛣️ FUTURE ROADMAP

Planned features:

```text
Product Images
Multiple Product Images
Brands
Product Variants
Color / Size
Wishlist
Ratings
Reviews
Advanced Search
Pagination
Coupons
Discount Engine
GST Invoice PDF
Invoice Download
Order Tracking
Shipping Provider Integration
Returns
Replacement
Refund Workflow
Razorpay Webhooks
Payment Reconciliation
Seller Module
Multi-vendor Marketplace
Redis
Background Jobs
Kafka improvements
Email Notifications
Docker Compose
CI/CD
Azure Deployment
AWS Deployment
AWS ECS
Logging
Monitoring
Production Security
Automated Testing
```

---

# 🎯 Project Purpose

The main purpose of this project is to demonstrate practical enterprise application development using:

```text
ASP.NET Core
C#
.NET 10
REST APIs
React
PostgreSQL
Entity Framework Core
JWT
Clean Architecture
Repository Pattern
Dependency Injection
Role-Based Authorization
Razorpay
Kafka
Docker
GST
Inventory
Orders
Reports
Testing
Cloud concepts
```

---

# 👨‍💻 Developer

**Aakash Chougule**

.NET / ASP.NET Developer

Project technologies:

`C#` · `ASP.NET Core` · `.NET 10` · `React.js` · `PostgreSQL` · `Entity Framework Core` · `JWT` · `Razorpay` · `Kafka` · `Docker` · `Azure` · `AWS`

---

# ⭐ Quick Reminder for Future Me

If I open this project after several months and forget everything:

```text
1. Clone repo
2. Install .NET 10
3. Install Node.js
4. Install PostgreSQL
5. Install Docker Desktop
6. Configure appsettings / secrets
7. Start PostgreSQL
8. Start Kafka
9. Run EF database update
10. Run API
11. Run NotificationService
12. npm install
13. npm run dev
14. Open http://localhost:5173
```

Most useful commands:

```powershell
dotnet build EnterpriseECommerce.slnx
```

```powershell
dotnet ef database update `
  --project src\EnterpriseECommerce.Infrastructure\EnterpriseECommerce.Infrastructure.csproj `
  --startup-project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

```powershell
dotnet run --project src\EnterpriseECommerce.API\EnterpriseECommerce.API.csproj
```

```powershell
dotnet run --project src\EnterpriseECommerce.NotificationService\EnterpriseECommerce.NotificationService.csproj
```
```powershell
docker compose up -d postgres zookeeper kafka
```

```powershell
cd client
npm install
npm run dev
```

```powershell
docker compose up -d
```

---

# 📄 License

This project is currently intended for learning, portfolio, interview preparation, and demonstration purposes.
