// Program.cs (modified):
// - Adds a static-file mapping so repository-level images (ContentRootPath/images)
//   are served at the request path /images during development.
// - For production, copy images into src/F1.Web/wwwroot/images/ so default
//   static file provider serves them. See the TODO comments in IndexModel.
// ASSUMPTION: Developers may keep images in repo under src/images/ for convenience.
using System.Text.Json;
using Markdig;
using Serilog;
using F1.Web.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Serilog (simple, reads appsettings if present)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Razor Pages
builder.Services.AddRazorPages();

// Services
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<NewsletterService>();
builder.Services.AddSingleton<ContactService>();

var app = builder.Build();

// Ensure storage and content folders exist
var storagePath = Path.Combine(app.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "content", "posts"));
// Ensure a development-level `src/images` folder exists for repository-stored images.
// ASSUMPTION: Developers may keep images in `src/images/` (one level above this project)
// during development. We will also serve that folder at the request path `/images` so
// the carousel can reference `/images/1.avif` without copying files.
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "images"));
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "images2"));

// Serve files from wwwroot (default)
app.UseStaticFiles();

// Also serve images from ContentRootPath/images at request path /images.
// This allows putting images in the repository root `src/images/` during development
// and have them available at `/images/...`. For production, copy images into
// `src/F1.Web/wwwroot/images/` so the default static file provider serves them.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "images")),
    RequestPath = "/images"
});
// Also serve repository-level images2 folder at request path /images2 so
// developers can keep track/track assets in src/F1.Web/images2 during development.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "images2")),
    RequestPath = "/images2"
});
app.UseRouting();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapRazorPages();

app.Run();
