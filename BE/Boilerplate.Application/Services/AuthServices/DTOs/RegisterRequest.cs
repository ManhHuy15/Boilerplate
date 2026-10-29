using System.ComponentModel.DataAnnotations;

namespace Boilerplate.Application.Services.AuthServices.DTOs;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
