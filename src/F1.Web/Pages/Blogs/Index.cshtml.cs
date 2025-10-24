using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Services;

namespace F1.Web.Pages.Blogs;

public class IndexModel : PageModel
{
    private readonly MarkdownService _md;
    public List<RenderedPost> Posts { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public bool HasMore { get; set; }

    public IndexModel(MarkdownService md) => _md = md;

    public void OnGet(int pageNumber = 1, int shuffle = 0)
    {
        PageNumber = pageNumber;
        var all = _md.GetAllPosts().ToList();
        if (shuffle == 1)
        {
            var rnd = new Random();
            all = all.OrderBy(x => rnd.Next()).ToList();
        }

        Posts = all.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        HasMore = all.Count > PageNumber * PageSize;
    }
}
