using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;
using TeamPro1.Hubs;  // ? SignalR Hub
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

// ? Add SignalR for real-time notifications
builder.Services.AddSignalR();

var app = builder.Build();

// Ensure database is created and migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    // Seed test data (only runs if database is empty)
    // Comment out this line after first run if you don't want to seed data
    await DbSeeder.SeedTestFacultyAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session must be before Authorization
app.UseSession();

app.UseAuthorization();

// ? Map SignalR Hub endpoint
app.MapHub<NotificationHub>("/notifyHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

