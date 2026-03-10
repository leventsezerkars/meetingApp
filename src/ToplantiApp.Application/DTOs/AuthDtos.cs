namespace ToplantiApp.Application.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password);

public record LoginDto(string Email, string Password);

public record AuthResponseDto
{
    public string Token { get; init; } = default!;
    public UserDto User { get; init; } = default!;
}

public record UserDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string? ProfileImagePath { get; init; }
}
