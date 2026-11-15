# ⚡ NexusDrop — Futuristic Merch E-Commerce Platform

A production-ready, full-stack merchandise e-commerce web application built with **.NET 9 (LTS)** and **ASP.NET Core MVC**, following **Clean Architecture** principles. Designed with a high-end futuristic/cyberpunk visual identity featuring dark glassmorphism, responsive UI, and robust backend engineering.

---

## 🚀 Key Highlights & Architecture

- **Clean Layered Architecture:**
  - `ECommerce.Core`: Domain entities, value objects, and business contracts.
  - `ECommerce.Application`: Application logic, interfaces, view models, and DTOs.
  - `ECommerce.Infrastructure`: Data persistence with EF Core, SQL Server context, Identity configuration, and seed data.
  - `ECommerce.Web`: Presentation layer with ASP.NET Core MVC Razor Views, ViewComponents, and Tag Helpers.

---

## ✨ Core Features

### 🛒 Customer Experience:
- **Interactive Product Catalog:** Dynamic filtering by category (T-Shirts, Mugs, Badges), multi-criteria search, sorting, and pagination.
- **Dual Shopping Cart Pipeline:** Seamless experience for anonymous guests (cookie/session-backed) and authenticated customers.
- **Order Tracking & Checkout:** Multi-step checkout workflow with order summary and status tracking (Pending, Processing, Shipped, Delivered).
- **Authentication & Authorization:** Secure user registration, login, and profiles powered by **ASP.NET Core Identity** and role-based access control.

### 🛡️ Admin Dashboard:
- Real-time inventory overview and metrics.
- Complete CRUD operations for products and categories.
- Order management workflow and status updates.
- Customer management.

---

## 🛠️ Tech Stack

- **Backend:** C#, ASP.NET Core MVC (.NET 9), Entity Framework Core
- **Database:** Microsoft SQL Server
- **Authentication:** ASP.NET Core Identity (Role-based: Admin & Customer)
- **Frontend:** Razor Views (.cshtml), HTML5, CSS3, JavaScript, Bootstrap 5, Custom Glassmorphism UI
- **Architecture & Patterns:** Clean Architecture, Repository Pattern, Dependency Injection, DTOs

---

## ⚡ Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/NexusDrop.git
   cd NexusDrop
   ```

2. **Update Connection String:**
   Check `appsettings.json` in `ECommerce.Web` to ensure the SQL Server connection string matches your local instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarwanHosnyStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Build and Run:**
   ```bash
   dotnet build
   dotnet run --project ECommerce.Web
   ```
   *(The database and initial seed data will be created automatically on the first run).*

---

## 👤 Author
**Marwan Hosni Ibrahim**  
- LinkedIn: [https://www.linkedin.com/in/marwan-hosni-2a01072b7]  
- GitHub: [https://github.com/marwanhosni]  
- Email: marwanhosni785@gmail.com
