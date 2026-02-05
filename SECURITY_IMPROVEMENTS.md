# Security & Architecture Improvements - Action Plan

## 🚨 CRITICAL - Immediate Action Required

### 1. Fix IDOR Vulnerability in API
**File:** `Eshop.API/Controllers/OrdersController.cs`
**Line:** 48

**Current Code (VULNERABLE):**
```csharp
[HttpGet("user/{userId}")]
public async Task<ActionResult<List<OrderDto>>> GetOrdersByUser(string userId, ...)
```

**Fixed Code:**
```csharp
[HttpGet("user/{userId}")]
[Authorize]
public async Task<ActionResult<List<OrderDto>>> GetOrdersByUser(string userId, ...)
{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    // Only allow users to see their own orders (unless Admin)
    if (currentUserId != userId && !User.IsInRole("Admin"))
    {
        return Forbid();
    }
    
    // Rest of method...
}
```

**Apply same fix to:**
- `ShoppingCartController.GetCart(string userId)` - Line 43
- `ShoppingCartController.GetCartByUserId(string userId)` - Line 32
- Any other endpoint that accepts userId as parameter

---

### 2. Enable JWT Authentication in API
**File:** `Eshop.API/Program.cs`

**Add before `builder.Services.AddControllers():`**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

**Add to appsettings.json:**
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-chars-long-12345678901234567890",
    "Issuer": "EshopAPI",
    "Audience": "EshopWeb",
    "ExpiresInMinutes": 60
  }
}
```

---

### 3. Fix Hardcoded URLs in Services
**Files:** All service classes in `Eshop.Web/Services/`

**Current (BAD):**
```csharp
_client = new RestClient("http://localhost:5298/api");
```

**Fixed:**
```csharp
public class ProductService : IProductService
{
    private readonly RestClient _client;
    private readonly IConfiguration _configuration;

    public ProductService(IConfiguration configuration, ...)
    {
        _configuration = configuration;
        var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] 
            ?? throw new InvalidOperationException("API BaseUrl not configured");
        _client = new RestClient(apiBaseUrl);
    }
}
```

**Update appsettings.json:**
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7068/api"  // HTTPS not HTTP!
  }
}
```

---

## ⚠️ HIGH PRIORITY - Within 1 Week

### 4. Implement Repository Pattern

**Create:** `Eshop.Core/Repositories/IProductRepository.cs`
```csharp
namespace Eshop.Core.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
```

**Create:** `Eshop.Core/Repositories/ProductRepository.cs`
```csharp
namespace Eshop.Core.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        // ... implement other methods
    }
}
```

**Register in Program.cs:**
```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// etc...
```

---

### 5. Move Business Logic to Services

**Create:** `Eshop.API/Services/OrderService.cs`
```csharp
namespace Eshop.API.Services
{
    public interface IOrderService
    {
        Task<OrderResult> CreateOrderAsync(string userId);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, string requestingUserId);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId, int page, int pageSize);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        // Inject repositories
        public OrderService(
            IOrderRepository orderRepository,
            IShoppingCartRepository cartRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<OrderResult> CreateOrderAsync(string userId)
        {
            // Move ALL business logic from controller here
            using var transaction = new TransactionScope(...);
            
            // 1. Get cart
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return OrderResult.Failure("Cart is empty");

            // 2. Validate stock
            foreach (var item in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product.StockQuantity < item.Quantity)
                    return OrderResult.Failure($"Not enough stock for {product.Name}");
            }

            // 3. Create order
            var order = new Order { ... };
            await _orderRepository.AddAsync(order);

            // 4. Update stock
            // 5. Clear cart
            
            transaction.Complete();
            return OrderResult.Success(order.Id);
        }
    }
}
```

**Update Controller:**
```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult> CreateOrder()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var result = await _orderService.CreateOrderAsync(userId);
    
    if (!result.IsSuccess)
        return BadRequest(result.ErrorMessage);
    
    return Ok(new { orderId = result.OrderId });
}
```

---

## 📊 MEDIUM PRIORITY - Within 1 Month

### 6. Add Rate Limiting
**Install:** `Microsoft.AspNetCore.RateLimiting`

**Program.cs:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// In middleware
app.UseRateLimiter();
```

---

### 7. Add Input Validation with FluentValidation
**Install:** `FluentValidation.AspNetCore`

**Create:** `Eshop.API/Validators/PlaceOrderRequestValidator.cs`
```csharp
public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequestDto>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(450);
    }
}
```

**Register:**
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PlaceOrderRequestValidator>();
```

---

### 8. Add Global Exception Handling
**Create:** `Eshop.API/Middleware/ExceptionHandlingMiddleware.cs`
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "An error occurred processing your request",
                // DO NOT include ex.Message in production!
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

// Register
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## 📝 LOW PRIORITY - Nice to Have

### 9. Add Unit Tests
**Create:** `Eshop.Tests` project

**Example:** `ProductRepositoryTests.cs`
```csharp
public class ProductRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 10 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }
}
```

### 10. Add Integration Tests
**Create:** `Eshop.IntegrationTests` project

**Example:** `OrdersControllerTests.cs`
```csharp
public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn401_WhenNotAuthenticated()
    {
        // Arrange
        var request = new PlaceOrderRequestDto { UserId = "test" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

---

## 📈 Summary

| Priority | Issue | Severity | Effort |
|----------|-------|----------|--------|
| 🔥 Critical | IDOR Vulnerability | High | 1 hour |
| 🔥 Critical | JWT Auth missing | High | 2 hours |
| 🔥 Critical | Hardcoded URLs | Medium | 1 hour |
| ⚠️ High | Repository Pattern | Medium | 1 week |
| ⚠️ High | Business Logic in Controllers | Medium | 1 week |
| 📊 Medium | Rate Limiting | Low | 2 hours |
| 📊 Medium | Input Validation | Low | 1 day |
| 📊 Medium | Exception Handling | Low | 2 hours |
| 📝 Low | Unit Tests | Low | Ongoing |
| 📝 Low | Integration Tests | Low | Ongoing |

**Total Estimated Effort:** 2-3 weeks for full implementation

---

## 🎯 Recommended Implementation Order

1. **Week 1:** Fix Critical security issues (IDOR, JWT, URLs)
2. **Week 2:** Implement Repository Pattern
3. **Week 3:** Move Business Logic to Services + Add Tests
4. **Week 4:** Add Rate Limiting, Validation, Exception Handling

---

**Generated:** February 5, 2026  
**Project:** NetShop (Eshop)  
**Analysis by:** GitHub Copilot
