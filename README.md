# 🛒 Eshop Project
A massive Eshop Project focusing on backend.
A full-stack **E-commerce Web Application** built with modern technologies:  
- **Backend**: ASP.NET Core Web API (C#), Entity Framework Core, SQL Server  
- **Frontend**: ASP.NET Core MVC (Razor views, Bootstrap 5, JavaScript)  
- **Architecture**: 3-layer (Core, API, Web), DTOs, Services, Dependency Injection  
- **Authentication/Authorization**: ASP.NET Identity with Roles (Admin / Customer)  
- **Database**: SQL Server with Code-First Migrations, Seeders for initial data  

---

## 🚀 Features

### 👤 Authentication & Authorization
- ASP.NET Identity integration  
- Roles: **Admin** and **Customer**  
- Secure login, registration, and role-based access  
- Admin seeding (default `admin@eshop.com` / `Admin123!`)  

### 📦 Product Management
- CRUD operations for Products and Categories (Admin only)  
- Product Images: upload and store in DB (BLOB)  
- Pagination and filtering  
- Category-based browsing  

### 🛒 Shopping Cart
- Add / Remove / Update product quantities  
- Cart persists per user  
- AJAX add-to-cart with instant UI feedback  
- Checkout integration  

### 📑 Orders
- Place order from cart  
- View order history per user  
- Admin dashboard to view/manage orders  

### 🌍 Localization
- English (default) & Greek (el)  
- Cookie-based language preference  

### 🎨 UI/UX
- Bootstrap 5 responsive layout  
- Product list with thumbnail, title, description  
- Toast notifications for actions (e.g., add to cart)  
- Clean buttons:  
  - `View` (info)  
  - `Edit` (blue, admin only)  
  - `Delete` (red, admin only)  
  - `Add to Cart` (purple/premium style for customers)  

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
Eshop.Core -> Entities, DbContext, DTOs
Eshop.API -> REST API Controllers, Swagger
Eshop.Web (MVC) -> Razor Views, Services, Identity, Cart/Orders UI

Roadmap / TODOs
1 - Stripe Payment integration for checkout
2 - Unit & Integration Testing with xUnit + TestServer
3 - Language Switcher UI (dropdown in navbar for EN/EL)

