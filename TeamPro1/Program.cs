using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;
using TeamPro1.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ? MVC
builder.Services.AddControllersWithViews();

// ? DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Session (for Student/Faculty/HOD login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// (Optional) ? SignalR for live notifications
builder.Services.AddSignalR();

var app = builder.Build();

// Ensure database is created and migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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

// ? Session must be before Authorization
app.UseSession();

app.UseAuthorization();

// (Optional) SignalR endpoint
app.MapHub<NotificationHub>("/notifyHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
