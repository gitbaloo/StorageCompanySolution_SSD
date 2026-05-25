using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class UserService(IUserRepository users, IEncryptionService encryptionService) : IUserService
{
    private readonly IUserRepository _users = users;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        var users = await _users.GetAllAsync();
        return [.. users.Select(DecryptUser)];
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var customer = await _users.GetByIdAsync(id) ?? throw new NotFoundException($"User '{id}' was not found.");

        return DecryptUser(customer);
    }

    public async Task<User> CreateAsync(string firstName, string lastName, string email, string phoneNumber, string password)
    {
        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));
        Guard.AgainstBlank(email, nameof(email));
        Guard.AgainstBlank(password, nameof(password));

        var existing = await _users.GetByEmailAsync(email);
        if (existing is not null)
            throw new BusinessRuleException("A customer with this email already exists.");

        var customer = new User
        {
            Id = Guid.NewGuid(),
            FirstName = _encryptionService.Encrypt(firstName.Trim()),
            LastName = _encryptionService.Encrypt(lastName.Trim()),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = _encryptionService.Encrypt(phoneNumber.Trim()),
            PasswordHash = $"MOCK_HASH::{password.Length}::{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _users.AddAsync(customer);

        return DecryptUser(customer);
    }

    public async Task<User> UpdateAsync(Guid id, string firstName, string lastName, string phoneNumber, bool isActive)
    {
        var customer = await GetByIdAsync(id);

        Guard.AgainstBlank(firstName, nameof(firstName));
        Guard.AgainstBlank(lastName, nameof(lastName));

        customer.FirstName = _encryptionService.Encrypt(firstName.Trim());
        customer.LastName = _encryptionService.Encrypt(lastName.Trim());
        customer.PhoneNumber = _encryptionService.Encrypt(phoneNumber.Trim());
        customer.IsActive = isActive;

        await _users.UpdateAsync(customer);

        return DecryptUser(customer);
    }

    private User DecryptUser(User user)
    {
        user.FirstName = _encryptionService.Decrypt(user.FirstName);
        user.LastName = _encryptionService.Decrypt(user.LastName);
        user.PhoneNumber = _encryptionService.Decrypt(user.PhoneNumber);

        return user;
    }
}
