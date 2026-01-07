namespace Core.Entities;

public class Shop : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public decimal CommissionRate { get; set; } = 10; // %10 default
    public bool IsActive { get; set; } = true;
    
    // Foreign Keys
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}