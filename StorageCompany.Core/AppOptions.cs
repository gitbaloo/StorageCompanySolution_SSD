using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core;

public sealed class AppOptions
{
    [Required] public string JwtSecret { get; set; } = string.Empty;

    [Required] public string EncryptionKey { get; set; } = string.Empty;
}