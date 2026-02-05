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

### 🔒 Security & Performance Features

**Authentication & Authorization:**
- **ASP.NET Identity + Cookies** - Session-based authentication on Web layer with persistent login cookies
  - Why cookies? Traditional MVC app with server-side rendering - cookies are simpler than JWT for browser-to-server auth
  - Secure password hashing via Identity framework (PBKDF2)
  - Role-based access control (Admin/Customer roles)
  - Session persistence across requests without token management complexity

**API Security:**
- **Rate Limiting** ✅ - Global rate limiter: 100 requests/minute per user/IP to prevent DDoS attacks
  - Built-in ASP.NET Core 8 `AddRateLimiter()` middleware
  - Custom rejection messages for throttled requests
- **IDOR Vulnerability Fixed** ✅ - Authorization checks on user-scoped endpoints (OrdersController, ShoppingCartController)
  - Validates `currentUserId == userId` or `User.IsInRole("Admin")` before data access
  - Prevents unauthorized access to other users' orders/carts
- **Admin-Only Endpoints** ✅ - Write operations protected with `[Authorize(Roles = "Admin")]`
  - Products: Create, Update, Delete, Image Upload
  - Categories: Create, Update, Delete
  - Stock Alerts: All admin dashboard operations
- **CORS Configuration** ✅ - API restricted to trusted Web origin (`https://localhost:5161`, `http://localhost:5161`)
  - Fixed from incorrect port 7071 → correct 5161
  - Configured with `AllowCredentials` for cookie support
- **No Hardcoded URLs** ✅ - All service API URLs configured via `appsettings.json:ApiSettings:BaseUrl`
  - Environment-specific configurations (Development, Staging, Production)
  - Easy deployment without code changes

**Performance Optimizations:**
- **Database Indexes** - Foreign key indexes on Products.CategoryId, OrderItems.OrderId/ProductId, CartItems relationships
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

## 🔐 Recent Security & Architecture Improvements

### Security Fixes (February 2026)
- ✅ **IDOR Vulnerability Fixed** - Added authorization checks to `OrdersController.GetOrdersByUser` and `ShoppingCartController.GetCartByUser` to prevent unauthorized access to other users' data
- ✅ **Rate Limiting Implemented** - Global API rate limiter (100 requests/minute per user) protects against DDoS attacks
- ✅ **CORS Misconfiguration Fixed** - Updated allowed origin from port 7071 to correct Web app port 5161
- ✅ **Hardcoded URLs Eliminated** - All service classes now use `IConfiguration` to read API base URL from `appsettings.json`
- ✅ **Admin Authorization Added** - All administrative endpoints (Create/Update/Delete for Products/Categories) now require Admin role

### Performance Improvements
- ✅ **Database Indexing** - Added foreign key indexes on Products, Orders, CartItems tables
- ✅ **Memory Caching** - Implemented `IMemoryCache` for frequently accessed product data
- ✅ **Query Optimization** - Used `AsNoTracking()` for read-only operations
- ✅ **Pagination** - Implemented server-side pagination for Products (default 11 items per page)

### Architecture Enhancements
- ✅ **Configuration-Based Services** - All services inject `IConfiguration` for flexible URL management
- ✅ **Transaction Management** - Order creation uses `TransactionScope` to ensure data consistency
- ✅ **Separation of Concerns** - Two separate DbContexts (ApplicationDbContext for business, AppIdentityDbContext for auth)
- ✅ **Stock Alerts System** - Complete API and UI implementation for low inventory notifications

### ⚠️ Known Limitations
- **API Authentication Not Fully Configured** - API has `[Authorize]` attributes but lacks JWT/Bearer token authentication scheme. Currently relies on manual `userId` validation.
- **No Unit/Integration Tests** - Test coverage is 0% (planned for future implementation)
- **No Repository Pattern** - Controllers directly inject DbContext (refactoring planned)
- **Business Logic in Controllers** - OrdersController.CreateOrder contains 67 lines of business logic (should move to service layer)

📚 **For detailed security analysis, see:** [SECURITY_IMPROVEMENTS.md](SECURITY_IMPROVEMENTS.md)

---

## 🗄️ Database Architecture

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

📚 **For detailed architecture explanation, see:** [`/docs/DATABASE_ARCHITECTURE.md`](docs/DATABASE_ARCHITECTURE.md)

---

## 🔐 Security & Configuration

### Sensitive Data Management

**⚠️ IMPORTANT:** Never commit secrets to version control!

#### Connection Strings
Connection strings with credentials should be stored in:

1. **Development:** `appsettings.Development.json` (gitignored)
2. **Production:** Environment variables or Azure Key Vault

#### Recommended Structure:

