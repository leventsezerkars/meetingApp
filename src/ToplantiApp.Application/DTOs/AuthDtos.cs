namespace ToplantiApp.Application.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, UserDto User);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? ProfileImagePath);
