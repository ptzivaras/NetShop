# 🛒 Eshop Project
This project follows a **3-Layer Architecture** pattern with clear separation of concerns.

A full-stack **E-commerce Web Application** built with modern technologies:  
- **Backend**: ASP.NET Core Web API (C#), Entity Framework Core, SQL Server  
- **Frontend**: ASP.NET Core MVC (Razor views, Bootstrap 5, JavaScript)  
- **Architecture**: 3-layer (Core, API, Web), DTOs, Services, Dependency Injection  
- **Authentication/Authorization**: ASP.NET Identity with Roles (Admin / Customer) 
- **Database**: SQL Server with Code-First Migrations, Seeders for initial data  
---

## 🚀 Features

### 👤 Authentication & Authorization
- ASP.NET Identity integration with role-based access
- Two roles: **Admin** and **Customer**
- Secure login, registration, password management
- Auto-seeded admin account: `admin@eshop.com` / `Admin123!`

### 📦 Product & Category Management (Admin Only)
- Full CRUD operations for Products and Categories
- Product image upload and storage (BLOB in database)
- Pagination and search/filtering
- Category-based product browsing

### 🛒 Shopping Cart
- Add/Remove/Update product quantities
- Persistent cart per authenticated user
- AJAX add-to-cart with instant UI feedback
- Seamless checkout integration

### 📑 Orders
- Place orders from shopping cart
- Order confirmation and history tracking
- Admin dashboard to view and manage all orders

### 📑 StockAlert
- Low stock inventory notifications for admins
- Displays alert count badge in navbar
- Admin can view, acknowledge, and dismiss alerts
- TODO: Implement automatic triggers (DB triggers or background job)


### 👤 User Profile
- View user account information (email, account details)
- Personal order history with pagination
- Order details and status tracking

### 🌍 Localization
- Multi-language support: English (default) & Greek
- Cookie-based language preference

### 🎨 UI/UX
- Fully responsive Bootstrap 5 layout
- Product cards with images, titles, descriptions, pricing
- Toast notifications for user actions
- Role-based UI elements (Admin sees Edit/Delete, Customers see Add to Cart)
- Pagination with page size control
- Search and filter functionality
- Image upload with preview (JPEG format for optimal storage)

### 🔒 Security & Performance 

**Authentication & Authorization:**
- **ASP.NET Identity + Cookies**
  - User management: Registration, login, password hashing, roles (Admin/Customer)
  - Authentication: Cookie-based (session stored on server, browser sends cookie automatically)
  - **Why cookies over JWT?**
  - Simpler for  MVC apps(no token storage/refresh logic required)
  - Browser handles cookies automatically
  - Built-in CSRF protection with ASP.NET Core
  -❌ Bad for mobile apps or cross-domain APIs


**API Security:**
- **HTTPS/TLS Encryption** ✅ - All communication encrypted with HTTPS to protect sensitive data
  - Cookie-based authentication requires HTTPS (Secure flag prevents cookie theft)
  - Prevents Man-in-the-Middle attacks on login credentials and session cookies
  - Development: Uses self-signed certificates (localhost:7068 API, localhost:7252 Web)
- **Rate Limiting** ✅ - Global rate limiter: 100 requests/minute per user/IP to prevent DDoS attacks
  - Built-in ASP.NET Core 8 `AddRateLimiter()` middleware
  - Custom rejection messages for throttled requests
- **IDOR Vulnerability Fixed(Temporarily)** ✅ - Authorization checks on user-scoped endpoints (OrdersController, ShoppingCartController)
  - Validates `currentUserId == userId` or `User.IsInRole("Admin")` before data access
  - Prevents unauthorized access to other users' orders/carts
- **Admin-Only Endpoints** ✅ - Write operations protected with `[Authorize(Roles = "Admin")]`
  - Products: Create, Update, Delete, Image Upload
  - Categories: Create, Update, Delete
  - Stock Alerts: All admin dashboard operations
- **Customer-Only Endpoints** ✅ - Write operations protected with `[Authorize(Roles = "Customer")]`
- **CORS Configuration** ✅ - API accepts requests only to trusted web origin
  - Go to appsettings.json and change domain name.
  - Now we are not in production this also needs improvement from local to production.
  - EG: https://randomname.com will be project future name :)
  - Configured with `AllowCredentials` for cookie support
- **No Hardcoded URLs** ✅ - All service API URLs configured via `appsettings.json:ApiSettings:BaseUrl`
  - Environment-specific configurations (Development, Staging, Production)
  - Easy deployment without code changes
- **Global Exception Handler** ✅ - Centralized error handling middleware prevents sensitive data exposure
  - Returns sanitized error messages to clients (no stack traces or internal details)
  - Logs full exception details server-side for debugging
  - Protects against information leakage attacks
- **Repository Pattern** ✅ - Abstraction layer between controllers and database
  - Generic `IRepository<T>` base with CRUD operations
  - 5 domain-specific repositories: Product, Category, Order, ShoppingCart, StockAlert
  - Improves testability with mockable data access
  - Centralizes data access logic for easier security audits
- **Service Layer** ✅ - Business logic extracted from controllers to service classes
  - 5 services with clear single responsibilities
  - OrderService handles complex transaction logic (cart → order → stock update)
  - Controllers reduced by 42% (793 → 461 lines)
  - Clean architecture: Controller → Service → Repository → DbContext

**Performance Optimizations:**
- **Database Indexes** - Foreign key indexes on Products.CategoryId, OrderItems.OrderId/ProductId, CartItems relationships
- **Query Optimization** - Used `AsNoTracking()` for read-only operations

- **Caching** - `IMemoryCache` caches frequently accessed product listings (reduces DB round-trips)
- **Pagination** - All list endpoints support `page` and `pageSize` parameters (default: 11 items/page)
- **Transactions** - `TransactionScope` ensures atomic operations in order creation (cart → order → stock update)
- **Image Storage** - JPEG format for smaller file sizes (byte[] stored in SQL Server, avoids filesystem complexity)

---

## 🛠️ Tech Stack

**Backend:**
- **ASP.NET Core 8.0** - Web API + MVC Framework
- **Entity Framework Core 8.0** - ORM with Code-First migrations
- **SQL Server LocalDB** - Development database
- **ASP.NET Identity** - Authentication and authorization
- **RestSharp** - HTTP client for API consumption
- **Rate Limiting** - Built-in ASP.NET Core rate limiting middleware
- **FluentValidation** - Library for validating object data values with fluent rules

**Frontend:**
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - Responsive UI framework
- **JavaScript / Fetch API** - AJAX operations
- **ViewComponents** - Reusable UI components

**Architecture:**
- **3-Layer Architecture** - Core (Domain), API (REST), Web (MVC), Contracts (DTOs)
- **Dependency Injection** - Built-in ASP.NET Core DI container
- **Configuration Management** - appsettings.json for environment-specific settings
- **Caching** - IMemoryCache for performance optimization
- **Transactions** - TransactionScope for data consistency  
- **RestSharp + Newtonsoft.Json** (API consumption in MVC layer)  

---


## 🗄️ Layer Communication Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      Eshop.Web (MVC)                        │
│  Presentation Layer - Razor Views, Controllers, Services    │
│  → AppIdentityDbContext (Identity/Auth - Web concern)       │
│  → Consumes API via RestSharp for business operations       │
└─────────────────────────────────────────────────────────────┘
                              ↓ HTTP/REST
┌─────────────────────────────────────────────────────────────┐
│                       Eshop.API                             │
│        REST API Controllers + Swagger Documentation         │
│  → Direct access to ApplicationDbContext for CRUD           │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      Eshop.Core                             │
│  Domain Layer - Entities, Business Logic, DbContext         │
│  → ApplicationDbContext (Products, Orders, Categories)      │
│  → Shared by API & referenced by other layers              │
└─────────────────────────────────────────────────────────────┘
                              ↔
┌─────────────────────────────────────────────────────────────┐
│                    Eshop.Contracts                          │
│         Data Transfer Objects (DTOs) - Shared Layer         │
│  → ProductDto, CategoryDto, OrderDto, CartDto, etc.         │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities:

**🔷 Eshop.Core** (Domain/Data Layer)
- Domain entities: `Product`, `Category`, `Order`, `OrderItem`, `ShoppingCart`
- `ApplicationDbContext` for business data
- Database migrations for business schema
- Shared across API and referenced by Web

**🔷 Eshop.Contracts** (DTOs)
- Data Transfer Objects for API communication
- Decouples domain models from API responses
- Used by both API and Web layers

**🔷 Eshop.API** (REST API)
- RESTful endpoints for CRUD operations
- Direct database access via `ApplicationDbContext`
- Swagger/OpenAPI documentation
- Stateless, token-based authentication

**🔷 Eshop.Web** (Presentation/MVC)
- Razor views with Bootstrap 5
- `AppIdentityDbContext` for user authentication (Web-specific concern)
- Services consume API via HTTP (RestSharp)
- Shopping cart UI, product browsing, order management

---

## 🗄️ Database Architecture

### Two Separate DbContexts & Databases

This project uses **two isolated database contexts** for different concerns:

#### 1. **ApplicationDbContext** (`Eshop.Core/Data/`)
- **Purpose:** Business domain data
- **Database:** `EshopDb`
- **Contains:** Products, Categories, Orders, OrderItems, ShoppingCarts
- **Used by:** API (direct access), Web (via API calls)

#### 2. **AppIdentityDbContext** (`Eshop.Web/Data/`)
- **Purpose:** User authentication & authorization
- **Database:** `EshopIdentityDb`
- **Contains:** AspNetUsers, AspNetRoles, AspNetUserRoles, etc. (7 Identity tables)
- **Used by:** Web layer only (login, registration, role management)

### Why This Design?

✅ **Separation of Concerns** - Authentication is a presentation-layer concern, not core business logic  
✅ **Security Isolation** - User credentials separated from business data  
✅ **Independent Scaling** - Deploy databases on different servers if needed  
✅ **Clean Migrations** - Identity changes don't affect business schema  

> **Key Principle:** Core layer = pure business logic (Products, Orders). Web layer = user-facing features (Login, Registration). Identity management is NOT core business logic.
---

### TODO 🔜 

#### ✅ Completed
- [x] **Input Validation** - FluentValidation for DTOs (ProductDto, CategoryDto, PlaceOrderRequestDto)
  - 3 validators with custom rules, integrated with ASP.NET Core model binding
- [x] **Error Handling** - Global exception middleware to prevent sensitive data exposure
  - Sanitized error messages, server-side logging, no stack traces to clients
- [x] **HTTPS/TLS** - All communication encrypted, cookies protected with Secure flag
  - API: https://localhost:7068, Web: https://localhost:7252
- [x] **Repository Pattern** - Abstraction layer between controllers and database
  - Generic base + 5 domain repositories (12 files total)
  - All controllers refactored to use repositories via services
- [x] **Service Layer** - Business logic extracted from controllers
  - 5 services (CategoryService, ProductService, OrderService, ShoppingCartService, StockAlertService)
  - Controllers reduced by 332 lines (-42%)
  - OrderService handles 67-line transaction logic previously in controller
- [x] **IDOR Temporary Fix** - Manual authorization checks on user-scoped endpoints
  - Validates `currentUserId == userId || isAdmin` before data access

#### 🚧 High Priority (Security & Core Functionality)
- [ ] **IDOR Permanent Fix** - Custom authorization attributes with policy-based authorization
  - Replace manual `if (currentUserId != userId)` checks with `[AuthorizeOwnerOrAdmin]` attribute
  - Centralized authorization logic, reusable across controllers
- [ ] **API Authentication** - JWT or Shared Cookies (for later if API consumed by mobile/SPA)
  - Currently: Web MVC co (Future)
- [ ] **Unit Tests** - xUnit project for service layer tests
  - Mock repositories with Moq for isolated unit tests
  - Test OrderService transaction logic, validation rules
- [ ] **Integration Tests** - TestServer for API endpoint tests
  - Test full request/response cycle with in-memory database
- [ ] **Load Testing** - k6 or Apache JMeter for performance benchmarks

#### 🚀 DevOps & Deployment (Future - Keep for end)
- [ ] **Docker Containerization** - Multi-stage Dockerfiles for API and Web
  - Docker Compose for local development with SQL Server container
- [ ] **CI/CD Pipeline** - GitHub Actions workflow for automated build/test/deploy
- [ ] **Azure Deployment** - App Service or Container Apps with managed SQL
- [ ] **Monitoring & Logging** - Application Insights telemetry + Serilog structured logskground job)
- [ ] **Concurrency Control** - Handle race conditions for limited stock (optimistic/pessimistic locking)

#### 🧪 Testing & Quality
- [ ] **Unit Tests** - xUnit tests for services and business logic
- [ ] **Integration Tests** - TestServer for API endpoint testing
- [ ] **Load Testing** - Performance benchmarks and stress testing

#### 🚀 DevOps & Deployment
- [ ] **Docker Containerization** - Dockerfile for API and Web projects
- [ ] **CI/CD Pipeline** - GitHub Actions for automated build/test/deploy
- [ ] **Azure Deployment** - App Service or Container Apps configuration
- [ ] **Monitoring & Logging** - Application Insights / Serilog structured logging

---

## 📚 Documentation
Document everything, maybe i will forget them in the future. I will try to think add as many concepts that can solve real world problems. I will upload Docs when polished.
Detailed documentation available in [`/docs`](docs/):
---