using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.User;

// <summary>Add new users into the system</summary>
public class AddUserDto
{
     [Required]
     [StringLength(60, MinimumLength = 3)]
     public string FirstName { get; set; } = string.Empty;
     
     [Required]
     [StringLength(60, MinimumLength = 3)]
     public string LastName { get; set; } = string.Empty;
     
     [Required]
     [EmailAddress]
     public string Email { get; set; } = string.Empty;
     
     [Required]
     [MinLength(8)]
     public string Password { get; set; } = string.Empty;
     
     [Required]
     [EnumDataType(typeof(UserRole))]
     public UserRole Role { get; set; }
}