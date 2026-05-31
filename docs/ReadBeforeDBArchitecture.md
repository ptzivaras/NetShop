# NetShop Documentation

Comprehensive technical documentation for the NetShop e-commerce platform.

---

### [Database Architecture](DATABASE_ARCHITECTURE.md)
**In-depth explanation of the two-database design pattern**

Learn about:
- Why we use two separate DbContexts (`ApplicationDbContext` and `AppIdentityDbContext`)
- Security isolation, scalability, and compliance benefits
- 3-Layer Architecture principles and separation of concerns
- Real-world use cases: GDPR compliance, schema evolution, independent scaling
- FAQ: Common questions about the architecture decisions

**Read this if:** You want to understand the architectural reasoning behind using separate databases for business and identity data.

---






---

## 🚀 Quick Start

### New Developer Onboarding

1. **Clone the repository**
   ```powershell
   git clone <your-repo-url>
   cd NetShop
   ```

2. **Setup databases** (see [Migrations Guide](MIGRATIONS_GUIDE.md))
   ```powershell
   cd Eshop.Web/Eshop.API
   dotnet ef database update --context ApplicationDbContext --project ../Eshop.Core
   
   cd ../Eshop.Web
   dotnet ef database update --context AppIdentityDbContext
   ```

3. **Configure secrets** (see [Security Configuration](SECURITY_CONFIG.md))
   ```powershell
   cd Eshop.Web/Eshop.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YourConnectionString"
   
   cd ../Eshop.Web
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:IdentityConnection" "YourConnectionString"
   ```

4. **Run the applications**
   ```powershell
   # Terminal 1 - API
   cd Eshop.Web/Eshop.API
   dotnet run
   
   # Terminal 2 - Web
   cd Eshop.Web/Eshop.Web
   dotnet run
   ```

---

### Adding a New Product Field

1. Update entity in `Eshop.Core/Models/Product.cs`
2. Create migration (see [Migrations Guide](MIGRATIONS_GUIDE.md#scenario-1-business-entity-changes))
3. Apply migration
4. Update DTOs in `Eshop.Contracts`
5. Update API controllers and Web views

### Why We Have Two Databases

Read [Database Architecture](DATABASE_ARCHITECTURE.md) for the complete explanation.

**TL;DR:**
- `EshopDb` (Business) → Products, Orders, Categories
- `EshopIdentityDb` (Identity) → Users, Roles, Authentication
- Reason: Security isolation, scalability, separation of concerns

---


