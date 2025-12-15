using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using F1.Web.Services;
using F1.Web.Data;
using F1.Web.Models;

var builder = WebApplication.CreateBuilder(args);


// Serilog Configuration

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database + Identity Setup

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=f1blog.db"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Services Registration

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor(); // Needed for _Nav.cshtml
builder.Services.AddMemoryCache();

// Application Services
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<NewsletterService>();
builder.Services.AddSingleton<ContactService>();
builder.Services.AddHttpClient<IF1ResultsService, F1ResultsService>();
builder.Services.AddHttpClient<IF1DataService, F1DataService>();
builder.Services.AddScoped<IContentSpotlightService, ContentSpotlightService>();

var app = builder.Build();

// Directory Setup

var contentRoot = app.Environment.ContentRootPath;

var storagePath = Path.Combine(contentRoot, "storage");
var postsPath = Path.Combine(contentRoot, "content", "posts");
var imagesPath = Path.Combine(contentRoot, "images");
var images2Path = Path.Combine(contentRoot, "images2");

Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(postsPath);
Directory.CreateDirectory(imagesPath);
Directory.CreateDirectory(images2Path);

// Static File Setup

var contentTypeProvider = new FileExtensionContentTypeProvider();

// Default static file provider (wwwroot)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

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

// Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


// Apply pending EF migrations on startup (development convenience)

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while migrating or initializing the database.");
}

app.Run();
