using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Data;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register your custom services and repositories
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
// Ensure IPolicyService is registered AFTER IVehicleService and ICoverageLevelService
// because PolicyService depends on them.
builder.Services.AddScoped<IPolicyService, PolicyService>();


// NEW: Register CoverageLevel Repository and Service
builder.Services.AddScoped<ICoverageLevelRepository, CoverageLevelRepository>();
builder.Services.AddScoped<ICoverageLevelService, CoverageLevelService>();
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();

builder.Services.AddScoped<IReportingPolicyRepository, ReportingPolicyRepository>();
builder.Services.AddScoped<IReportingClaimRepository, ReportingClaimRepository>();
builder.Services.AddScoped<IReportingVehicleRepository, ReportingVehicleRepository>();
builder.Services.AddScoped<IReportingCustomerRepository, ReportingCustomerRepository>();


builder.Services.AddScoped<IReportService, ReportService>();



builder.Services.AddScoped<RoleManager<IdentityRole>>();


var app = builder.Build();

// Seed roles and admin user on application startup
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Seed initial Coverage Levels if they don't exist
        await DbInitializer.SeedRolesAndUsers(serviceProvider); // Existing seeding
        await SeedCoverageLevels(serviceProvider); // NEW: Seed Coverage Levels

    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database or applying migrations.");
    }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


// NEW: Helper method to seed initial Coverage Levels
async Task SeedCoverageLevels(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!context.CoverageLevels.Any())
        {
            var basic = new CoverageLevel { Name = "Basic", Description = "Minimal coverage for essential protection.", PremiumMultiplier = 1.0m, CoverageMultiplier = 1.0m };
            var standard = new CoverageLevel { Name = "Standard", Description = "Balanced coverage for common risks.", PremiumMultiplier = 1.2m, CoverageMultiplier = 1.5m };
            var premium = new CoverageLevel { Name = "Premium", Description = "Comprehensive coverage with enhanced benefits.", PremiumMultiplier = 1.5m, CoverageMultiplier = 2.0m };

            context.CoverageLevels.AddRange(basic, standard, premium);
            await context.SaveChangesAsync();
        }
    }
}
