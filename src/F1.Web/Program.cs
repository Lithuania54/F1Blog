// Program.cs (recommended final version)
// - Registers ASP.NET Core Identity (login/register system)
// - Registers IHttpContextAccessor for navbar injection
// - Keeps Serilog logging
// - Maps static folders for /images and /images2
// - Ensures all required directories exist
// - Applies pending EF migrations on startup (development convenience)

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using F1.Web.Services;
using F1.Web.Data;
using F1.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Serilog Configuration
// --------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --------------------
// Database + Identity Setup
// --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=f1blog.db"));

// helpful developer page for EF errors
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// --------------------
// Services Registration
// --------------------
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor(); // Needed for _Nav.cshtml
builder.Services.AddMemoryCache();

// Application Services
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<NewsletterService>();
builder.Services.AddSingleton<ContactService>();
builder.Services.AddHttpClient<IF1DataService, F1DataService>();
builder.Services.AddScoped<IContentSpotlightService, ContentSpotlightService>();

var app = builder.Build();

// --------------------
// Directory Setup
// --------------------
var contentRoot = app.Environment.ContentRootPath;

var storagePath = Path.Combine(contentRoot, "storage");
var postsPath = Path.Combine(contentRoot, "content", "posts");
var imagesPath = Path.Combine(contentRoot, "images");
var images2Path = Path.Combine(contentRoot, "images2");

Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(postsPath);
Directory.CreateDirectory(imagesPath);
Directory.CreateDirectory(images2Path);

// --------------------
// Static File Setup
// --------------------

// Default static file provider (wwwroot)
app.UseStaticFiles();

// Serve images from ContentRootPath/images at /images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

// Serve secondary images (race tracks) from /images2
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(images2Path),
    RequestPath = "/images2"
});

// --------------------
// Middleware Pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // show EF Core database error page for developers
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseAuthentication(); // Identity
app.UseAuthorization();

app.MapRazorPages();

// --------------------
// Apply pending EF migrations on startup (development convenience)
// --------------------
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        // In dev this will apply migrations automatically. In production you may prefer manual migration.
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    // Avoid crashing the app on migration errors; Serilog is already configured.
    Log.Error(ex, "An error occurred while migrating or initializing the database.");
}

// --------------------
// Run Application
// --------------------
app.Run();
