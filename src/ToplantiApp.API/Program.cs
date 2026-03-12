using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ToplantiApp.API.Middleware;
using ToplantiApp.API.Services;
using ToplantiApp.Application;
using ToplantiApp.Application.Common;
using ToplantiApp.Infrastructure;
using ToplantiApp.Infrastructure.Data;
using ToplantiApp.Infrastructure.Data.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Toplanti Yonetim API",
        Version = "v1",
        Description = "Toplanti yonetim sistemi REST API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["App:FrontendUrl"] ?? "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        logger.LogInformation("Veritabani migration calistiriliyor...");
        db.Database.Migrate();
        logger.LogInformation("Migration tamamlandi.");
    }
    catch (Exception ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;
        logger.LogError(ex, "Veritabani migration basarisiz. Uygulama kapaniyor. Hata: {Message}", inner);
        throw;
    }

    try
    {
        db.ApplyTriggerMigrations();
        logger.LogInformation("Trigger migration tamamlandi.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Trigger migration atlandi (trigger zaten mevcut veya SQL hatasi). Uygulama devam ediyor.");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // "/" isteğinde wwwroot/index.html döner
app.UseStaticFiles();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Docker/load balancer icin basit canlilik kontrolu (veritabani bagimliligi yok)
app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();

// SPA: API'ye ait olmayan path'lerde index.html döndür (Angular client-side routing)
app.MapFallbackToFile("index.html");

app.Run();
