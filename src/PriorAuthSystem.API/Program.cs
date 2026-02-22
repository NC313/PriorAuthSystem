using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Application.Common.Behaviors;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;
using PriorAuthSystem.Infrastructure.Repositories;
using PriorAuthSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

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
    cfg.RegisterServicesFromAssembly(typeof(PriorAuthSystem.Application.Common.Behaviors.LoggingBehavior<,>).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(PriorAuthSystem.Application.Common.Behaviors.LoggingBehavior<,>).Assembly);

// OpenAPI / Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
