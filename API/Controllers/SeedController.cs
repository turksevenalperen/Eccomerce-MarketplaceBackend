using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core.Entities;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SeedData([FromQuery] bool force = false)
    {
        // Eğer force true ise, ilişkili tüm tabloları temizle
        if (force)
        {
            _context.CartItems.RemoveRange(_context.CartItems);
            _context.Carts.RemoveRange(_context.Carts);
            _context.Products.RemoveRange(_context.Products);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Shops.RemoveRange(_context.Shops);
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();
        }
        else if (await _context.Products.AnyAsync())
        {
            return BadRequest("Products already exist in database!");
        }

        // Create seller user
        var seller = new User
        {
            Email = "seller@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "John",
            LastName = "Seller",
            PhoneNumber = "+1234567890",
            Role = UserRole.Seller,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(seller);
        await _context.SaveChangesAsync();

        // Create shop
        var shop = new Shop
        {
            Name = "Tech Store",
            Description = "Best electronics and gadgets",
            CommissionRate = 10.00m,
            IsActive = true,
            OwnerId = seller.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        // Create categories
        var electronics = new Category
        {
            Name = "Electronics",
            Slug = "electronics",
            Description = "Electronic devices and accessories",
            CreatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(electronics);
        await _context.SaveChangesAsync();

        var computers = new Category
        {
            Name = "Computers",
            Slug = "computers",
            Description = "Laptops, desktops and accessories",
            ParentCategoryId = electronics.Id,
            CreatedAt = DateTime.UtcNow
        };
        var smartphones = new Category
        {
            Name = "Smartphones",
            Slug = "smartphones",
            Description = "Mobile phones and accessories",
            ParentCategoryId = electronics.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Categories.AddRange(computers, smartphones);
        await _context.SaveChangesAsync();

        // Create products
        var products = new List<Product>
        {
            new Product
            {
                Name = "iPhone 15 Pro Max",
                Slug = "iphone-15-pro-max",
                Description = "Latest iPhone with A17 Pro chip, titanium design, and advanced camera system. Features a 6.7-inch Super Retina XDR display.",
                Price = 1199.99m,
                DiscountPrice = 1099.99m,
                Stock = 50,
                MainImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=500",
                IsActive = true,
                ShopId = shop.Id,
                CategoryId = smartphones.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "MacBook Pro 16\"",
                Slug = "macbook-pro-16",
                Description = "Powerful MacBook Pro with M3 Max chip, 16GB RAM, 512GB SSD. Perfect for professionals and creators.",
                Price = 2499.99m,
                Stock = 30,
                MainImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500",
                IsActive = true,
                ShopId = shop.Id,
                CategoryId = computers.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Samsung Galaxy S24 Ultra",
                Slug = "samsung-galaxy-s24-ultra",
                Description = "Premium Android smartphone with 200MP camera, S Pen, and stunning AMOLED display. 12GB RAM, 256GB storage.",
                Price = 1199.99m,
                DiscountPrice = 1049.99m,
                Stock = 40,
                MainImageUrl = "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=500",
                IsActive = true,
                ShopId = shop.Id,
                CategoryId = smartphones.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Dell XPS 15",
                Slug = "dell-xps-15",
                Description = "Sleek and powerful laptop with Intel i7, 16GB RAM, 1TB SSD. 15.6\" 4K OLED display for stunning visuals.",
                Price = 1799.99m,
                DiscountPrice = 1699.99m,
                Stock = 25,
                MainImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=500",
                IsActive = true,
                ShopId = shop.Id,
                CategoryId = computers.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "AirPods Pro 2nd Gen",
                Slug = "airpods-pro-2",
                Description = "Apple AirPods Pro with active noise cancellation, adaptive transparency, and spatial audio. USB-C charging case.",
                Price = 249.99m,
                DiscountPrice = 229.99m,
                Stock = 100,
                MainImageUrl = "https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=500",
                IsActive = true,
                ShopId = shop.Id,
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Seed data created successfully!", productCount = products.Count });
    }
}