namespace Application.DTOs.Products;

public record ProductListDto(
    int Id,
    string Name,
    string Slug,
    decimal Price,
    decimal? DiscountPrice,
    string? MainImageUrl,
    decimal AverageRating,
    int ReviewCount,
    string ShopName,
    string CategoryName
);

public record ProductDetailDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    string? MainImageUrl,
    List<string> Images,
    decimal AverageRating,
    int ReviewCount,
    int ViewCount,
    ShopInfoDto Shop,
    CategoryInfoDto Category,
    DateTime CreatedAt
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    int CategoryId,
    string? MainImageUrl,
    List<string>? ImageUrls
);

public record UpdateProductDto(
    string? Name,
    string? Description,
    decimal? Price,
    decimal? DiscountPrice,
    int? Stock,
    int? CategoryId,
    bool? IsActive
);

public record ShopInfoDto(
    int Id,
    string Name,
    string? LogoUrl
);

public record CategoryInfoDto(
    int Id,
    string Name,
    string Slug
);