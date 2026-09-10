using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;
using StudentRecordSystem.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddScoped<TimetableService>();
builder.Services.AddControllers();

var connectionString =
    builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
    

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string was not found.");
}

if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    connectionString = ConvertDatabaseUrlToConnectionString(connectionString);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Students", "AdminOnly");
    options.Conventions.AuthorizeFolder("/ClassRooms", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Subjects", "AdminOnly");
    options.Conventions.AuthorizeFolder("/QuranRecords", "AdminOnly");
    options.Conventions.AuthorizeFolder("/MutoonRecords", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizePage("/Attendances/Take", "AdminOnly");

    options.Conventions.AuthorizePage("/StudentPortal/Attendance", "StudentOnly");
    options.Conventions.AuthorizePage("/StudentPortal/QuranProgress", "StudentOnly");
    options.Conventions.AuthorizePage("/StudentPortal/MutoonProgress", "StudentOnly");
    options.Conventions.AuthorizePage("/StudentPortal/Match", "StudentOnly");
    options.Conventions.AuthorizePage("/StudentPortal/Requests", "StudentOnly");

    options.Conventions.AuthorizeFolder("/Timetable", "AdminOnly");
    options.Conventions.AuthorizePage("/TeacherPortal/MyTimetable", "TeacherOnly");
    options.Conventions.AuthorizePage("/StudentPortal/MySchedule", "StudentOnly");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders = new IRequestCultureProvider[]
{
    new CookieRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
};

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = uri.AbsolutePath.Trim('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    return builder.ConnectionString;
}