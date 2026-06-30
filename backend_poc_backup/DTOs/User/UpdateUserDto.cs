using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.User;

// <summary>Update an user </summary>
public class UpdateUserDto
{
    // <summary>User First Name </summary>
    [StringLength(60, MinimumLength = 3)]
    public string? FirstName { get; set; }
    
    // <summary>User Last Name </summary>
    [StringLength(60, MinimumLength = 3)]
    public string? LastName { get; set; }
    
    // <summary> User email address</summary>
    [EmailAddress]
    public string? Email { get; set; }
    
    // <summary>User role</summary>
    [EnumDataType(typeof(UserRole))]
    public UserRole? Role { get; set; }
}