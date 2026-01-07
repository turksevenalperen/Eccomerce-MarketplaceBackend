namespace Application.DTOs.Orders;

public record CreateOrderDto(
    string ShippingAddress,
    string ShippingCity,
    string ShippingZipCode,
    string ShippingCountry,
    string CustomerPhone,
    string PaymentMethod
);

public record OrderDto(
    int Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    List<OrderItemDto> Items,
    string ShippingAddress,
    string ShippingCity,
    DateTime CreatedAt
);

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? ImageUrl
);

public record OrderListDto(
    int Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    int ItemCount,
    DateTime CreatedAt
);