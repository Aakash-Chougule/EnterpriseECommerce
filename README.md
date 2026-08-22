# 🛒 Enterprise E-Commerce & Order Management System

A full-stack **enterprise-style e-commerce and order management platform** built using **ASP.NET Core (.NET 10), React.js, PostgreSQL, Entity Framework Core, JWT Authentication, Razorpay, Docker, Kafka, Azure, and AWS concepts**.

The project is designed as a real-world e-commerce application inspired by platforms such as Flipkart and other modern online marketplaces.

It demonstrates backend architecture, authentication and authorization, product and inventory management, shopping cart functionality, order processing, payments, GST calculations, reporting, and a React-based frontend.

> 🚧 **Project Status:** Active learning project — current development phase completed and temporarily paused. Additional enterprise features are planned for future versions.

---

# 📌 Project Overview

The goal of this project is to build a production-style e-commerce system rather than a basic CRUD application.

The application currently includes:

- User registration and login
- JWT authentication
- Role-based authorization
- Customer and admin functionality
- Category management
- Product management
- Inventory management
- Low-stock monitoring
- Shopping cart
- Checkout
- Order management
- Razorpay payment integration
- GST-aware product pricing
- HSN codes
- CGST / SGST / IGST calculations
- Shipping information
- Delivery charges
- Inventory reports
- Product/category-wise reporting
- Admin dashboard
- Customer profile
- Order history

The architecture separates the **Domain, Application, Infrastructure, API, Frontend, Notification Service, and Tests**.

---

# 🏗️ Architecture

The backend follows a layered / Clean Architecture inspired structure.

```text
EnterpriseECommerce
│
├── client
│   └── React Frontend
│
├── src
│   │
│   ├── EnterpriseECommerce.Domain
│   │   ├── Entities
│   │   └── Enums
│   │
│   ├── EnterpriseECommerce.Application
│   │   ├── DTOs
│   │   ├── Interfaces
│   │   └── Services
│   │
│   ├── EnterpriseECommerce.Infrastructure
│   │   ├── Persistence
│   │   ├── Configurations
│   │   └── Repositories
│   │
│   ├── EnterpriseECommerce.API
│   │   └── Controllers
│   │
│   └── EnterpriseECommerce.NotificationService
│
├── tests
│   ├── EnterpriseECommerce.UnitTests
│   └── EnterpriseECommerce.IntegrationTests
│
└── EnterpriseECommerce.slnx
```

---

# ⚙️ Technology Stack

## Backend

- C#
- ASP.NET Core
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- Dependency Injection
- Repository Pattern
- Service Layer
- Clean Architecture principles

## Frontend

- React.js
- JavaScript
- HTML5
- CSS3
- Axios
- React Router

## Database

- PostgreSQL
- Entity Framework Core Migrations

## Authentication & Security

- JWT Authentication
- Role-Based Authorization
- BCrypt Password Hashing
- Protected API endpoints

## Payments

- Razorpay integration
- Razorpay Order creation
- Payment verification
- Payment status management

## Messaging / Microservices

- Apache Kafka
- Notification Service architecture

## DevOps / Cloud

The project is being designed with support for:

- Docker
- CI/CD pipelines
- Microsoft Azure
- AWS
- AWS ECS

## Testing

- Unit Tests
- Integration Tests

## API Testing / Documentation

- Swagger / OpenAPI
- Postman

---

# 👥 User Roles

The system supports role-based access.

## Customer

Customers can:

- Register
- Login
- Browse products
- View product information
- Add products to cart
- Update cart quantities
- Remove cart items
- Checkout
- Enter delivery information
- Select payment method
- Place orders
- Make payments
- View order history
- View profile information

## Admin

Administrators can:

- Manage categories
- Manage products
- Create products
- Update products
- Activate/deactivate products
- Manage inventory
- Increase/decrease stock
- Monitor low-stock products
- View customer orders
- Manage order status
- Access reports
- Access administrative dashboards

---

# 🔐 Authentication & Authorization

Authentication is implemented using **JWT Bearer Authentication**.

After successful login, the API generates a JWT token containing user information and role claims.

Protected endpoints require:

```http
Authorization: Bearer <JWT_TOKEN>
```

Role-based authorization is used to protect administrative functionality.

Example:

```csharp
[Authorize(Roles = "Admin")]
```

