using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Interfaces;
using Kurvenanzeige.Infrastructure.Data;
using Kurvenanzeige.Infrastructure.Data.Repositories;
using Kurvenanzeige.Infrastructure.Plc;
using Kurvenanzeige.Infrastructure.Services;
using Kurvenanzeige.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration sections
builder.Services.Configure<PlcConnectionConfig>(
    builder.Configuration.GetSection("PlcConnection"));
builder.Services.Configure<DataPollingConfig>(
    builder.Configuration.GetSection("DataPolling"));
builder.Services.Configure<DataRetentionConfig>(
    builder.Configuration.GetSection("DataRetention"));

// Application services
builder.Services.AddSingleton<IPlcService, S7PlcService>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddSingleton<UiUpdateService>();

// Background services
builder.Services.AddHostedService<DataPollingService>();
builder.Services.AddHostedService<DataArchivingService>();

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.InitializeDatabaseAsync();
}

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
