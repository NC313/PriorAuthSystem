using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPendingPriorAuths;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;
using PriorAuthSystem.Infrastructure.Repositories;
using PriorAuthSystem.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("PriorAuthApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});

// EF Core - SQL Server LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Notification service (Web uses a no-op; real-time updates flow via SignalR client)
builder.Services.AddScoped<IPriorAuthNotificationService, WebNotificationService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetPendingPriorAuthsQuery).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal sealed class WebNotificationService : IPriorAuthNotificationService
{
    public Task SendStatusUpdate(Guid requestId, string newStatus) => Task.CompletedTask;
}
