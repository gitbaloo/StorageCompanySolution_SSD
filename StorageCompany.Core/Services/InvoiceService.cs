using System.Security.Cryptography;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Exceptions;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class InvoiceService(IInvoiceRepository invoices, IRentalRepository rentals, IEncryptionService encryptionService) : IInvoiceService
{
    private readonly IInvoiceRepository _invoices = invoices;
    private readonly IRentalRepository _rentals = rentals;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<Invoice> GenerateForRentalAsync(Guid rentalId, DateTime dueDateUtc)
    {
        var rental = await _rentals.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental '{rentalId}' was not found.");

        if (rental.Status != RentalStatus.Active)
            throw new BusinessRuleException("Invoices can only be generated for active rentals.");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            UserId = rental.UserId,
            InvoiceNumber = _encryptionService.Encrypt($"INV-{DateTime.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(1000, 9999)}"),
            Amount = rental.MonthlyPrice,
            DueDateUtc = DateTime.SpecifyKind(dueDateUtc, DateTimeKind.Utc),
            Status = InvoiceStatus.Unpaid,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _invoices.AddAsync(invoice);
        invoice.InvoiceNumber = _encryptionService.Decrypt(invoice.InvoiceNumber);
        return invoice;
    }

    public async Task<Invoice> GetByIdAsync(Guid id)
    {
        var invoice = await _invoices.GetByIdAsync(id) ?? throw new NotFoundException($"Invoice '{id}' was not found.");
        invoice.InvoiceNumber = _encryptionService.Decrypt(invoice.InvoiceNumber);
        return invoice;
    }

    public async Task<IReadOnlyList<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        var invoices = await _invoices.GetByCustomerIdAsync(customerId);
        return [.. invoices.Select(i => { i.InvoiceNumber = _encryptionService.Decrypt(i.InvoiceNumber); return i; })];
    }

    public async Task<IReadOnlyList<Invoice>> GetByRentalIdAsync(Guid rentalId)
    {
        var invoices = await _invoices.GetByRentalIdAsync(rentalId);
        return [.. invoices.Select(i => { i.InvoiceNumber = _encryptionService.Decrypt(i.InvoiceNumber); return i; })];
    }

    public async Task<Invoice> MarkAsPaidAsync(Guid invoiceId)
    {
        var invoice = await GetByIdAsync(invoiceId);
        invoice.Status = InvoiceStatus.Paid;
        invoice.InvoiceNumber = _encryptionService.Encrypt(invoice.InvoiceNumber);
        await _invoices.UpdateAsync(invoice);
        invoice.InvoiceNumber = _encryptionService.Decrypt(invoice.InvoiceNumber);
        return invoice;
    }
}
