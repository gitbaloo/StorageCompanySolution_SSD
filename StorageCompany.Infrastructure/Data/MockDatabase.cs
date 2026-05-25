using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using System.Security.Cryptography;
using System.Text;

namespace StorageCompany.Infrastructure.Data;

public static class MockDatabase
{
    public static readonly object SyncRoot = new();

    public static class Ids
    {
        public static readonly Guid UserAnna = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid UserPeter = Guid.Parse("10000000-0000-0000-0000-000000000002");

        public static readonly Guid FacilityCopenhagen = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid FacilityAarhus = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid FacilityOdense = Guid.Parse("20000000-0000-0000-0000-000000000003");

        public static readonly Guid UnitTypeSmall = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid UnitTypeMedium = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid UnitTypeLarge = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public static readonly Guid UnitTypeBusiness = Guid.Parse("30000000-0000-0000-0000-000000000004");

        public static readonly Guid UnitCphSmall = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid UnitCphMedium = Guid.Parse("40000000-0000-0000-0000-000000000002");
        public static readonly Guid UnitCphLarge = Guid.Parse("40000000-0000-0000-0000-000000000003");
        public static readonly Guid UnitAarhusSmall = Guid.Parse("40000000-0000-0000-0000-000000000004");
        public static readonly Guid UnitAarhusBusiness = Guid.Parse("40000000-0000-0000-0000-000000000005");
        public static readonly Guid UnitOdenseMedium = Guid.Parse("40000000-0000-0000-0000-000000000006");

        public static readonly Guid ExistingRental = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid ExistingInvoice = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public static readonly Guid ExistingPayment = Guid.Parse("70000000-0000-0000-0000-000000000001");
        public static readonly Guid ExistingAccessCode = Guid.Parse("80000000-0000-0000-0000-000000000001");
        public static readonly Guid ExistingSupportRequest = Guid.Parse("90000000-0000-0000-0000-000000000001");
    }

    public static List<User> Users { get; } = [];

