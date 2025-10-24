using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Services;

namespace F1.Web.Pages.Api;

public class SubscribeModel : PageModel
{
    private readonly NewsletterService _news;

    public SubscribeModel(NewsletterService news) => _news = news;

    public async Task<IActionResult> OnPostAsync([FromForm] string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return new JsonResult(new { ok = false, message = "Email required" }) { StatusCode = 400 };
        // Basic server-side validation
        try
        {
            var ok = _news.Subscribe(email);
            return new JsonResult(new { ok = ok });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 500 };
        }
    }
}
