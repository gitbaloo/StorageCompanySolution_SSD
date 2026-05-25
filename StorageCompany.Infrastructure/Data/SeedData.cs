using System.Security.Cryptography;
using System.Text;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Infrastructure.Data;

public static class SeedData
{
    public static void Initialize(IEncryptionService encryptionService)
    {
        MockDatabase.Users.Clear();
        MockDatabase.Users.AddRange([
            new User
            {
                Id = MockDatabase.Ids.UserAnna,
                FirstName = encryptionService.Encrypt("Anna"),
                LastName = encryptionService.Encrypt("Jensen"),
                Email = "anna@example.com",
                PhoneNumber = encryptionService.Encrypt("+45 12 34 56 78"),
                PasswordSalt = string.Empty,
                PasswordHash = HashPassword("Customer123!"),
                Role = Constants.CustomerRole,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = MockDatabase.Ids.UserPeter,
                FirstName = encryptionService.Encrypt("Peter"),
                LastName = encryptionService.Encrypt("Nielsen"),
                Email = "peter@example.com",
                PhoneNumber = encryptionService.Encrypt("+45 87 65 43 21"),
                PasswordSalt = string.Empty,
                PasswordHash = HashPassword("Admin123!"),
                Role = Constants.AdminRole,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-12)
            }
        ]);

        MockDatabase.Invoices.Clear();
        MockDatabase.Invoices.Add(new Invoice
        {
            Id = MockDatabase.Ids.ExistingInvoice,
            RentalId = MockDatabase.Ids.ExistingRental,
            UserId = MockDatabase.Ids.UserAnna,
            InvoiceNumber = encryptionService.Encrypt("INV-DEMO-001"),
            Amount = 699m,
            DueDateUtc = DateTime.UtcNow.Date.AddDays(15),
            Status = InvoiceStatus.Paid,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
        });

        MockDatabase.Payments.Clear();
        MockDatabase.Payments.Add(new Payment
        {
            Id = MockDatabase.Ids.ExistingPayment,
            RentalId = MockDatabase.Ids.ExistingRental,
            UserId = MockDatabase.Ids.UserAnna,
            InvoiceId = MockDatabase.Ids.ExistingInvoice,
            Amount = 699m,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.Paid,
            PaymentDateUtc = DateTime.UtcNow.AddDays(-15),
            TransactionReference = encryptionService.Encrypt("MOCK-DEMO-PAID-001"),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
        });

        MockDatabase.AccessCodes.Clear();
        MockDatabase.AccessCodes.Add(new AccessCode
        {
            Id = MockDatabase.Ids.ExistingAccessCode,
            RentalId = MockDatabase.Ids.ExistingRental,
            Code = encryptionService.Encrypt("123456"),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
        });
    }

    private static string HashPassword(string password)
    {
        using var sha512 = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha512.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
