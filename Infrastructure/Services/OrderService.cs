using Application.DTOs.Orders;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p.Shop)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            throw new Exception("Cart is empty");

        // Check stock
        foreach (var item in cart.Items)
        {
            if (item.Product.Stock < item.Quantity)
                throw new Exception($"Not enough stock for {item.Product.Name}");
        }

        var orderNumber = GenerateOrderNumber();
        var totalAmount = cart.Items.Sum(i => 
            (i.Product.DiscountPrice ?? i.Product.Price) * i.Quantity);

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            TotalAmount = totalAmount,
            CommissionAmount = totalAmount * 0.1m, // 10% commission
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingZipCode = dto.ShippingZipCode,
            ShippingCountry = dto.ShippingCountry,
            CustomerPhone = dto.CustomerPhone,
            Status = OrderStatus.Pending
        };

        var orderItems = cart.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.Product.DiscountPrice ?? i.Product.Price,
            TotalPrice = (i.Product.DiscountPrice ?? i.Product.Price) * i.Quantity,
            Order = order
        }).ToList();

        order.Items = orderItems;

        // Create payment
        var payment = new Payment
        {
            Amount = totalAmount,
            Method = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow,
            Order = order
        };

        // Reduce stock
        foreach (var item in cart.Items)
        {
            item.Product.Stock -= item.Quantity;
        }

        _context.Orders.Add(order);
        _context.Payments.Add(payment);
        cart.Items.Clear();

        await _context.SaveChangesAsync();

        await _context.Entry(order)
            .Collection(o => o.Items)
            .Query()
            .Include(oi => oi.Product)
            .LoadAsync();

        return MapToOrderDto(order);
    }

    public async Task<List<OrderListDto>> GetUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto(
                o.Id,
                o.OrderNumber,
                o.TotalAmount,
                o.Status.ToString(),
                o.Items.Count,
                o.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : MapToOrderDto(order);
    }

    public async Task<bool> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return false;
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            return false;

        order.Status = OrderStatus.Cancelled;

        // Restore stock
        foreach (var item in order.Items)
        {
            item.Product.Stock += item.Quantity;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        var items = order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product.Name,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice,
            i.Product.MainImageUrl
        )).ToList();

        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            order.Status.ToString(),
            items,
            order.ShippingAddress,
            order.ShippingCity,
            order.CreatedAt
        );
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}