**appsettings.json** (committed to repo):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EshopDb;",
    "IdentityConnection": "Server=(localdb)\\mssqllocaldb;Database=EshopIdentityDb;"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7068/api"
  }
}
```

**appsettings.Development.json** (NOT committed - contains secrets):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EshopDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;",
    "IdentityConnection": "Server=YOUR_SERVER;Database=EshopIdentityDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

#### What's in .gitignore:
- `appsettings.Development.json`
- `appsettings.Production.json`
- `*.user` files
- `bin/`, `obj/` directories
- `.env` files (if used)
- `Figma_Delete/` folder

---

## 🗃️ Database Migrations

### Setup Databases (First Time)

#### 1. Create Business Database (ApplicationDbContext):
```powershell
# From Eshop.Web directory
cd Eshop.API

# Create migration
dotnet ef migrations add InitialCreate `
  --context ApplicationDbContext `
  --project ../Eshop.Core `
  --output-dir Migrations/Application

# Apply migration
dotnet ef database update `
  --context ApplicationDbContext `
  --project ../Eshop.Core
```

#### 2. Create Identity Database (AppIdentityDbContext):
```p� Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB included with Visual Studio)
- Visual Studio 2022 or VS Code

### Setup & Run

1. **Clone the repository**
   ```powershell
   git clone <your-repo-url>
   cd NetShop
   ```

2. **Restore packages**
   ```powershell
   cd Eshop.Web
   dotnet restore
   ```

3. **Setup databases** (see [`/docs/MIGRATIONS_GUIDE.md`](docs/MIGRATIONS_GUIDE.md) for details)
   ```powershell
   # Business database
   cd Eshop.API
   dotnet ef database update --context ApplicationDbContext --project ../Eshop.Core
   
   # Identity database
   cd ../Eshop.Web
   dotnet ef database update --context AppIdentityDbContext
   ```

4. **Run the applications**
   ```powershell
   # Terminal 1 - API
   cd Eshop.API
   dotnet run
   
   # Terminal 2 - Web
   cd Eshop.Web
   dotnet run
   ```

5. **Access the application**
   - Web UI: https://localhost:7071
   - API + Swagger: https://localhost:7068/swagger

### Default Admin Account
- **Email:** `admin@eshop.com`
- **Password:** `Admin123!`

---

## 🔐 Security & Configuration

**⚠️ Important:** Never commit secrets to version control!

- Connection strings with credentials → `appsettings.Development.json` (gitignored)
- Production secrets → Environment variables or Azure Key Vault
- `.gitignore` already configured for sensitive files

📚 **For configuration details, see:** [`/docs/SECURITY_CONFIG.md`](docs/SECURITY_CONFIG.md📋 Roadmap & Future Features

### 🔜 In Progress / Planned
- [ ] **API Versioning** - Versioned API endpoints (v1, v2) for backward compatibility
- [ ] **Payment Integration** - Stripe payment gateway for checkout
- [ ] **Language Switcher UI** - Dropdown in navbar for EN/EL selection
- [ ] **Product Search** - Advanced search with filters (price range, category, stock status)
- [ ] **Product Reviews** - Customer ratings and reviews
- [ ] **Repository Pattern** - Refactor from direct DbContext to Repository abstraction layer
- [ ] **JWT Authentication for API** - Replace cookie auth with stateless JWT tokens for API layer

### 🧪 Testing & Quality
- [ ] Unit tests with xUnit
- [ ] Integration tests with TestServer
- [ ] API endpoint tests

### 🚀 DevOps & Deployment
- [ ] Docker containerization
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Azure deployment configuration

---

## 📚 Documentation

Detailed documentation available in [`/docs`](docs/):
- [Database Architecture](docs/DATABASE_ARCHITECTURE.md) - In-depth explanation of two DbContext design
- [Migrations Guide](docs/MIGRATIONS_GUIDE.md) - Complete guide for database migrations
- [Security Configuration](docs/SECURITY_CONFIG.md) - Best practices for secrets management
- [Security Improvements Plan](SECURITY_IMPROVEMENTS.md) - Comprehensive security audit and improvement roadmap

---

## 📝 Recent Changes & Commit History

### Latest Commits (February 2026)
```bash
fffe5a8 docs: Update documentation and improve .gitignore
9102f66 chore(migrations): Add initial database migrations
ae18539 feat(views): Add complete Products CRUD views
d5dfda6 feat(stockalerts): Add StockAlerts API and ViewComponent
86a26fb feat(viewmodels): Add missing properties to Product ViewModels
d55ed7d chore: Fix configuration and cleanup code
a8f37dc feat(api): Add role-based authorization to admin endpoints
f4844ec refactor(services): Replace hardcoded API URLs with configuration
cc1a463 feat(api): Add rate limiting and fix CORS configuration
720494a fix(security): Add authorization checks to prevent IDOR vulnerabilities
```

**Key Improvements:**
- ✅ **Security Fixes** - IDOR vulnerability patched, rate limiting enabled, CORS configured
- ✅ **Configuration** - Removed hardcoded URLs, externalized to appsettings.json
- ✅ **Authorization** - Admin-only endpoints protected with role-based access control
- ✅ **Features** - StockAlerts system (API + ViewComponent), complete Products views
- ✅ **Infrastructure** - Database migrations, ViewModels extended, documentation updated

---

## 👨‍💻 Author

**Panagiotis Tzivaras**  
📅 Last Updated: February 2026