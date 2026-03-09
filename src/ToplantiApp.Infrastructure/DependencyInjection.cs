using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;
using ToplantiApp.Infrastructure.Repositories;
using ToplantiApp.Infrastructure.Services;

namespace ToplantiApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}
