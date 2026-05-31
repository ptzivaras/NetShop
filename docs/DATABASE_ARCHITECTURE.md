# 🗄️ Database Architecture - Detailed Guide

## Overview

This document explains the **two-database architecture** design decision in the NetShop project, detailing why we use separate DbContexts and databases for business and identity data.

---

## Architecture Decision: Two Separate DbContexts

### 1. ApplicationDbContext (Business Domain)

**Location:** `Eshop.Core/Data/ApplicationDbContext.cs`

**Database:** `EshopDb`

#### Entities Managed:
```
Products          - Product catalog with images, pricing, stock
Categories        - Product categories and organization
Orders            - Customer order records
OrderItems        - Individual items within orders
ShoppingCarts     - User shopping carts
CartItems         - Items in shopping carts
StockAlerts       - Low inventory notifications (future feature)
```

#### Responsibilities:
- All business domain logic
- Product and inventory management
- Order processing
- Shopping cart operations
- Read/Write operations for business data

#### Usage:
- **Eshop.API** → Direct access via Dependency Injection
- **Eshop.Web** → No direct access, consumes via REST API

#### Key Design Principles:
✅ **Single Source of Truth** - API owns business data access  
✅ **Separation of Concerns** - Business logic isolated in Core layer  
✅ **Reusability** - Can be used by multiple consumers (API, console apps, background services)  
✅ **Domain-Driven Design** - Entities represent business concepts, not infrastructure

---

### 2. AppIdentityDbContext (Identity & Authentication)

**Location:** `Eshop.Web/Data/AppIdentityDbContext.cs`

**Database:** `EshopIdentityDb`

#### Entities Managed (ASP.NET Identity):
```
AspNetUsers           - User accounts and profiles
AspNetRoles           - Role definitions (Admin, Customer)
AspNetUserRoles       - User-to-role mappings
AspNetUserClaims      - Custom user claims
AspNetUserLogins      - External login providers (Google, Facebook, etc.)
AspNetRoleClaims      - Role-based claims
AspNetUserTokens      - Authentication tokens and refresh tokens
```

#### Responsibilities:
- User authentication (login, logout)
- User registration
- Password management (reset, change)
- Role-based authorization
- External authentication providers
- Token management

#### Usage:
- **Eshop.Web** → Direct access via ASP.NET Identity services
- **Eshop.API** → No direct access, validates tokens/cookies passed from Web

#### Key Design Principles:
✅ **Presentation Layer Concern** - Authentication is a Web UI responsibility  
✅ **Security Isolation** - User credentials separated from business data  
✅ **Framework Integration** - Leverages ASP.NET Identity out-of-the-box features  
✅ **Not Core Business Logic** - Identity is infrastructure, not domain logic

---

## Why Two Separate Databases?

### ✅ Security Isolation

**Problem Solved:**
- User passwords, tokens, and personal data are highly sensitive
- Business data breaches shouldn't expose user credentials
- Different encryption and security policies for each database

**Implementation:**
```
EshopDb (Business)          EshopIdentityDb (Identity)
├─ No user passwords        ├─ Hashed passwords
├─ Business queries only    ├─ Auth queries only
├─ Public-facing data       ├─ Highly sensitive data
└─ Regular backups          └─ More frequent backups + encryption
```

---

### ✅ Scalability & Performance

**Problem Solved:**
- Authentication queries can be CPU-intensive (password hashing)
- Business queries can be data-intensive (product searches, reports)
- Scale independently based on load

**Implementation:**
```
Scenario 1: High Traffic E-commerce Sales
├─ EshopDb → Scale up (more CPU, faster SSD)
└─ EshopIdentityDb → Standard performance

Scenario 2: Mass User Registrations
├─ EshopDb → Standard performance
└─ EshopIdentityDb → Scale up (more CPU for bcrypt hashing)

Scenario 3: Production Deployment
├─ EshopDb → Primary region datacenter
└─ EshopIdentityDb → Can be in different region for compliance
```

---


## The 3-Layer Architecture Perspective

### Why AppIdentityDbContext is in Eshop.Web (Not Eshop.Core)

#### ❌ Common Mistake: Both DbContexts in Core

```
Eshop.Core/
  ├─ Data/
  │   ├─ ApplicationDbContext.cs      ❌ OK
  │   └─ AppIdentityDbContext.cs      ❌ WRONG - Violates SoC
```

**Problem:**
- Core layer becomes aware of authentication concerns
- Core layer is supposed to be **pure business logic**
- Identity is infrastructure/presentation concern, not domain logic
- Breaks "Core should have no UI dependencies" principle

---

#### ✅ Correct Approach: Separate by Concern

```
Eshop.Core/                  Eshop.Web/
  ├─ Data/                     ├─ Data/
  │   └─ ApplicationDbContext    │   └─ AppIdentityDbContext
  ├─ Models/                   ├─ Controllers/
  │   ├─ Product                 │   ├─ AccountController (login)
  │   ├─ Order                   │   └─ ProfileController (user)
  │   └─ Category               ├─ ViewModels/
                                 └─ Services/ (API clients)
```

**Reasoning:**

| Concern | Layer | Justification |
|---------|-------|---------------|
| **Product** | Core | Business entity, part of domain model |
| **Order** | Core | Business entity, part of domain model |
| **Identity** | Web | Presentation layer needs authentication |
| **Login UI** | Web | User-facing feature, not business logic |

**Key Insight:**
> "Users are not products you sell, they are people who buy products."
> 
> Identity management is **how you present your application**, not **what your application does**.

---
---


##  Questions (I tried to make it profesional)

### Q: Why not use a single database with both DbContexts?

**A:** You could, but you lose:
- Security isolation benefits
- Independent scaling
- Easier compliance management
- Clear separation of concerns at infrastructure level

It's technically possible but architecturally less clean.

---

### Q: Can I use ApplicationDbContext from Eshop.Web?

**A:** **No, you shouldn't!** This breaks the architecture:
- Web should consume API for business data
- Direct database access bypasses business logic layer
- Creates tight coupling between Web and Core

**Exception:** Read-only queries for performance optimization (rare cases)

---



If you need to display user email with an order, join the data at the **application layer** (API), not the database layer.

---

### Q: Should StockAlert use ApplicationDbContext?

**A:** **Yes!** Stock alerts are business logic (inventory management), not authentication.

```
ApplicationDbContext:
├─ Products          ← Business entity
├─ Orders            ← Business entity
└─ StockAlerts       ← Business entity (inventory concern)

AppIdentityDbContext:
├─ AspNetUsers       ← Who can log in
└─ AspNetRoles       ← What they can do
```

---
### Key Details

**Two DbContexts for two different concerns:**
- `ApplicationDbContext` → Business domain (Products, Orders)
- `AppIdentityDbContext` → User authentication (Login, Registration)

**Location matters:**
- Business context in `Eshop.Core` (shared, reusable)
- Identity context in `Eshop.Web` (presentation-specific)

**Two databases for security, scalability, compliance:**
- `EshopDb` → Business data
- `EshopIdentityDb` → User credentials

**3-Layer Architecture principle:**
- Core = Pure business logic (no auth concerns)
- Web = Presentation + authentication
- API = Business operations gateway

---
---
