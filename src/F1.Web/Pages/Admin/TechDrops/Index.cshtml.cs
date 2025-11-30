using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Data;
using F1.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace F1.Web.Pages.Admin.TechDrops;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public TechDrop Input { get; set; } = new();

    public List<TechDrop> Items { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Items = await _db.TechDrops
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        Items = Items
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(cancellationToken);
            return Page();
        }

        _db.TechDrops.Add(Input);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToPage();
    }
}
