using System.ComponentModel.DataAnnotations;
using StorageCompany.Core.Enums;

namespace StorageCompany.Core.Validators;

public static class PasswordPolicyValidator
{
    private const int NormalUserMinimumLength = 12;
    private const int AdminMinimumLength = 16;
    private const string CommonPasswordBlocklistFileName = "common-passwords.txt";

    private static readonly Lazy<HashSet<string>> CommonPasswordBlocklist = new(
        LoadCommonPasswordBlocklist);

    public static void Validate(string password, string role)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ValidationException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Password cannot contain only whitespace.");
        }

        var minimumLength = IsAdminRole(role)
            ? AdminMinimumLength
            : NormalUserMinimumLength;

        if (password.Length < minimumLength)
        {
            throw new ValidationException(
                $"Password must be at least {minimumLength} characters long.");
        }

        if (!ContainsSpecialCharacter(password))
        {
            throw new ValidationException(
                "Password must contain at least one special character.");
        }

        if (IsCommonPassword(password))
        {
            throw new ValidationException(
                "Password is too common. Choose a stronger password.");
        }
    }

    private static bool IsAdminRole(string role)
    {
        return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, Constants.AdminRole, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsSpecialCharacter(string password)
    {
        return password.Any(character =>
            !char.IsLetterOrDigit(character) &&
            !char.IsWhiteSpace(character));
    }

    private static bool IsCommonPassword(string password)
    {
        return CommonPasswordBlocklist.Value.Contains(password);
    }

    private static HashSet<string> LoadCommonPasswordBlocklist()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            CommonPasswordBlocklistFileName);

        if (!File.Exists(filePath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return File.ReadLines(filePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.StartsWith('#'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}