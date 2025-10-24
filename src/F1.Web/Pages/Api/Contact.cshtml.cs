using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Services;

namespace F1.Web.Pages.Api;

public class ContactModel : PageModel
{
    private readonly ContactService _contact;

    public ContactModel(ContactService contact) => _contact = contact;

    public async Task<IActionResult> OnPostAsync([FromForm] string name, [FromForm] string email, [FromForm] string message)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            return new JsonResult(new { ok = false, message = "missing" }) { StatusCode = 400 };

        try
        {
            var msg = new ContactMessage(name ?? string.Empty, email, message, DateTime.UtcNow);
            _contact.Save(msg);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 500 };
        }
    }
}
