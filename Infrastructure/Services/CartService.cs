using Application.DTOs.Cart;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return MapToCartDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
        }

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null) throw new Exception("Product not found");
        if (product.Stock < dto.Quantity) throw new Exception("Not enough stock");

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            if (existingItem.Quantity > product.Stock)
                throw new Exception("Not enough stock");
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            cart.Items.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return MapToCartDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) throw new Exception("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) throw new Exception("Item not found");

        if (dto.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            if (dto.Quantity > item.Product.Stock)
                throw new Exception("Not enough stock");
            
            item.Quantity = dto.Quantity;
        }

        await _context.SaveChangesAsync();

        return MapToCartDto(cart);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int itemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return false;

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return false;

        cart.Items.Remove(item);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return false;

        cart.Items.Clear();
        await _context.SaveChangesAsync();

        return true;
    }

    private static CartDto MapToCartDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.Id,
            i.Product.Id,
            i.Product.Name,
            i.Product.DiscountPrice ?? i.Product.Price,
            i.Quantity,
            i.Product.MainImageUrl,
            (i.Product.DiscountPrice ?? i.Product.Price) * i.Quantity
        )).ToList();

        var totalAmount = items.Sum(i => i.ItemTotal);

        return new CartDto(cart.Id, items, totalAmount);
    }
}