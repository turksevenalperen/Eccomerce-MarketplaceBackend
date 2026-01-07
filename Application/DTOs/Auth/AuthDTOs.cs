namespace Application.DTOs.Auth;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsSeller = false
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token,
    DateTime ExpiresAt
);