Passwords are securely hashed instead of being stored as plain text.

---

# 📦 Product Management

Products contain information such as:

```text
Product ID
Category ID
Product Name
Description
SKU
HSN Code
GST Rate
Selling Price
Stock Quantity
Active Status
Created Date
Updated Date
```

The SKU uniquely identifies a product from a business perspective.

---

# 🏷️ Category Management

Products are organized into categories.

Administrators can:

- Create categories
- Update categories
- Activate categories
- Deactivate categories
- Associate products with categories

This allows products and reports to be organized category-wise.

---

# 🇮🇳 GST Support

The project contains GST-aware pricing functionality suitable for an Indian e-commerce workflow.

Each product can contain:

```text
HSN Code
GST Rate
GST-inclusive Selling Price
```

Supported GST rates can include:

```text
0%
5%
12%
18%
28%
```

The application treats the product selling price as **GST inclusive**.

Example:

```text
Selling Price = ₹1,180
GST Rate      = 18%

Taxable Value = ₹1,000
GST Amount    = ₹180

Final Product Price = ₹1,180
```

GST is therefore not incorrectly added again to an already GST-inclusive product price.

---

# 🧾 HSN Code

Products support **HSN (Harmonized System of Nomenclature) codes**.

HSN information can be stored with each product and preserved with order information for future invoice and tax reporting functionality.

Example:

```text
Product: Mechanical Keyboard
HSN Code: 8471
GST Rate: 18%
```

The actual HSN/GST classification should be configured according to the applicable tax rules for the product.

---

# 💰 GST Calculation

When an order is created, tax information is preserved as part of the order-item snapshot.

For GST-inclusive pricing:

```text
Taxable Amount =
Gross Amount × 100 / (100 + GST Rate)

GST Amount =
Gross Amount - Taxable Amount
```

Example:

```text
Gross Price = ₹1,180
GST = 18%

Taxable Amount = ₹1,000
GST Amount = ₹180
```

---

# 🏠 Intra-State GST

For orders where the applicable supply and destination states are the same, GST can be divided into:

```text
CGST
+
SGST
```

Example:

```text
Total GST = ₹180

CGST = ₹90
SGST = ₹90
```

---

# 🚚 Inter-State GST

For applicable inter-state transactions, GST can be represented as:

```text
IGST
```

Example:

```text
Taxable Amount = ₹1,000
IGST = ₹180
Final Product Price = ₹1,180
```

---

# 🚚 Shipping & Delivery

Checkout collects structured delivery information.

The order can contain:

```text
Shipping Address
City
State
State Code
PIN / Postal Code
```

Shipping information is also used by the pricing/tax workflow.

The architecture supports delivery charge calculation so that the customer can see the final payable amount before payment.

---

# 🛒 Shopping Cart

Each authenticated customer has a shopping cart.

Supported functionality includes:

- Add product
- Remove product
- Increase quantity
- Decrease quantity
- Clear cart
- Calculate cart total
- Validate product availability

The cart uses the current product price.

The final purchase information is copied into the order during checkout so historical orders are not affected if the product is modified later.

---

# 📋 Order Management

Orders contain customer, product, pricing, tax, payment, and shipping information.

The order lifecycle includes states such as:

```text
Pending
   ↓
Confirmed
   ↓
Processing
   ↓
Shipped
   ↓
Delivered
```

Orders may also be:

```text
Cancelled
```

Business rules prevent invalid transitions.

For example, a shipped or delivered order should not simply be cancelled using the normal cancellation flow.

---

# 📸 Order Item Snapshot

An important design decision in this project is storing product information inside `OrderItem` when the order is created.

The snapshot can preserve:

```text
Product ID
Product Name
SKU
HSN Code
Quantity
Unit Price
GST Rate
Taxable Amount
GST Amount
CGST
SGST
IGST
Final Line Total
```

This means historical order data remains accurate even if the administrator later changes:

- Product name
- Product price
- GST rate
- HSN code

---

# 💳 Payment System

The project contains payment functionality with Razorpay integration.

Payment information can include:

```text
Payment ID
Order ID
Amount
Payment Method
Transaction ID
Payment Status
Razorpay Order ID
Razorpay Payment ID
Razorpay Signature
Failure Reason
Created Date
Updated Date
```

Supported payment lifecycle includes:

