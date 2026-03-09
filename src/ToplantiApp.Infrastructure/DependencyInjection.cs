using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;
using ToplantiApp.Infrastructure.Jobs;
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

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("CleanupCancelledMeetings");
            q.AddJob<CleanupCancelledMeetingsJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("CleanupCancelledMeetings-trigger")
                .WithCronSchedule("0 0 2 * * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
