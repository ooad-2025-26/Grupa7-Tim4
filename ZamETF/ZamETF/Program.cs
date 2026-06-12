using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using iText.Commons;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Korisnik, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ZamETF.Services.EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

// Kreiraj role i admin korisnika pri pokretanju
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Korisnik>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    // Kreiraj role ako ne postoje
    string[] role = { "Administrator", "Student", "Profesor", "StudentskaSluzba" };
    foreach (var rola in role)
    {
        if (!await roleManager.RoleExistsAsync(rola))
            await roleManager.CreateAsync(new IdentityRole<int>(rola));
    }

    // Kreiraj admin korisnika ako ne postoji
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
        var result = await userManager.CreateAsync(noviAdmin, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(noviAdmin, "Administrator");
    }
    else
    {
        // Ako admin već postoji ali nije u roli, dodaj ga
        if (!await userManager.IsInRoleAsync(admin, "Administrator"))
            await userManager.AddToRoleAsync(admin, "Administrator");
    }
}

app.Run();
