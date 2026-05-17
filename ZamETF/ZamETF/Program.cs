using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Korisnik, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Kreiraj admin korisnika pri pokretanju
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Korisnik>>();

    var admin = await userManager.FindByEmailAsync("admin@zametf.ba");
    if (admin == null)
    {
        var noviAdmin = new Administrator
        {
            Ime = "Admin",
            Prezime = "ZamETF",
            UserName = "admin",
            Email = "admin@zametf.ba",
            Uloga = Uloga.Administrator,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(noviAdmin, "Admin123!");
    }
}

app.Run();