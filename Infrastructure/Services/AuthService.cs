using Application.DTOs.Auth;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(ApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new Exception("Email already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.IsSeller ? UserRole.Seller : UserRole.Customer
        };

        _context.Users.Add(user);

        // Create cart for user
        var cart = new Cart { User = user };
        _context.Carts.Add(cart);

        // If seller, create shop
        if (dto.IsSeller)
        {
            var shop = new Shop
            {
                Name = $"{dto.FirstName}'s Shop",
                Description = "New shop",
                Owner = user
            };
            _context.Shops.Add(shop);
        }

        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            token,
            DateTime.UtcNow.AddDays(7)
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            token,
            DateTime.UtcNow.AddDays(7)
        );
    }
}