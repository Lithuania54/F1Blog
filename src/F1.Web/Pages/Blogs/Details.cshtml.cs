using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Services;

namespace F1.Web.Pages.Blogs;

public class DetailsModel : PageModel
{
    private readonly MarkdownService _md;
    public RenderedPost? Post { get; set; }

    public DetailsModel(MarkdownService md) => _md = md;

    public void OnGet(string? slug)
    {
        if (string.IsNullOrEmpty(slug)) return;
        var file = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "content", "posts", slug + ".md");
        if (System.IO.File.Exists(file))
        {
            Post = _md.Load(file);
        }
    }
}
