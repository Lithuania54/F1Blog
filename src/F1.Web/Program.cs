using System.Text.Json;
using Markdig;
using Serilog;
using F1.Web.Services;

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

app.UseStaticFiles();
app.UseRouting();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapRazorPages();

app.Run();
