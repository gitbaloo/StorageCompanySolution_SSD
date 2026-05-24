using System.Text.Json.Serialization;
using NSwag;
using NSwag.Generation.Processors.Security;
using Scalar.AspNetCore;
using StorageCompany.Api.Middleware;
using StorageCompany.Core;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Repositories;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;


namespace StorageCompany.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services
            .AddOptions<AppOptions>()
            .Bind(builder.Configuration.GetSection("AppOptions"))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.JwtSecret), "JwtSecret is required")
            .ValidateOnStart();

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCors", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Repositories: infrastructure implementations behind Core interfaces.
        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IFacilityRepository, FacilityRepository>();
        builder.Services.AddSingleton<IStorageUnitTypeRepository, StorageUnitTypeRepository>();
        builder.Services.AddSingleton<IStorageUnitRepository, StorageUnitRepository>();
        builder.Services.AddSingleton<IReservationRepository, ReservationRepository>();
        builder.Services.AddSingleton<IRentalRepository, RentalRepository>();
        builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
        builder.Services.AddSingleton<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddSingleton<IAccessCodeRepository, AccessCodeRepository>();
        builder.Services.AddSingleton<ISupportRequestRepository, SupportRequestRepository>();

        // Core business services.
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IFacilityService, FacilityService>();
        builder.Services.AddScoped<IStorageUnitTypeService, StorageUnitTypeService>();
        builder.Services.AddScoped<IStorageUnitService, StorageUnitService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IRentalService, RentalService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IInvoiceService, InvoiceService>();
        builder.Services.AddScoped<IAccessCodeService, AccessCodeService>();
        builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("AuthPolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetAuthRateLimitKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("GlobalApiPolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        static string GetAuthRateLimitKey(HttpContext context)
        {
            var ip = GetClientIp(context);

            return $"auth:{ip}";
        }

        static string GetClientIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        var app = builder.Build();

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseHttpsRedirection();
        app.UseCors("DevelopmentCors");
        app.UseRateLimiter();

        app.MapControllers()
           .RequireRateLimiting("GlobalApiPolicy");


        app.UseOpenApi(conf =>
        {
            conf.Path = "openapi/v1.json";
        });
        app.MapScalarApiReference();
        

        app.Run();
    }
}
