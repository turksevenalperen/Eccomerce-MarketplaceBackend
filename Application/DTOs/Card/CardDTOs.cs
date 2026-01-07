namespace Application.DTOs.Cart;

public record CartDto(
    int Id,
    List<CartItemDto> Items,
    decimal TotalAmount
);

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    decimal ItemTotal
);

public record AddToCartDto(
    int ProductId,
    int Quantity = 1
);

public record UpdateCartItemDto(
    int Quantity
);