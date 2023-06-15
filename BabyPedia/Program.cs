
using BabyPedia.Controllers;
using BabyPedia.Data;
using BabyPedia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BabyPediaContextConnection") ??
                       throw new InvalidOperationException("Connection string 'BabyPediaContextConnection' not found.");

// Add services to the container.
builder.Services.AddDbContext<BabyPediaContext>(o =>
{
    o.UseSqlite(connectionString);
    o.EnableSensitiveDataLogging();
});

builder.Services.AddSignalR();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BabyPediaContext>();
builder.Services.AddSingleton<EmailHandler>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapRazorPages();
app.UseRouting();
app.UseAuthentication();

app.MapHub<ChatHub>("/chatHub");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

async Task CreateAccountsAndRole(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    {
        string adminRoleName = "Admin";
        string adminEmail = "admin@admin.com";
        string adminPassword = "AdminPassword123";

        // Check if the admin role exists, and create it if it doesn't
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        // Check if the admin user exists, and create it if it doesn't
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            IdentityUser adminUser = new IdentityUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }

    {
        string pediaroleName = "Pedia";
        string pediaEmail = "pedia@pedia.com";
        string pediaPassword = "password123";

        // Check if the admin role exists, and create it if it doesn't
        if (!await roleManager.RoleExistsAsync(pediaroleName))
        {
            await roleManager.CreateAsync(new IdentityRole(pediaroleName));
        }

        // Check if the admin user exists, and create it if it doesn't
        if (await userManager.FindByEmailAsync(pediaEmail) == null)
        {
            var adminUser = new PartneredPedia()
            {
                Email = pediaEmail,
                UserName = pediaEmail,
                FirstName = "Albert",
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, pediaPassword);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, pediaroleName);
        }
    }

    {
        string pediaroleName = "Parent";
        string pediaEmail = "parent@parent.com";
        string pediaPassword = "password123";

        // Check if the admin role exists, and create it if it doesn't
        if (!await roleManager.RoleExistsAsync(pediaroleName))
        {
            await roleManager.CreateAsync(new IdentityRole(pediaroleName));
        }

        // Check if the admin user exists, and create it if it doesn't
        if (await userManager.FindByEmailAsync(pediaEmail) == null)
        {
            Parent adminUser = new Parent
            {
                Email = pediaEmail,
                UserName = pediaEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, pediaPassword);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, pediaroleName);
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<BabyPediaContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await context.Database.EnsureCreatedAsync();

    // Add admin.
    await CreateAccountsAndRole(userManager, roleManager);

    // Add Vaccines.
    List<Vaccine> vaccines = new List<Vaccine>()
    {
        new Vaccine()
        {
            Name = "Hepatitis B",
            LotNumber = "HBV1234",
            Manufacturer = "AstraZeneca",
            ExpirationDate = new DateTime(2023, 10, 1),
            Disease = "Hepatitis B",
            DiseaseSymptoms = "Fever, Fatigue, Abdominal pain, Jaundice",
            DiseaseComplications = "Liver cancer, Liver failure",
            RecommendedAge = 0.5,
            DateTimeCreated = DateTime.UtcNow
        },
        new Vaccine()
        {
            Name = "Pentavalent Vaccine",
            LotNumber = "PV5678",
            Manufacturer = "Pfizer",
            ExpirationDate = new DateTime(2024, 4, 1),
            Disease = "Diphtheria, Tetanus, Pertussis, Hepatitis B, Haemophilus influenzae type b",
            DiseaseSymptoms = "Fever, Cough, Sore throat, Weakness, Vomiting",
            DiseaseComplications = "Brain damage, Pneumonia, Ear infection",
            RecommendedAge = 2,
            DateTimeCreated = DateTime.UtcNow
        },
        new Vaccine()
        {
            Name = "Oral Polio Vaccine",
            LotNumber = "OPV9012",
            Manufacturer = "Sanofi",
            ExpirationDate = new DateTime(2023, 12, 1),
            Disease = "Polio",
            DiseaseSymptoms = "Fever, Sore throat, Headache, Vomiting",
            DiseaseComplications = "Paralysis, Difficulty breathing",
            RecommendedAge = 1.5,
            DateTimeCreated = DateTime.UtcNow
        },
        new Vaccine()
        {
            Name = "Inactivated Polio Vaccine",
            LotNumber = "IPV3456",
            Manufacturer = "Novartis",
            ExpirationDate = new DateTime(2022, 8, 1),
            Disease = "Polio",
            DiseaseSymptoms = "Fever, Sore throat, Headache, Vomiting",
            DiseaseComplications = "Paralysis, Difficulty breathing",
            RecommendedAge = 2,
            DateTimeCreated = DateTime.UtcNow
        },
        new Vaccine()
        {
            Name = "Pneumococcal Vaccine",
            LotNumber = "PV8910",
            Manufacturer = "Johnson & Johnson",
            ExpirationDate = new DateTime(2023, 6, 1),
            Disease = "Pneumococcal disease",
            DiseaseSymptoms = "Fever, Cough, Chest pain, Shortness of breath",
            DiseaseComplications = "Meningitis, Blood infection, Pneumonia",
            RecommendedAge = 2,
            DateTimeCreated = DateTime.UtcNow
        },
        new Vaccine()
        {
            Name = "Measles, Mumps, Rubella",
            LotNumber = "MMR1112",
            Manufacturer = "Merck",
            ExpirationDate = new DateTime(2024, 1, 1),
            Disease = "Measles, Mumps, Rubella",
            DiseaseSymptoms = "Fever, Rash, Swollen glands, Muscle pain",
            DiseaseComplications = "Encephalitis, Meningitis, Deafness",
            RecommendedAge = 1,
            DateTimeCreated = DateTime.UtcNow
        }
    };

    await context.Vaccines.AddRangeAsync(vaccines);
    await context.SaveChangesAsync();
}


app.Run();