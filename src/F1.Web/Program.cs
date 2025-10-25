// Program.cs (final fixed version)
// - Registers IHttpContextAccessor for navbar injection
// - Keeps Serilog logging
// - Maps static folders for /images and /images2
// - Ensures all required directories exist

using System.Text.Json;
using Markdig;
using Serilog;
using F1.Web.Services;
using Microsoft.Extensions.FileProviders;

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
// Services Registration
// --------------------
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor(); // ✅ Needed for _Nav.cshtml

// Application Services
builder.Services.AddHttpContextAccessor();
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapRazorPages();

// --------------------
// Run Application
// --------------------
app.Run();
