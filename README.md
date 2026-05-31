# Eshop Project
This project follows a **3-Layer Architecture** pattern with clear separation of concerns.

A full-stack **E-commerce Web Application** built with modern technologies:  
- **Backend**: ASP.NET Core Web API (C#), Entity Framework Core, SQL Server  
- **Frontend**: ASP.NET Core MVC (Razor views, Bootstrap 5, JavaScript)  
- **Architecture**: 3-layer (Core, API, Web), DTOs, Services, Dependency Injection  
- **Authentication/Authorization**: ASP.NET Identity with Roles (Admin / Customer) 
- **Database**: SQL Server with Code-First Migrations, Seeders for initial data  
---

## Features

### Authentication & Authorization
- ASP.NET Identity integration with role-based access
- Two roles: **Admin** and **Customer**
- Secure login, registration, password management
- Auto-seeded admin account: `admin@eshop.com` / `Admin123!`

### Product & Category Management (Admin Only)
- Full CRUD operations for Products and Categories
- Product image upload and storage (BLOB in database)
- Pagination with advanced search & filters: text search, category, **price range**, **stock status**
- Category-based product browsing

### Shopping Cart
- Add/Remove/Update product quantities
- Persistent cart per authenticated user
- **AJAX add to cart** — no page reload, Bootstrap toast notification, live badge count in navbar
- Seamless checkout integration

### Orders
- Place orders from shopping cart
- Order confirmation and history tracking
- Paginated order history per user

### Admin Dashboard
- **Stats cards**: Total products, orders, revenue (+ this month breakdown), low-stock alerts
- **Monthly chart**: Orders (bar) + Revenue (line) for last 6 months via Chart.js
- Recent orders list with order ID, date, total

### Stock Alerts
- **Automatic detection** via background service — runs every 10 minutes, triggers alerts when stock ≤ 5 units
- Prevents duplicate alerts (skips products with existing unacknowledged alert)
- Admin can view, acknowledge, and dismiss alerts
- Alert count badge in navbar (live count from API)

### Product Reviews
- Customer rating system (1-5 stars) with comments (max 1000 chars)
- CRUD operations with authorization (users can only edit/delete their own reviews)
- API endpoints: GET by product/user, POST/PUT/DELETE
- IDOR protection with ownership validation

### User Profile
- View user account information (email, account details)
- Personal order history with pagination
- Order details and status tracking

### Product Wishlist
- Authenticated users can save products to a personal wishlist
- ♡ button on every product card and details page
- Dedicated `/Wishlist` page with all saved products and remove option
- Stored in database per user (persists across sessions)

### Localization
- Multi-language support: English (default) & Greek
- Cookie-based language preference

### UI/UX
- Fully responsive Bootstrap 5 layout
- Product cards with images, titles, descriptions, pricing
- Toast notifications for user actions
- Role-based UI elements (Admin sees Edit/Delete, Customers see Add to Cart)
- Pagination with page size control
- Image upload with preview (JPEG format for optimal storage)
- **UI Animations**: page fade-in on load, card hover lift, button press scale, navbar scroll shadow, cart badge bounce, stat count-up on Admin Dashboard

### Security & Performance 

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
- **IDOR Protection (Temporary Fix)** ✅ - Authorization checks on user-scoped endpoints (OrdersController, ShoppingCartController)
  - Validates `currentUserId == userId` or `User.IsInRole("Admin")` before data access
  - Prevents unauthorized access to other users' orders/carts
  - **Note:** Currently the API uses a NoOp auth handler for internal Web→API calls. Without JWT, direct API access (Postman/curl) bypasses Identity. Full fix requires JWT — see TODO.
- **Admin-Only Endpoints** ✅ - Write operations protected with `[Authorize(Roles = "Admin")]`
  - Products: Create, Update, Delete, Image Upload
  - Categories: Create, Update, Delete
  - Stock Alerts: All admin dashboard operations
- **Customer-Only Endpoints** ✅ - Write operations protected with `[Authorize(Roles = "Customer")]`
- **CORS Configuration** ✅ - API accepts requests only to trusted web origin
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
  - Domain-specific repositories for every entity (Product, Category, Order, ShoppingCart, StockAlert, Review, Wishlist)
  - Improves testability with mockable data access
  - Centralizes data access logic for easier security audits
- **Service Layer** ✅ - Business logic extracted from controllers to service classes
  - Services for every domain (Order, Product, Category, ShoppingCart, StockAlert, Review, Wishlist)
  - Clean architecture: Controller → Service → Repository → DbContext

**Performance Optimizations:**
- **Database Indexes** - Foreign key indexes on Products.CategoryId, OrderItems.OrderId/ProductId, CartItems relationships
- **Query Optimization** - Used `AsNoTracking()` for read-only operations
- **Optimistic Concurrency Control** - `[Timestamp] RowVersion` on Product entity prevents lost updates
  - Detects simultaneous modifications by multiple users
  - DbUpdateConcurrencyException handling with user-friendly error messages
  - Applied to stock updates during order placement
- **Caching** - `IMemoryCache` caches frequently accessed product listings (reduces DB round-trips)
- **Pagination** - All list endpoints support `page` and `pageSize` parameters (default: 11 items/page)
- **Transactions** - `TransactionScope` ensures atomic operations in order creation (cart → order → stock update)
- **Image Storage** - JPEG format for smaller file sizes (byte[] stored in SQL Server, avoids filesystem complexity)

---

## Tech Stack

**Backend:**
- **ASP.NET Core 8.0** - Web API + MVC Framework
- **Entity Framework Core 8.0** - ORM with Code-First migrations
- **SQL Server LocalDB** - Development database
- **ASP.NET Identity** - Authentication and authorization
- **RestSharp 112.1.0** - HTTP client for MVC → API communication
  - Simpler code (automatic JSON deserialization)
  - Built-in error handling
  - Automatic query parameter encoding
- **Asp.Versioning.Mvc 8.1.1** - API endpoint versioning
- **Rate Limiting** - Built-in ASP.NET Core rate limiting middleware
- **FluentValidation** - DTO validation with fluent rules

**Testing:**
- **xUnit** - Unit testing framework
- **Moq** - Mocking library for repository isolation in unit tests
- **FluentAssertions** - Readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing with in-process TestServer
- **SQLite (in-memory)** - Lightweight test database for integration tests

**Frontend:**
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - Responsive UI framework
- **ViewComponents** - Reusable UI components

**Architecture:**
- **3-Layer Architecture** - Core (Domain), API (REST), Web (MVC), Contracts (DTOs)
- **Dependency Injection** - Built-in ASP.NET Core DI container
- **Configuration Management** - appsettings.json for environment-specific settings
- **Caching** - IMemoryCache for performance optimization
- **Transactions** - TransactionScope for data consistency  

---


## Layer Communication Flow

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
- Domain entities: `Product`, `Category`, `Order`, `OrderItem`, `ShoppingCart`, `CartItem`, `StockAlert`, `Review`, `WishlistItem`
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
- REST API — consumed internally by the MVC Web layer via RestSharp. No external auth scheme (TODO: JWT)

**🔷 Eshop.Web** (Presentation/MVC)
- Razor views with Bootstrap 5
- `AppIdentityDbContext` for user authentication (Web-specific concern)
- Services consume API via HTTP (RestSharp)
- Shopping cart UI, product browsing, order management

---

## Database Architecture

### Two Separate DbContexts & Databases

This project uses **two isolated database contexts** for different concerns:

#### 1. **ApplicationDbContext** (`Eshop.Core/Data/`)
- **Purpose:** Business domain data
- **Database:** `EshopDb`
- **Contains:** Products, Categories, Orders, OrderItems, ShoppingCarts, CartItems, StockAlerts, Reviews, WishlistItems
- **Used by:** API (direct access), Web (via API calls)

#### 2. **AppIdentityDbContext** (`Eshop.Web/Data/`)
- **Purpose:** User authentication & authorization
- **Database:** `EshopIdentityDb`
- **Contains:** AspNetUsers, AspNetRoles, AspNetUserRoles, etc. (7 Identity tables)
- **Used by:** Web layer only (login, registration, role management)

### Why This Design?
- **Separation of Concerns** - Authentication is a presentation-layer concern, not core business logic
- **Security Isolation** - User credentials separated from business data
- **Independent Scaling** - Deploy databases on different servers if needed
- **Clean Migrations** - Identity changes don't affect business schema
---

####  Detailed Concepts in Project
- [x] **Input Validation** - FluentValidation for DTOs (ProductDto, CategoryDto, PlaceOrderRequestDto)
- [x] **Error Handling** - Global exception middleware to prevent sensitive data exposure
- [x] **HTTPS/TLS** - All communication encrypted, cookies protected with Secure flag
- [x] **Repository Pattern** - Abstraction layer between controllers and database
- [x] **Service Layer** - Business logic extracted from controllers
- [x] **IDOR Permanent Fix** - Policy-based authorization with custom attributes
  - Custom `[AuthorizeOwnerOrAdmin]` attribute replaces manual checks
  - Route/query parameter validation via authorization handler
  - Centralized authorization logic (OwnerOrAdminRequirement + Handler)
  - Applied to: GetOrdersByUser, GetCartByUser endpoints
- [x] **API Versioning** - All API endpoints versioned at v1.0
- [x] **Optimistic Concurrency Control** - Product stock updates with conflict detection
- [x] **Product Reviews** - Customer rating system (1-5 stars) with CRUD and authorization
- [x] **Unit Tests** - Service layer unit tests with mocked repositories (all domain services covered)
- [x] **Consistent Error Responses** - ProblemDetails-style JSON for all errors (incl. rate limiting)
- [x] **Correlation IDs** - X-Correlation-Id header on every request/response, added to log scope
- [x] **Structured Logging** - Correlation ID in log scope for per-request tracing
- [x] **Pagination/Filtering Consistency** - Added pagination to GetAllOrders and GetReviewsByProduct
- [x] **AJAX Cart Updates** - Add to cart without page reload, toast notification, live cart badge
- [x] **Admin Dashboard** - Stats cards (products, orders, revenue, alerts), monthly chart (Chart.js), recent orders
- [x] **Exception Mapping** - Domain exceptions mapped to correct HTTP status codes (400/403/404/501/500)
- [x] **Stock Alert Triggers** - Background service (`IHostedService`) runs every 10 minutes(Detects products with stock ≤ 5 units, creates alert only if no existing unacknowledged alert)
- [x] **Product Wishlist** - button on product cards and details page
- [x] **Product Search** - Advanced filters: price range (min/max), stock status, text + category
- [x] **UI Animations** - Card hover lift, page fade-in, navbar scroll shadow, badge bounce, count-up etc
- [x] **Integration Tests** - TestWebApplicationFactory with SQLite in-memory DB and TestAuthenticationHandler
  - All API endpoints tested: routing, authorization (401 for unauthenticated), pagination, filtering

#### Not Implemented Concepts(Important)
- [ ] **JWT Authentication** — Required to fully secure direct API access (Postman/curl/mobile)
  - **Current state:** API uses a NoOp auth handler for internal Web→API calls. ASP.NET Identity protects end-users via the Web layer, but the API itself has no real authentication scheme.
  - **Why it matters:** Without JWT, anyone who finds the API URL can call it directly without authentication. The IDOR checks in controllers are the only line of defense.
  - **When to implement:** If the API is ever exposed externally (mobile app, React SPA, public docs).

- [ ] **Load Testing** - k6 or Apache JMeter for performance benchmarks

- [ ] **Monitoring & Logging** - Application Insights telemetry + Serilog structured logs

---

## Documentation
Document everything, maybe i will forget them in the future. I will try to think and add as many concepts as i can  solving real world problems. I will upload Docs when polished.
Documentation available in docs folder!!
---