using System.ComponentModel.DataAnnotations;

namespace MindLens.Api.DTOs.Auth;

// <summary>Login Request</summary>
public class LoginRequestDto
{
    // <summary>User Email</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    // <summary>User Password</summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    public string? TenantId { get; set; }
}