using System.Security.Cryptography;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class PaymentService(IRentalRepository rentals, IPaymentRepository payments, IInvoiceRepository invoices, IEncryptionService encryptionService) : IPaymentService
{
    private readonly IRentalRepository _rentals = rentals;
    private readonly IPaymentRepository _payments = payments;
    private readonly IInvoiceRepository _invoices = invoices;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<Payment> GetByIdAsync(Guid id)
    {
        var payment = await _payments.GetByIdAsync(id) ?? throw new NotFoundException($"Payment '{id}' was not found.");
        payment.TransactionReference = _encryptionService.Decrypt(payment.TransactionReference);
        return payment;
    }

    public async Task<Payment> CreateMockPaymentAsync(Guid rentalId, decimal amount, PaymentMethod paymentMethod, Guid? invoiceId = null)
    {
        Guard.AgainstEmpty(rentalId, nameof(rentalId));
        Guard.AgainstNonPositive(amount, nameof(amount));

        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        if (rental.Status != RentalStatus.Active)
            throw new BusinessRuleException("Payments can only be made for active rentals.");

        Invoice? invoice = null;
        if (invoiceId.HasValue)
        {
            invoice = await _invoices.GetByIdAsync(invoiceId.Value)
                ?? throw new NotFoundException($"Invoice '{invoiceId}' was not found.");

            if (invoice.RentalId != rental.Id)
                throw new BusinessRuleException("The invoice does not belong to the selected rental.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            UserId = rental.UserId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Paid,
            PaymentDateUtc = DateTime.UtcNow,
            TransactionReference = _encryptionService.Encrypt($"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}"),
            CreatedAtUtc = DateTime.UtcNow
        };

        if (invoice is not null)
        {
            invoice.Status = InvoiceStatus.Paid;
            await _invoices.UpdateAsync(invoice);
        }

        await _payments.AddAsync(payment);
        payment.TransactionReference = _encryptionService.Decrypt(payment.TransactionReference);
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId)
    {
        var payments = await _payments.GetByCustomerIdAsync(customerId);
        return [.. payments.Select(p => { p.TransactionReference = _encryptionService.Decrypt(p.TransactionReference); return p; })];
    }

    public async Task<IReadOnlyList<Payment>> GetByRentalIdAsync(Guid rentalId)
    {
        var payments = await _payments.GetByRentalIdAsync(rentalId);
        return [.. payments.Select(p => { p.TransactionReference = _encryptionService.Decrypt(p.TransactionReference); return p; })];
    }
}