    public static List<Facility> Facilities { get; } =
    [
        new Facility
        {
            Id = Ids.FacilityCopenhagen,
            Name = "Nordic Storage Copenhagen",
            Address = "Industrivej 12",
            City = "Copenhagen",
            PostalCode = "2300",
            Country = "Denmark",
            PhoneNumber = "+45 70 10 20 30",
            Email = "copenhagen@nordicstorage.demo",
            AccessHours = "06:00 - 22:00",
            OfficeHours = "09:00 - 17:00",
            HasParking = true,
            HasElevator = true,
            HasCCTV = true,
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-6)
        },
        new Facility
        {
            Id = Ids.FacilityAarhus,
            Name = "Nordic Storage Aarhus",
            Address = "Lagergade 8",
            City = "Aarhus",
            PostalCode = "8000",
            Country = "Denmark",
            PhoneNumber = "+45 70 10 20 31",
            Email = "aarhus@nordicstorage.demo",
            AccessHours = "05:00 - 23:00",
            OfficeHours = "09:00 - 16:00",
            HasParking = true,
            HasElevator = false,
            HasCCTV = true,
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-4)
        },
        new Facility
        {
            Id = Ids.FacilityOdense,
            Name = "Nordic Storage Odense",
            Address = "Flyttevej 4",
            City = "Odense",
            PostalCode = "5000",
            Country = "Denmark",
            PhoneNumber = "+45 70 10 20 32",
            Email = "odense@nordicstorage.demo",
            AccessHours = "07:00 - 21:00",
            OfficeHours = "10:00 - 16:00",
            HasParking = true,
            HasElevator = true,
            HasCCTV = true,
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-2)
        }
    ];

    public static List<StorageUnitType> StorageUnitTypes { get; } =
    [
        new StorageUnitType
        {
            Id = Ids.UnitTypeSmall,
            Name = "Small",
            SizeInSquareMeters = 2.5m,
            Description = "Compact unit for boxes, seasonal items, and small furniture.",
            RecommendedFor = "Student room or small decluttering project",
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-6)
        },
        new StorageUnitType
        {
            Id = Ids.UnitTypeMedium,
            Name = "Medium",
            SizeInSquareMeters = 6m,
            Description = "Useful space for furniture, moving boxes, and appliances.",
            RecommendedFor = "One-bedroom apartment",
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-6)
        },
        new StorageUnitType
        {
            Id = Ids.UnitTypeLarge,
            Name = "Large",
            SizeInSquareMeters = 12m,
            Description = "Larger storage for home moves or renovation projects.",
            RecommendedFor = "Two-bedroom home",
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-6)
        },
        new StorageUnitType
        {
            Id = Ids.UnitTypeBusiness,
            Name = "Business",
            SizeInSquareMeters = 18m,
            Description = "Flexible space for business stock, tools, or archive storage.",
            RecommendedFor = "Small business inventory",
            CreatedAtUtc = DateTime.UtcNow.AddMonths(-6)
        }
    ];

    public static List<StorageUnit> StorageUnits { get; } =
    [
        new StorageUnit { Id = Ids.UnitCphSmall, FacilityId = Ids.FacilityCopenhagen, UnitTypeId = Ids.UnitTypeSmall, UnitNumber = "CPH-A101", Floor = 0, MonthlyPrice = 349m, Status = StorageUnitStatus.Available, IsClimateControlled = false, IsDriveUp = true, CreatedAtUtc = DateTime.UtcNow.AddMonths(-5) },
        new StorageUnit { Id = Ids.UnitCphMedium, FacilityId = Ids.FacilityCopenhagen, UnitTypeId = Ids.UnitTypeMedium, UnitNumber = "CPH-B203", Floor = 1, MonthlyPrice = 699m, Status = StorageUnitStatus.Rented, IsClimateControlled = true, IsDriveUp = false, CreatedAtUtc = DateTime.UtcNow.AddMonths(-5) },
        new StorageUnit { Id = Ids.UnitCphLarge, FacilityId = Ids.FacilityCopenhagen, UnitTypeId = Ids.UnitTypeLarge, UnitNumber = "CPH-C010", Floor = 0, MonthlyPrice = 1199m, Status = StorageUnitStatus.Available, IsClimateControlled = true, IsDriveUp = true, CreatedAtUtc = DateTime.UtcNow.AddMonths(-5) },
        new StorageUnit { Id = Ids.UnitAarhusSmall, FacilityId = Ids.FacilityAarhus, UnitTypeId = Ids.UnitTypeSmall, UnitNumber = "AAR-A014", Floor = 0, MonthlyPrice = 299m, Status = StorageUnitStatus.Available, IsClimateControlled = false, IsDriveUp = true, CreatedAtUtc = DateTime.UtcNow.AddMonths(-3) },
        new StorageUnit { Id = Ids.UnitAarhusBusiness, FacilityId = Ids.FacilityAarhus, UnitTypeId = Ids.UnitTypeBusiness, UnitNumber = "AAR-D001", Floor = 0, MonthlyPrice = 1699m, Status = StorageUnitStatus.Available, IsClimateControlled = true, IsDriveUp = true, CreatedAtUtc = DateTime.UtcNow.AddMonths(-3) },
        new StorageUnit { Id = Ids.UnitOdenseMedium, FacilityId = Ids.FacilityOdense, UnitTypeId = Ids.UnitTypeMedium, UnitNumber = "ODE-B050", Floor = 1, MonthlyPrice = 599m, Status = StorageUnitStatus.Maintenance, IsClimateControlled = false, IsDriveUp = false, CreatedAtUtc = DateTime.UtcNow.AddMonths(-2) }
    ];

    public static List<Reservation> Reservations { get; } = [];

    public static List<Rental> Rentals { get; } =
    [
        new Rental
        {
            Id = Ids.ExistingRental,
            UserId = Ids.UserAnna,
            StorageUnitId = Ids.UnitCphMedium,
            StartDateUtc = DateTime.UtcNow.Date.AddDays(-15),
            MonthlyPrice = 699m,
            Status = RentalStatus.Active,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-15)
        }
    ];

    public static List<Invoice> Invoices { get; } = [];

    public static List<Payment> Payments { get; } = [];


    public static List<AccessCode> AccessCodes { get; } = [];

    public static List<SupportRequest> SupportRequests { get; } =
    [
        new SupportRequest
        {
            Id = Ids.ExistingSupportRequest,
            UserId = Ids.UserAnna,
            RentalId = Ids.ExistingRental,
            Subject = "Need help with access code",
            Message = "This is a demo support request for the showcase API.",
            Status = SupportRequestStatus.Open,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        }
    ];
}
