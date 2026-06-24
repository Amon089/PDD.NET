using Habitto.Domain.Interfaces;
using Habitto.Infrastructure.Persistence;
using Habitto.Infrastructure.Persistence.Repositories;
using Habitto.Infrastructure.Services;
using Habitto.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Habitto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HabittoDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<HabittoDbContext>());

        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        // Mocks intercambiables: para apuntar a un proveedor real, reemplazar
        // SOLO estas dos líneas.
        services.AddScoped<IIdentityVerificationService, MockIdentityVerificationService>();
        services.AddScoped<INotificationService, MockNotificationService>();

        services.AddScoped<IExcelReportExporter, ClosedXmlReportExporter>();

        return services;
    }
}
