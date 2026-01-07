using Application.DTOs.Auth;
using Application.DTOs.Products;
using Application.DTOs.Cart;
using Application.DTOs.Orders;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface IProductService
{
    Task<List<ProductListDto>> GetAllProductsAsync(int page = 1, int pageSize = 20);
    Task<ProductDetailDto?> GetProductByIdAsync(int id);
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug);
    Task<List<ProductListDto>> SearchProductsAsync(string query, int page = 1);
    Task<ProductDetailDto> CreateProductAsync(int shopId, CreateProductDto dto);
    Task<ProductDetailDto?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemDto dto);
    Task<bool> RemoveFromCartAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
}

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<List<OrderListDto>> GetUserOrdersAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<bool> CancelOrderAsync(int userId, int orderId);
}

public interface ITokenService
{
    string GenerateToken(int userId, string email, string role);
}