```text
Pending
Success
Failed
Refunded
```

---

# 💳 Razorpay Integration

The payment workflow is designed around:

```text
Checkout
   ↓
Create Application Order
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
Update Payment Status
   ↓
Update Order Status
```

Sensitive Razorpay credentials should remain on the backend.

Never expose the Razorpay secret key in frontend code.

---

# 📊 Inventory Management

The application contains a dedicated inventory management section.

Administrators can monitor:

- Active products
- In-stock products
- Low-stock products
- Out-of-stock products
- Total inventory units

Administrators can also adjust stock quantities.

---

# ⚠️ Low Stock Monitoring

A configurable low-stock threshold can be used to identify products requiring restocking.

Example:

```text
Threshold = 5

Stock = 20 → In Stock
Stock = 5  → Low Stock
Stock = 0  → Out of Stock
```

---

# 📈 Inventory Reports

The project includes inventory reporting functionality.

Reports can contain product-level information such as:

```text
Product
Category
SKU
Current Stock
Stock Status
Price
Inventory Value
```

Reports can also be organized category-wise for better inventory analysis.

---

# 📊 Admin Dashboard

Administrative functionality provides a centralized place for managing the e-commerce platform.

Current/target dashboard information includes:

```text
Products
Categories
Inventory
Orders
Customers
Payments
Low Stock
Reports
```

---

# 🌐 API Endpoints

Some of the API areas implemented in the project include:

```text
/api/Auth
/api/Products
/api/Categories
/api/Cart
/api/Orders
/api/Payments
```

Administrative endpoints are protected using role-based authorization.

Swagger can be used to inspect and test available API endpoints.

---

# 🗄️ Database

The application uses **PostgreSQL** with **Entity Framework Core**.

Major entities include:

```text
Users
Roles
Categories
Products
Carts
CartItems
Orders
OrderItems
Payments
```

Entity Framework Core configurations define relationships, indexes, constraints and decimal precision.

---

# 🔄 Entity Framework Core Migrations

After configuring the PostgreSQL connection string, migrations can be applied using:

```bash
dotnet ef database update \
  --project src/EnterpriseECommerce.Infrastructure \
  --startup-project src/EnterpriseECommerce.API
```

To create a new migration:

```bash
dotnet ef migrations add MigrationName \
  --project src/EnterpriseECommerce.Infrastructure \
  --startup-project src/EnterpriseECommerce.API
```

---

# 🚀 Running the Backend

Clone the repository:

```bash
git clone <YOUR_REPOSITORY_URL>

cd EnterpriseECommerce
```

Restore packages:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build EnterpriseECommerce.slnx
```

Configure your PostgreSQL connection string and required secrets using development configuration or environment variables.

Then apply migrations:

```bash
dotnet ef database update \
  --project src/EnterpriseECommerce.Infrastructure \
  --startup-project src/EnterpriseECommerce.API
```

Run the API:

```bash
dotnet run \
  --project src/EnterpriseECommerce.API
```

Open Swagger using the local URL displayed by ASP.NET Core.

---

# ⚛️ Running the React Frontend

Open another terminal:

```bash
cd client
```

Install dependencies:

```bash
npm install
```

Run the development server:

```bash
npm run dev
```

The Vite development server will display the frontend URL, typically similar to:

```text
http://localhost:5173
```

---

# 🔑 Environment Configuration

Do **NOT** commit production secrets to GitHub.

Sensitive values may include:

```text
Database connection strings
JWT signing keys
Razorpay Key Secret
Kafka credentials
Azure credentials
AWS credentials
Email credentials
```

Use environment variables, secret managers, or development-specific configuration.

Example conceptual configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_DATABASE_CONNECTION"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY"
  },
  "Razorpay": {
    "KeyId": "YOUR_RAZORPAY_KEY_ID",
    "KeySecret": "YOUR_RAZORPAY_SECRET"
  }
}
```

Never commit real credentials.

---

# 🔒 Security Considerations

The project uses or is designed around:

- Password hashing
- JWT authentication
- Role authorization
- Server-side price calculation
- Server-side GST calculation
- Payment signature verification
- Protected administrative APIs
- DTOs instead of exposing entities directly
- Database constraints
- Input validation

An important principle is:

> **Never trust financial values sent by the frontend.**

