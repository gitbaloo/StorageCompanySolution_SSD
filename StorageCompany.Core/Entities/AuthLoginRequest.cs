using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core.Entities;

public class AuthLoginRequest
{

    [MinLength(3)] [Required] public string Email { get; set; } = null!;
    
    [Required] public string Password { get; set; } = null!;
}