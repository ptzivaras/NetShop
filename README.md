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

### 🔒 Security Features
- **API Authorization** - All write operations (POST/PUT/DELETE) protected with `[Authorize]` attributes
- **Role-based Access Control** - Admin-only endpoints for product/category management
- **CORS Protection** - API restricted to trusted origins only (Web UI at localhost:7071)
- **Identity Framework** - Secure password hashing, token management, and authentication flows

---

## 🛠️ Tech Stack

- **ASP.NET Core 8**  
- **Entity Framework Core 8**  
- **SQL Server**  
- **ASP.NET Identity**  
- **Bootstrap 5**  
- **JavaScript / Fetch API**  
- **RestSharp + Newtonsoft.Json** (API consumption in MVC layer)  

---

## ⚙️ Architecture

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
- [ ] **Stock Alerts System** - Low inventory notifications for admins (entity ready, UI pending)
- [ ] **Payment Integration** - Stripe payment gateway for checkout
- [ ] **Language Switcher UI** - Dropdown in navbar for EN/EL selection
- [ ] **Product Search** - Advanced search with filters (price range, category, stock status)
- [ ] **Product Reviews** - Customer ratings and reviews

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

---

## 👨‍💻 Author

**Panagiotis Tzivaras**  
📅 Last Updated: February 2026