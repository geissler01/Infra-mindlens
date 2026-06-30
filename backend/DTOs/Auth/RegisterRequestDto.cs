using System.ComponentModel.DataAnnotations;

namespace MindLens.Api.DTOs.Auth;

// <summary>Register Request</summary>
public class RegisterRequestDto
{
    // <summary>First Name of the user</summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    // <summary>Last Name of the user</summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    
    // <summary>Email of user</summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    // <summary>User password</summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}