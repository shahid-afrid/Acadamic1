using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using TeamPro1.Models;
using TeamPro1.Hubs;  // SignalR Hub
using TeamPro1.Data;  // For DbSeeder

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session (for Student/Faculty/Admin login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Configure forwarded headers so the app works correctly behind ngrok/reverse proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                             | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Ensure database is created and migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // SECURITY: Only seed test data in development environment
    // Remove this line or comment out for production deployment
    if (app.Environment.IsDevelopment())
    {
        await DbSeeder.SeedTestFacultyAsync(db);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Support forwarded headers from ngrok / reverse proxies
app.UseForwardedHeaders();

// Only use HTTPS redirection when the app is listening on both HTTP and HTTPS.
// When running HTTP-only (e.g., behind ngrok), this prevents 307 redirect loops.
var serverAddresses = app.Urls;
var hasHttpsUrl = serverAddresses.Any(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (hasHttpsUrl)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// Session must be before Authorization
app.UseSession();

app.UseAuthorization();

// Map SignalR Hub endpoint
app.MapHub<NotificationHub>("/notifyHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

