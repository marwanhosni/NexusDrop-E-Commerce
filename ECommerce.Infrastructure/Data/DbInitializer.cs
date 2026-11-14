using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            logger.LogInformation("Ensuring MarwanHosnyStoreDb database is created and up to date...");
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Users
            var adminEmail = "admin@marwanhosny.store";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Marwan Hosny [Founder & Lead]",
                    StreetAddress = "704 Sector 9 Cyber Way",
                    City = "Cairo",
                    State = "Cairo-Neo",
                    PostalCode = "11511-X",
                    Country = "United Neo Federation",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@2026!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var customerEmail = "customer@marwanhosny.store";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    EmailConfirmed = true,
                    FullName = "Karim Zaki",
                    StreetAddress = "42 Neon Boulevard",
                    City = "Alexandria",
                    State = "ALX",
                    PostalCode = "21500",
                    Country = "United Neo Federation",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(customerUser, "Customer@2026!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            // 3. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "T-Shirts",
                        Slug = "t-shirts",
                        Description = "High-grade cybernetic streetwear woven with carbon-weave cotton and reactive luminescent dyes.",
                        IconClass = "bi-tag-fill",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Mugs",
                        Slug = "mugs",
                        Description = "Thermal-regulating drinkware engineered with double-wall ceramic and heat-reactive neon holographic glaze.",
                        IconClass = "bi-cup-hot-fill",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Badges",
                        Slug = "badges",
                        Description = "Collectible laser-etched metallic insignias, UV-reactive enamel pins, and magnetic cyber emblems.",
                        IconClass = "bi-shield-shaded",
                        DisplayOrder = 3,
                        IsActive = true
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 4. Seed Products
            if (!await context.Products.AnyAsync())
            {
                var tshirtsCat = await context.Categories.FirstAsync(c => c.Slug == "t-shirts");
                var mugsCat = await context.Categories.FirstAsync(c => c.Slug == "mugs");
                var badgesCat = await context.Categories.FirstAsync(c => c.Slug == "badges");

                var products = new List<Product>
                {
                    // T-SHIRTS
                    new Product
                    {
                        Name = "Neon Samurai Oversized Cyber Tee",
                        Slug = "neon-samurai-oversized-cyber-tee",
                        ShortDescription = "Heavyweight 280GSM organic cotton featuring dual-tone neon purple and cyan silkscreen graphics.",
                        Description = "Forged for the digital ronin. The Neon Samurai Tee combines luxury heavyweight French terry cotton with high-frequency reflective screen printing that illuminates under ultraviolet and street lamps. Relaxed dropped-shoulder drop cut with reinforced collar seams.",
                        Price = 48.00m,
                        OriginalPrice = 60.00m,
                        SKU = "TSH-SAMURAI-01",
                        StockQuantity = 45,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = tshirtsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/tshirt-samurai.svg", AltText = "Neon Samurai Cyber Tee", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "Quantum Glitch Protocol Tee",
                        Slug = "quantum-glitch-protocol-tee",
                        ShortDescription = "Abstract data stream graphic rendered in electric blue phosphorescent ink on charcoal fabric.",
                        Description = "Channel the raw signal of decentralized cyberspace. The Quantum Glitch Protocol shirt features a high-density matrix schematic with tactile rubberized print accents that reflect low-ambient light.",
                        Price = 42.00m,
                        OriginalPrice = 52.00m,
                        SKU = "TSH-QUANTUM-02",
                        StockQuantity = 32,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = tshirtsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/tshirt-quantum.svg", AltText = "Quantum Glitch Protocol Tee", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "Zero-Day Cyberdeck Minimalist Shirt",
                        Slug = "zero-day-cyberdeck-shirt",
                        ShortDescription = "Clean, technical typography on jet-black acid-washed combed ringspun cotton.",
                        Description = "Subtle on the surface, devastating in execution. The Zero-Day shirt features discreet hexadecimal kernel code along the back spine and a glowing holographic chest glyph.",
                        Price = 38.00m,
                        OriginalPrice = null,
                        SKU = "TSH-ZERODAY-03",
                        StockQuantity = 20,
                        IsFeatured = false,
                        IsActive = true,
                        CategoryId = tshirtsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/tshirt-zeroday.svg", AltText = "Zero-Day Minimalist Shirt", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "HyperDrive Synthwave Retro Tee",
                        Slug = "hyperdrive-synthwave-retro-tee",
                        ShortDescription = "Outrun gradient grid with holographic chrome foil stamp detailing.",
                        Description = "Cruising down endless neon highways at 88 MPH. Printed with premium chrome foil stamp accents that shimmer when caught in headlights and club strobes.",
                        Price = 45.00m,
                        OriginalPrice = 55.00m,
                        SKU = "TSH-HYPER-04",
                        StockQuantity = 18,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = tshirtsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-8),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/tshirt-hyperdrive.svg", AltText = "HyperDrive Synthwave Tee", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },

                    // MUGS
                    new Product
                    {
                        Name = "Thermal Shifting CyberMatrix Ceramic Mug",
                        Slug = "thermal-shifting-cybermatrix-mug",
                        ShortDescription = "Heat-activated color shifting mug that reveals glowing neon circuitry as hot liquid is poured.",
                        Description = "Wake up your neuro-synapses. Engineered with thermochromic nano-crystal glaze, this matte black ceramic mug transforms in real-time when hot coffee or tea is introduced, unlocking a brilliant electric cyan matrix diagram.",
                        Price = 24.00m,
                        OriginalPrice = 30.00m,
                        SKU = "MUG-MATRIX-01",
                        StockQuantity = 60,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = mugsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-18),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/mug-matrix.svg", AltText = "Thermal Shifting CyberMatrix Mug", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "Caffeine Core Overclocked Steel Tumbler",
                        Slug = "caffeine-core-steel-tumbler",
                        ShortDescription = "Double-wall vacuum insulated 450ml matte obsidian stainless steel tumbler with neon purple ring.",
                        Description = "Keeps fuel at optimum temperature for up to 12 hours of uninterrupted coding sprints. Laser-machined grade 304 stainless steel with spill-proof magnetic slide lid and non-slip silicone base.",
                        Price = 32.00m,
                        OriginalPrice = 40.00m,
                        SKU = "MUG-CORE-02",
                        StockQuantity = 25,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = mugsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-12),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/mug-steel.svg", AltText = "Overclocked Steel Tumbler", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "Neon Horizon Ceramic Espresso Cup",
                        Slug = "neon-horizon-espresso-cup",
                        ShortDescription = "Compact 120ml double-glazed ceramic cup with iridescent electroplated rim.",
                        Description = "Crafted for intense, high-octane shots of espresso. Features an ergonomic thumb groove and an iridescent violet-to-cyan gradient electroplated titanium glaze.",
                        Price = 18.00m,
                        OriginalPrice = null,
                        SKU = "MUG-ESPRESSO-03",
                        StockQuantity = 40,
                        IsFeatured = false,
                        IsActive = true,
                        CategoryId = mugsCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/mug-espresso.svg", AltText = "Neon Horizon Espresso Cup", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },

                    // BADGES
                    new Product
                    {
                        Name = "NetRunner Classified Hard Enamel Badge",
                        Slug = "netrunner-classified-enamel-badge",
                        ShortDescription = "Collector grade gunmetal zinc-alloy with recessed black nickel and glowing cyan enamel.",
                        Description = "Official access badge for Tier-4 grid infiltrators. Hand-polished hard enamel badge with dual military-grade clutch backings to secure firmly onto jackets, backpacks, or harness vests.",
                        Price = 15.00m,
                        OriginalPrice = 20.00m,
                        SKU = "BDG-NETRUN-01",
                        StockQuantity = 95,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = badgesCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-22),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/badge-netrunner.svg", AltText = "NetRunner Hard Enamel Badge", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "Quantum Singularity Magnetic Cyber Pin",
                        Slug = "quantum-singularity-magnetic-pin",
                        ShortDescription = "Superconducting NdFeB magnetic clasp badge with holographic prismatic core.",
                        Description = "No fabric damage required. Held in place by an ultra-strong neodymium magnetic backplate. Encapsulates a micro-prismatic diffraction grating that diffracts ambient room light into rainbow spectrums.",
                        Price = 18.00m,
                        OriginalPrice = null,
                        SKU = "BDG-SINGULAR-02",
                        StockQuantity = 50,
                        IsFeatured = true,
                        IsActive = true,
                        CategoryId = badgesCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-14),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/badge-singularity.svg", AltText = "Quantum Singularity Magnetic Pin", IsPrimary = true, DisplayOrder = 0 }
                        }
                    },
                    new Product
                    {
                        Name = "A.I. Neural Syndicate Metallic Patch",
                        Slug = "ai-neural-syndicate-patch",
                        ShortDescription = "Tactical rubberized PVC hook-and-loop patch with glow-in-the-dark phosphor accents.",
                        Description = "Weatherproof, tear-resistant PVC tactical emblem. Features a deeply sculpted 3D neural network topology. Charges up in sunlight and discharges a continuous radioactive lime-cyan glow.",
                        Price = 14.00m,
                        OriginalPrice = 18.00m,
                        SKU = "BDG-NEURAL-03",
                        StockQuantity = 75,
                        IsFeatured = false,
                        IsActive = true,
                        CategoryId = badgesCat.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "/images/products/badge-neural.svg", AltText = "A.I. Neural Syndicate Patch", IsPrimary = true, DisplayOrder = 0 }
                        }
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // 5. Seed a demo initial order so admin dashboard has immediate live analytics!
            if (!await context.Orders.AnyAsync())
            {
                var sampleProduct1 = await context.Products.FirstAsync(p => p.SKU == "TSH-SAMURAI-01");
                var sampleProduct2 = await context.Products.FirstAsync(p => p.SKU == "MUG-MATRIX-01");

                var sampleOrder = new Order
                {
                    OrderNumber = "MHS-2026-9021",
                    UserId = customerUser?.Id,
                    CustomerName = "Karim Zaki",
                    CustomerEmail = "customer@marwanhosny.store",
                    CustomerPhone = "+20 (100) 019-2831",
                    ShippingAddress = "42 Neon Boulevard, Suite 800",
                    City = "Alexandria",
                    State = "ALX",
                    PostalCode = "21500",
                    Country = "Egypt",
                    OrderDate = DateTime.UtcNow.AddHours(-16),
                    Subtotal = 72.00m,
                    ShippingCost = 0.00m,
                    TaxAmount = 5.76m,
                    TotalAmount = 77.76m,
                    Status = OrderStatus.Processing,
                    PaymentStatus = PaymentStatus.Paid,
                    PaymentMethod = "CyberPay Quantum Vault",
                    Notes = "Please leave package in secure delivery drop box."
                };

                sampleOrder.Items.Add(new OrderItem
                {
                    ProductId = sampleProduct1.Id,
                    ProductName = sampleProduct1.Name,
                    ProductImageUrl = sampleProduct1.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = sampleProduct1.Price,
                    Quantity = 1,
                    TotalPrice = sampleProduct1.Price
                });

                sampleOrder.Items.Add(new OrderItem
                {
                    ProductId = sampleProduct2.Id,
                    ProductName = sampleProduct2.Name,
                    ProductImageUrl = sampleProduct2.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = sampleProduct2.Price,
                    Quantity = 1,
                    TotalPrice = sampleProduct2.Price
                });

                await context.Orders.AddAsync(sampleOrder);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database initialization and seed completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
