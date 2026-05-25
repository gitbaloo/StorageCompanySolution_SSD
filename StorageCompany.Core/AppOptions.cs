using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core;

public class AppOptions
{
    [Required]
    [MinLength(32)]
    public string JwtSecret { get; set; } = string.Empty;
  
    [Required] public string EncryptionKey { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string RequestSigningSecret { get; set; } = string.Empty;
}