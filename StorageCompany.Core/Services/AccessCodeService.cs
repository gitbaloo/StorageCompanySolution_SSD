using System.Security.Cryptography;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class AccessCodeService(IAccessCodeRepository accessCodes, IRentalRepository rentals, IEncryptionService encryptionService) : IAccessCodeService
{
    private readonly IAccessCodeRepository _accessCodes = accessCodes;
    private readonly IRentalRepository _rentals = rentals;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<AccessCode> GenerateForRentalAsync(Guid rentalId)
    {
        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        var existing = await _accessCodes.GetActiveByRentalIdAsync(rental.Id);
        if (existing is not null)
        {
            existing.Code = _encryptionService.Decrypt(existing.Code);
            return existing;
        }

        var code = new AccessCode
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            Code = _encryptionService.Encrypt(RandomNumberGenerator.GetInt32(100000, 999999).ToString()),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        
        await _accessCodes.AddAsync(code);
        code.Code = _encryptionService.Decrypt(code.Code);
        return code;
    }

    public async Task<AccessCode> GetActiveByRentalIdAsync(Guid rentalId)
    {
        var code = await _accessCodes.GetActiveByRentalIdAsync(rentalId) ?? throw new NotFoundException($"No active access code was found for rental '{rentalId}'.");
        code.Code = _encryptionService.Decrypt(code.Code);
        return code;
    }

    public async Task DeactivateByRentalIdAsync(Guid rentalId)
    {
        var code = await _accessCodes.GetActiveByRentalIdAsync(rentalId);
        if (code is null)
            return;

        code.IsActive = false;
        code.ExpiresAtUtc = DateTime.UtcNow;
        await _accessCodes.UpdateAsync(code);
    }
}
