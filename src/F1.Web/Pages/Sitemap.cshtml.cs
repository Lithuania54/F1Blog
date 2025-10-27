using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using F1.Web.Services;

namespace F1.Web.Pages;

public class SitemapModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public SitemapModel(IWebHostEnvironment env) => _env = env;

    public IActionResult OnGet()
    {
        var baseUrl = Request.Scheme + "://" + Request.Host;
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine($"<url><loc>{baseUrl}/</loc></url>");
        var postsDir = Path.Combine(_env.ContentRootPath, "content", "posts");
        if (Directory.Exists(postsDir))
        {
            foreach(var f in Directory.GetFiles(postsDir, "*.md"))
            {
                var name = Path.GetFileNameWithoutExtension(f);
                sb.AppendLine($"<url><loc>{baseUrl}/posts/{name}</loc></url>");
            }
        }
        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml");
    }
}
