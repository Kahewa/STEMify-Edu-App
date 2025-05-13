using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using STEMify.Data;
using STEMify.Data.Repositories;
using STEMify.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure connection strings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var appConnectionString = builder.Configuration.GetConnectionString("AppConnection");

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(appConnectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings (e.g. access denied path)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork(provider.GetRequiredService<AppDbContext>()));

// Register generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddHttpClient();



var app = builder.Build();

// HTTP request pipeline configuration
if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//// Seed roles and admin user
//using(var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
//        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
//        await SeedData.Initialize(services, "YourAdminPassword123!");
//    }
//    catch(Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred seeding the DB.");
//    }
//}

app.Run();
