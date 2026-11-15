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
                    Country = "Egypt",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure password matches standard demo password
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, resetToken, "Admin@123456");
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
                    Country = "Egypt",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(customerUser, "Customer@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
            else
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(customerUser);
                await userManager.ResetPasswordAsync(customerUser, resetToken, "Customer@123456");
            }

            // 3. Seed / Ensure All 6 Categories
            var definedCategories = new List<Category>
            {
                new Category
                {
                    Name = "T-Shirts",
                    Slug = "t-shirts",
                    Description = "Heavyweight 280GSM French Terry cotton tees with luxury drop-shoulder tailoring and cyber detailing.",
                    IconClass = "bi-tag-fill",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Category
                {
                    Name = "Hoodies & Jackets",
                    Slug = "hoodies",
                    Description = "Ultra-heavyweight 420GSM cotton pullovers and tactical zip-up outerwear with technical utility accents.",
                    IconClass = "bi-layers-fill",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Category
                {
                    Name = "Mugs & Drinkware",
                    Slug = "mugs",
                    Description = "Matte black ceramic reactors, insulated double-wall stainless steel tumblers, and espresso ware.",
                    IconClass = "bi-cup-hot-fill",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new Category
                {
                    Name = "Desk Mats",
                    Slug = "desk-mats",
                    Description = "Extended XXL micro-woven gaming pads with non-slip natural rubber bases and reinforced neon borders.",
                    IconClass = "bi-aspect-ratio-fill",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new Category
                {
                    Name = "Headwear & Caps",
                    Slug = "headwear",
                    Description = "Structured flat-brim snapbacks, warm ribbed wool beanies, and breathable mesh truckers with 3D embroidery.",
                    IconClass = "bi-gem",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new Category
                {
                    Name = "Badges & Pins",
                    Slug = "badges",
                    Description = "Laser-cut metallic collector emblems, hard enamel lapel pins with polished nickel, and magnetic clasps.",
                    IconClass = "bi-shield-shaded",
                    DisplayOrder = 6,
                    IsActive = true
                }
            };

            foreach (var cat in definedCategories)
            {
                var existingCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == cat.Slug);
                if (existingCat == null)
                {
                    await context.Categories.AddAsync(cat);
                }
                else
                {
                    existingCat.Name = cat.Name;
                    existingCat.Description = cat.Description;
                    existingCat.IconClass = cat.IconClass;
                    existingCat.DisplayOrder = cat.DisplayOrder;
                    existingCat.IsActive = true;
                }
            }
            await context.SaveChangesAsync();

            // Reload categories map
            var categoriesMap = await context.Categories.ToDictionaryAsync(c => c.Slug);

            // 4. Seed / Ensure All 24 Realistic Products (4 per category)
            var definedProducts = new List<(Product prod, string imageUrl)>
            {
                // ==================== 1. T-SHIRTS (4 Products) ====================
                (new Product
                {
                    Name = "Neon Samurai Heavyweight Oversized Tee",
                    Slug = "neon-samurai-oversized-cyber-tee",
                    ShortDescription = "Heavyweight 280GSM organic cotton featuring dual-tone neon purple and cyan silkscreen graphics.",
                    Description = "Forged for the digital era. The Neon Samurai Tee combines luxury heavyweight French terry cotton with high-frequency reflective screen printing that illuminates under ultraviolet and ambient street lamps. Relaxed dropped-shoulder cut with reinforced double-stitched collar seams.",
                    Price = 48.00m,
                    OriginalPrice = 60.00m,
                    SKU = "TSH-SAMURAI-01",
                    StockQuantity = 45,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["t-shirts"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                }, "/images/products/tshirt-samurai.jpg"),

                (new Product
                {
                    Name = "Quantum Core French Terry Streetwear Tee",
                    Slug = "quantum-core-french-terry-tee",
                    ShortDescription = "Dark charcoal organic cotton streetwear tee with soft natural drape and subtle cyan chest glyph.",
                    Description = "High-end minimalist streetwear. Made from 100% ring-spun combed cotton with pre-shrunk treatment. Features a discreet high-density rubberized chest imprint that reflects low ambient light.",
                    Price = 42.00m,
                    OriginalPrice = 52.00m,
                    SKU = "TSH-QUANTUM-02",
                    StockQuantity = 32,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["t-shirts"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }, "/images/products/tshirt-quantum.jpg"),

                (new Product
                {
                    Name = "Zero-Day Minimalist Black Cotton Tee",
                    Slug = "zero-day-minimalist-black-tee",
                    ShortDescription = "Clean, technical typography on jet-black acid-washed combed cotton with clean rib collar.",
                    Description = "Subtle on the surface, devastating in execution. The Zero-Day shirt features discreet hexadecimal kernel code along the back spine and a glowing holographic chest glyph.",
                    Price = 38.00m,
                    OriginalPrice = 45.00m,
                    SKU = "TSH-ZERODAY-03",
                    StockQuantity = 25,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["t-shirts"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }, "/images/products/tshirt-zeroday.jpg"),

                (new Product
                {
                    Name = "HyperDrive Acid-Washed Cyber Tee",
                    Slug = "hyperdrive-acid-washed-cyber-tee",
                    ShortDescription = "Vintage mineral acid-washed black streetwear tee with holographic chrome foil detailing.",
                    Description = "Cruising down endless neon highways. Custom acid-washed mineral treatment gives each shirt a unique textured gray-black patina with premium chrome foil stamp accents.",
                    Price = 45.00m,
                    OriginalPrice = 55.00m,
                    SKU = "TSH-HYPER-04",
                    StockQuantity = 20,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["t-shirts"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                }, "/images/products/tshirt-hyperdrive.jpg"),

                // ==================== 2. HOODIES & JACKETS (4 Products) ====================
                (new Product
                {
                    Name = "Neon Glitch Heavyweight Oversized Hoodie",
                    Slug = "neon-glitch-heavyweight-hoodie",
                    ShortDescription = "Heavyweight 420GSM French Terry cotton pullover with double-layer hood and kangaroo pocket.",
                    Description = "Engineered for maximum cold-weather comfort and street presence. Made from ultra-dense 420GSM brushed French terry cotton with custom dyed drawstrings, metal-capped aglets, and a spacious structured hood.",
                    Price = 65.00m,
                    OriginalPrice = 80.00m,
                    SKU = "HOD-GLITCH-01",
                    StockQuantity = 35,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["hoodies"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                }, "/images/products/hoodie-glitch.jpg"),

                (new Product
                {
                    Name = "Matrix Core Tactical Zip-Up Jacket",
                    Slug = "matrix-core-tactical-jacket",
                    ShortDescription = "Weather-resistant tactical jacket with waterproof technical zippers and utility arm pockets.",
                    Description = "Tactical utility meets modern cyberpunk fashion. Built with a DWR (Durable Water Repellent) matte shell, dual chest utility pouches, and discreet neon green matrix piping along the sleeves.",
                    Price = 78.00m,
                    OriginalPrice = 95.00m,
                    SKU = "HOD-MATRIX-02",
                    StockQuantity = 22,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["hoodies"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                }, "/images/products/hoodie-matrix.jpg"),

                (new Product
                {
                    Name = "Zero-Gravity Charcoal Stealth Pullover",
                    Slug = "zero-gravity-charcoal-pullover",
                    ShortDescription = "Clean minimalist charcoal pullover hoodie with concealed thumbholes and dense fleece lining.",
                    Description = "Sleek, silent, and refined. Features seamless side paneling, ribbed hem cuffs with hidden thumbhole loops, and a high-density embroidered stealth monogram on the left wrist.",
                    Price = 58.00m,
                    OriginalPrice = 70.00m,
                    SKU = "HOD-STEALTH-03",
                    StockQuantity = 28,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["hoodies"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                }, "/images/products/hoodie-stealth.jpg"),

                (new Product
                {
                    Name = "Cyberpunk Obsidian Fleece Hoodie",
                    Slug = "cyberpunk-obsidian-fleece-hoodie",
                    ShortDescription = "Deep obsidian black fleece-lined hoodie with neon violet reflective side accents.",
                    Description = "Warm, ultra-soft fleece interior with a sleek water-repellent exterior. Designed with an oversized drop-shoulder fit that layers effortlessly over t-shirts and flannels.",
                    Price = 62.00m,
                    OriginalPrice = 75.00m,
                    SKU = "HOD-OBSIDIAN-04",
                    StockQuantity = 30,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["hoodies"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                }, "/images/products/hoodie-cyberpunk.jpg"),

                // ==================== 3. MUGS & DRINKWARE (4 Products) ====================
                (new Product
                {
                    Name = "Matrix Thermal Reactor Ceramic Mug",
                    Slug = "matrix-thermal-reactor-ceramic-mug",
                    ShortDescription = "Heat-activated color shifting matte ceramic mug with glowing neon green matrix code.",
                    Description = "Wake up your neuro-synapses. Engineered with thermochromic nano-crystal glaze, this matte black ceramic mug transforms in real-time when hot coffee or tea is introduced, unlocking a brilliant electric cyan matrix diagram.",
                    Price = 24.00m,
                    OriginalPrice = 30.00m,
                    SKU = "MUG-MATRIX-01",
                    StockQuantity = 60,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["mugs"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                }, "/images/products/mug-matrix.jpg"),

                (new Product
                {
                    Name = "Caffeine Overclocked Stainless Tumbler",
                    Slug = "caffeine-overclocked-stainless-tumbler",
                    ShortDescription = "Double-wall vacuum insulated 450ml matte obsidian stainless steel tumbler with spill-proof lid.",
                    Description = "Keeps beverages steaming hot for 8 hours or ice-cold for 18 hours. Grade 304 food-grade stainless steel with magnetic slider lid and laser-etched circuitry motif.",
                    Price = 32.00m,
                    OriginalPrice = 40.00m,
                    SKU = "MUG-STEEL-02",
                    StockQuantity = 30,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["mugs"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                }, "/images/products/mug-steel.jpg"),

                (new Product
                {
                    Name = "Neon Horizon Matte Black Espresso Cup",
                    Slug = "neon-horizon-matte-black-espresso-cup",
                    ShortDescription = "Compact 120ml double-glazed ceramic cup with iridescent electroplated titanium rim.",
                    Description = "Crafted for intense, high-octane shots of espresso. Features an ergonomic thumb groove and an iridescent violet-to-cyan gradient electroplated titanium glaze.",
                    Price = 18.00m,
                    OriginalPrice = 22.00m,
                    SKU = "MUG-ESPRESSO-03",
                    StockQuantity = 40,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["mugs"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }, "/images/products/mug-espresso.jpg"),

                (new Product
                {
                    Name = "Glitch Core Double-Wall Ceramic Mug",
                    Slug = "glitch-core-double-wall-ceramic-mug",
                    ShortDescription = "350ml heavy stoneware coffee mug with comfort C-handle and scratch-proof matte glaze.",
                    Description = "Sturdy, substantial, and heat-retaining. Kiln-fired at 1280°C with chip-resistant rim and printed with dishwasher-safe ceramic pigments.",
                    Price = 22.00m,
                    OriginalPrice = 28.00m,
                    SKU = "MUG-GLITCH-04",
                    StockQuantity = 50,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["mugs"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }, "/images/products/mug-glitch.jpg"),

                // ==================== 4. DESK MATS (4 Products) ====================
                (new Product
                {
                    Name = "Cyber Circuit XXL Extended Desk Mat",
                    Slug = "cyber-circuit-xxl-extended-desk-mat",
                    ShortDescription = "900x400x4mm extended mouse pad with ultra-smooth micro-woven cloth and stitched edges.",
                    Description = "Covers your entire desk setup with plenty of room for keyboard, mouse, and phone. High-density micro-woven cloth surface delivers pinpoint tracking accuracy with anti-fray reinforced lock stitching.",
                    Price = 28.00m,
                    OriginalPrice = 35.00m,
                    SKU = "PAD-CIRCUIT-01",
                    StockQuantity = 50,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["desk-mats"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-16)
                }, "/images/products/pad-circuit.svg"),

                (new Product
                {
                    Name = "Synthwave Neon Horizon Gaming Mat",
                    Slug = "synthwave-neon-horizon-gaming-mat",
                    ShortDescription = "Retro-futuristic outrun sun and perspective grid desk pad with spill-resistant coating.",
                    Description = "Transform your desk into an 80s neon paradise. Features a water-resistant nano-coating that repels accidental liquid spills, wiping clean with a simple cloth.",
                    Price = 26.00m,
                    OriginalPrice = 32.00m,
                    SKU = "PAD-HORIZON-02",
                    StockQuantity = 45,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["desk-mats"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-11)
                }, "/images/products/pad-horizon.svg"),

                (new Product
                {
                    Name = "Matrix Terminal Code Precision Pad",
                    Slug = "matrix-terminal-code-precision-pad",
                    ShortDescription = "Developer terminal matrix font graphic on high-speed low-friction textile weave.",
                    Description = "Designed specifically for coders, developers, and esports players. Non-slip textured natural rubber base grips firmly to wood, glass, and laminate surfaces.",
                    Price = 25.00m,
                    OriginalPrice = 30.00m,
                    SKU = "PAD-MATRIX-03",
                    StockQuantity = 40,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["desk-mats"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                }, "/images/products/pad-matrix.svg"),

                (new Product
                {
                    Name = "Minimalist Stealth Obsidian Desk Mat",
                    Slug = "minimalist-stealth-obsidian-desk-mat",
                    ShortDescription = "Monochrome stealth black desk pad with subtle cyan perimeter stitching and embossed logo.",
                    Description = "Understated luxury for executive desk setups. Supple synthetic leather blended with micro-fiber cloth for effortless mouse glide and wrist comfort.",
                    Price = 24.00m,
                    OriginalPrice = 28.00m,
                    SKU = "PAD-STEALTH-04",
                    StockQuantity = 35,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["desk-mats"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                }, "/images/products/pad-minimal.svg"),

                // ==================== 5. HEADWEAR & CAPS (4 Products) ====================
                (new Product
                {
                    Name = "Quantum 3D Embroidered Snapback Cap",
                    Slug = "quantum-3d-embroidered-snapback-cap",
                    ShortDescription = "Structured 6-panel acrylic-wool blend snapback cap with raised puff 3D embroidered crest.",
                    Description = "Premium high-profile crown with green under-visor, moisture-wicking internal sweatband, and adjustable snap closure for a universal comfortable fit.",
                    Price = 24.00m,
                    OriginalPrice = 30.00m,
                    SKU = "CAP-QUANTUM-01",
                    StockQuantity = 35,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["headwear"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }, "/images/products/cap-quantum.jpg"),

                (new Product
                {
                    Name = "Binary Matrix Ribbed Knit Beanie",
                    Slug = "binary-matrix-ribbed-knit-beanie",
                    ShortDescription = "Warm 100% hypoallergenic acrylic double-layer knit beanie with rubberized cyber patch.",
                    Description = "Snug, flexible fit that keeps you warm through cold city nights. Foldable cuff allows customized styling with a durable silicone logo patch stitched on front.",
                    Price = 18.00m,
                    OriginalPrice = 24.00m,
                    SKU = "CAP-BEANIE-02",
                    StockQuantity = 40,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["headwear"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }, "/images/products/cap-beanie.jpg"),

                (new Product
                {
                    Name = "Reflective Breathable Trucker Cap",
                    Slug = "reflective-breathable-trucker-cap",
                    ShortDescription = "Curved bill trucker cap with breathable mesh back and light-reflective front badge.",
                    Description = "Keeps you cool during long summer days. Lightweight foam front with reflective vinyl heat-pressed emblem and classic snapback adjustment.",
                    Price = 22.00m,
                    OriginalPrice = 28.00m,
                    SKU = "CAP-TRUCKER-03",
                    StockQuantity = 30,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["headwear"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                }, "/images/products/cap-trucker.jpg"),

                (new Product
                {
                    Name = "Tactical Low-Profile Stealth Cap",
                    Slug = "tactical-low-profile-stealth-cap",
                    ShortDescription = "Unstructured dad-hat profile in washed cotton twill with antique brass buckle strap.",
                    Description = "Effortless casual style. Soft washed cotton twill with pre-curved visor and an adjustable fabric strap with metal slide buckle.",
                    Price = 20.00m,
                    OriginalPrice = 25.00m,
                    SKU = "CAP-STEALTH-04",
                    StockQuantity = 25,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["headwear"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }, "/images/products/cap-stealth.jpg"),

                // ==================== 6. BADGES & PINS (4 Products) ====================
                (new Product
                {
                    Name = "Luxury Silver & Black Enamel Pin",
                    Slug = "luxury-silver-black-enamel-pin",
                    ShortDescription = "Jewelry-grade hard enamel lapel pin with polished nickel plating and double rubber clutch.",
                    Description = "Crafted like fine jewelry. Hand-polished hard enamel finish that is completely smooth to the touch, framed in mirror-polished silver-nickel plating.",
                    Price = 16.00m,
                    OriginalPrice = 20.00m,
                    SKU = "BDG-NEURAL-01",
                    StockQuantity = 80,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["badges"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-22)
                }, "/images/products/badge-neural.jpg"),

                (new Product
                {
                    Name = "NetRunner Hexagonal Metallic Badge",
                    Slug = "netrunner-hexagonal-metallic-badge",
                    ShortDescription = "Collector grade gunmetal zinc-alloy badge with recessed circuit traces and dual pins.",
                    Description = "Official access badge for network engineers. Solid zinc alloy with antique gunmetal plating and military-grade butterfly clasp backings.",
                    Price = 18.00m,
                    OriginalPrice = 22.00m,
                    SKU = "BDG-NETRUN-02",
                    StockQuantity = 60,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["badges"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-17)
                }, "/images/products/badge-netrunner.jpg"),

                (new Product
                {
                    Name = "Quantum Singularity Magnetic Emblem",
                    Slug = "quantum-singularity-magnetic-emblem",
                    ShortDescription = "Superconducting NdFeB magnetic clasp badge with holographic prismatic core.",
                    Description = "No fabric holes required. Held in place by an ultra-strong neodymium magnetic backplate that safely snaps through jackets, suits, and bag straps.",
                    Price = 20.00m,
                    OriginalPrice = 25.00m,
                    SKU = "BDG-SINGULAR-03",
                    StockQuantity = 50,
                    IsFeatured = true,
                    IsActive = true,
                    CategoryId = categoriesMap["badges"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                }, "/images/products/badge-singularity.jpg"),

                (new Product
                {
                    Name = "Cyber Syndicate Hard Enamel Shield",
                    Slug = "cyber-syndicate-hard-enamel-shield",
                    ShortDescription = "Tactical shield emblem pin with recessed matte black and neon cyan epoxy filling.",
                    Description = "Heavy, tactile metal pin designed for jackets, laptop bags, and collector display boards. Includes a luxury velvet backing card.",
                    Price = 15.00m,
                    OriginalPrice = 18.00m,
                    SKU = "BDG-SHIELD-04",
                    StockQuantity = 70,
                    IsFeatured = false,
                    IsActive = true,
                    CategoryId = categoriesMap["badges"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                }, "/images/products/badge-shield.jpg")
            };

            foreach (var (prod, imgUrl) in definedProducts)
            {
                var existingProd = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.SKU == prod.SKU || p.Slug == prod.Slug);
                if (existingProd == null)
                {
                    prod.Images = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl = imgUrl,
                            AltText = prod.Name,
                            IsPrimary = true,
                            DisplayOrder = 0
                        }
                    };
                    await context.Products.AddAsync(prod);
                }
                else
                {
                    existingProd.Name = prod.Name;
                    existingProd.Slug = prod.Slug;
                    existingProd.SKU = prod.SKU;
                    existingProd.ShortDescription = prod.ShortDescription;
                    existingProd.Description = prod.Description;
                    existingProd.Price = prod.Price;
                    existingProd.OriginalPrice = prod.OriginalPrice;
                    existingProd.StockQuantity = prod.StockQuantity;
                    existingProd.IsFeatured = prod.IsFeatured;
                    existingProd.IsActive = true;
                    existingProd.CategoryId = prod.CategoryId;

                    // Update primary image to realistic photo
                    var primaryImage = existingProd.Images.FirstOrDefault(i => i.IsPrimary);
                    if (primaryImage != null)
                    {
                        primaryImage.ImageUrl = imgUrl;
                        primaryImage.AltText = prod.Name;
                    }
                    else
                    {
                        existingProd.Images.Add(new ProductImage
                        {
                            ImageUrl = imgUrl,
                            AltText = prod.Name,
                            IsPrimary = true,
                            DisplayOrder = 0
                        });
                    }
                }
            }
            await context.SaveChangesAsync();

            // 5. Seed a demo initial order if none exist
            if (!await context.Orders.AnyAsync())
            {
                var sampleProduct1 = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.SKU == "TSH-SAMURAI-01");
                var sampleProduct2 = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.SKU == "HOD-GLITCH-01");

                if (sampleProduct1 != null && sampleProduct2 != null)
                {
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
                        Subtotal = sampleProduct1.Price + sampleProduct2.Price,
                        ShippingCost = 0.00m,
                        TaxAmount = Math.Round((sampleProduct1.Price + sampleProduct2.Price) * 0.08m, 2),
                        TotalAmount = sampleProduct1.Price + sampleProduct2.Price + Math.Round((sampleProduct1.Price + sampleProduct2.Price) * 0.08m, 2),
                        Status = OrderStatus.Processing,
                        PaymentStatus = PaymentStatus.Paid,
                        PaymentMethod = "Credit Card",
                        Notes = "Please deliver after 5 PM."
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
            }

            logger.LogInformation("Database initialization and seed completed successfully with 6 categories and 24 realistic products!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