Product price, GST, shipping charges, discounts and final payment totals should be validated or calculated by the backend.

---

# 🧪 Testing

The solution contains:

```text
EnterpriseECommerce.UnitTests
EnterpriseECommerce.IntegrationTests
```

Run tests using:

```bash
dotnet test
```

Build the complete solution using:

```bash
dotnet build EnterpriseECommerce.slnx
```

---

# 🐳 Docker

Docker support is part of the project's architecture/roadmap.

The goal is to containerize components such as:

```text
ASP.NET Core API
React frontend
PostgreSQL
Kafka
Notification Service
```

This will make local development and cloud deployment easier.

---

# 📨 Kafka & Notification Service

The solution contains a separate:

```text
EnterpriseECommerce.NotificationService
```

The architecture is intended to support event-driven communication through Apache Kafka.

Future events can include:

```text
OrderCreated
PaymentSuccessful
OrderConfirmed
OrderShipped
OrderDelivered
```

Consumers can process these events asynchronously for:

- Email notifications
- Order notifications
- Payment confirmations
- Shipping updates

---

# ☁️ Cloud & DevOps Roadmap

The project is intended to demonstrate cloud and DevOps concepts using technologies such as:

```text
Docker
CI/CD
Azure
AWS
AWS ECS
Kafka
Cloud-hosted PostgreSQL
```

These parts will continue to be expanded in future development phases.

---

# 🛣️ Future Roadmap

Planned improvements include:

- Product image upload
- Multiple product images
- Brand management
- Product variants
- Size/color variants
- Search and advanced filtering
- Sorting
- Pagination
- Wishlist
- Product reviews and ratings
- Coupons
- Advanced discount engine
- Delivery serviceability by PIN code
- Dynamic shipping charges
- Shipping provider integration
- Order tracking
- Invoice generation
- GST invoice PDF
- Refund workflow
- Return/replacement workflow
- Razorpay webhook handling
- Payment reconciliation
- Seller/vendor module
- Multiple sellers
- Seller dashboard
- Advanced sales reports
- Revenue analytics
- GST reports
- Inventory history
- Stock movement audit
- Kafka event processing
- Email notifications
- Docker Compose
- CI/CD pipeline
- Azure deployment
- AWS deployment
- AWS ECS deployment
- Logging and monitoring
- Redis caching
- Improved automated tests
- Production security hardening

---

# 🎯 Project Purpose

This project was created to strengthen practical knowledge of enterprise .NET development and demonstrate how multiple technologies work together in a realistic application.

Major concepts demonstrated include:

```text
ASP.NET Core
REST APIs
Clean Architecture
C#
Entity Framework Core
PostgreSQL
React.js
JWT
Role-Based Authorization
Razorpay
GST Calculations
Inventory Management
Order Management
Repository Pattern
Dependency Injection
Kafka
Docker
Cloud Architecture
Unit Testing
Integration Testing
```

---

# 📚 Learning Outcomes

Building this project provides practical experience with:

- Designing domain entities
- Creating REST APIs
- Implementing business rules
- Working with PostgreSQL
- Using Entity Framework Core
- Managing migrations
- Authentication and authorization
- Secure password storage
- JWT token generation
- React/API integration
- Shopping cart design
- Order lifecycle management
- Inventory management
- Payment gateway integration
- GST-aware e-commerce design
- Clean Architecture
- Repository Pattern
- Dependency Injection
- Testing
- Microservice concepts
- Event-driven architecture
- Cloud and DevOps concepts

---

# ⚠️ Current Development Status

The current development phase has been **temporarily paused**.

The repository represents the working state of the application at the end of the current development phase.

Development will continue in a future phase with additional features, production hardening, cloud deployment, event-driven functionality and further UI/UX improvements.

---

# 👨‍💻 Developer

**Aakash Chougule**

.NET / ASP.NET Developer

Technologies explored through this project:

`C#` · `ASP.NET Core` · `.NET 10` · `React.js` · `PostgreSQL` · `Entity Framework Core` · `JWT` · `Razorpay` · `Kafka` · `Docker` · `Azure` · `AWS`

---

# ⭐ Support

If you find this project useful for learning enterprise ASP.NET Core architecture, feel free to star the repository.

---

## 📄 License

This project is currently intended for **learning, portfolio, and demonstration purposes**.