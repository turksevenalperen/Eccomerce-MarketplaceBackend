using Application.DTOs.Products;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductListDto>> GetAllProductsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.DiscountPrice,
                p.MainImageUrl,
                p.AverageRating,
                p.ReviewCount,
                p.Shop.Name,
                p.Category.Name
            ))
            .ToListAsync();
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        // Increment view count
        product.ViewCount++;
        await _context.SaveChangesAsync();

        return MapToDetailDto(product);
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null) return null;

        product.ViewCount++;
        await _context.SaveChangesAsync();

        return MapToDetailDto(product);
    }

    public async Task<List<ProductListDto>> SearchProductsAsync(string query, int page = 1)
    {
        return await _context.Products
            .Where(p => p.IsActive && 
                   (p.Name.Contains(query) || p.Description.Contains(query)))
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * 20)
            .Take(20)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.DiscountPrice,
                p.MainImageUrl,
                p.AverageRating,
                p.ReviewCount,
                p.Shop.Name,
                p.Category.Name
            ))
            .ToListAsync();
    }

    public async Task<ProductDetailDto> CreateProductAsync(int shopId, CreateProductDto dto)
    {
        var shop = await _context.Shops.FindAsync(shopId);
        if (shop == null) throw new Exception("Shop not found");

        var slug = GenerateSlug(dto.Name);

        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            Stock = dto.Stock,
            ShopId = shopId,
            CategoryId = dto.CategoryId,
            MainImageUrl = dto.MainImageUrl
        };

        if (dto.ImageUrls != null && dto.ImageUrls.Any())
        {
            var images = dto.ImageUrls.Select((url, index) => new ProductImage
            {
                ImageUrl = url,
                DisplayOrder = index,
                Product = product
            }).ToList();

            product.Images = images;
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Shop).LoadAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();

        return MapToDetailDto(product);
    }

    public async Task<ProductDetailDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.DiscountPrice.HasValue) product.DiscountPrice = dto.DiscountPrice;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDetailDto(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductDetailDto MapToDetailDto(Product product)
    {
        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Price,
            product.DiscountPrice,
            product.Stock,
            product.MainImageUrl,
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            product.AverageRating,
            product.ReviewCount,
            product.ViewCount,
            new ShopInfoDto(product.Shop.Id, product.Shop.Name, product.Shop.LogoUrl),
            new CategoryInfoDto(product.Category.Id, product.Category.Name, product.Category.Slug),
            product.CreatedAt
        );
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c") + "-" + Guid.NewGuid().ToString()[..8];
    }
}