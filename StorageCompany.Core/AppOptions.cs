using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core;

public class AppOptions
{
    [Required]
    [MinLength(32)]
    public string JwtSecret { get; set; } = string.Empty;
  
    [Required]
    [MinLength(44)] // 44 minimum is need due to conversion between base64 can cause a 32-byte array to be represented with up to 44 characters.
    public string EncryptionKey { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string RequestSigningSecret { get; set; } = string.Empty;
}