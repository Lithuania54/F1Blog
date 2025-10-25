// Program.cs (final version with Identity + existing features)
// - Registers ASP.NET Core Identity (login/register system)
// - Registers IHttpContextAccessor for navbar injection
// - Keeps Serilog logging
// - Maps static folders for /images and /images2
// - Ensures all required directories exist

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using F1.Web.Services;
using F1.Web.Data; // ✅ Add this namespace after you create ApplicationDbContext.cs

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

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// --------------------
// Services Registration
// --------------------
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor(); // ✅ Needed for _Nav.cshtml

// Application Services
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<NewsletterService>();
builder.Services.AddSingleton<ContactService>();

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
app.UseRouting();
app.UseSerilogRequestLogging();

app.UseAuthentication(); // ✅ Identity middleware
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapRazorPages();

app.Run();
