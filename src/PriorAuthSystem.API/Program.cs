using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.API.Hubs;
using PriorAuthSystem.API.Middleware;
using PriorAuthSystem.API.Services;
using PriorAuthSystem.Application.Common.Behaviors;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;
using PriorAuthSystem.Infrastructure.Repositories;
using PriorAuthSystem.Infrastructure.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // EF Core - SQL Server LocalDB
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories & Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Infrastructure services
    builder.Services.AddSingleton<FhirMappingService>();
    builder.Services.AddSingleton<AuditService>();

    // MediatR + pipeline behaviors
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(LoggingBehavior<,>).Assembly));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(LoggingBehavior<,>).Assembly);

    // Controllers
    builder.Services.AddControllers();

    // OpenAPI / Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // SignalR
    builder.Services.AddSignalR();
    builder.Services.AddScoped<IPriorAuthNotificationService, PriorAuthNotificationService>();

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbInitializer.SeedAsync(context);
    }

    // Global exception handler
    app.UseMiddleware<GlobalExceptionHandler>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.MapControllers();
    app.MapHub<PriorAuthHub>("/hubs/priorauth");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
