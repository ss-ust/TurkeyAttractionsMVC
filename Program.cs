using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurkeyAttractionsMVC.Data;

var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services using ApplicationDbContext
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed admin role and user
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Ensure the admin role exists
    var roleExists = await roleManager.RoleExistsAsync("admin");
    if (!roleExists)
    {
        await roleManager.CreateAsync(new IdentityRole("admin"));
    }

    // Create admin user if it doesn't exist
    var adminUserEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminUserEmail,
            Email = adminUserEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Pass123.");
        if (result.Succeeded)
        {
            // Assign the user to admin role
            await userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure Authentication is used before Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
