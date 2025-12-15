using System.Security.Claims;
using F1.Web.Data;
using F1.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace F1.Web.Controllers;

public class F1GameController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public F1GameController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/F1Game")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "F1 Game";

        var bestResult = await GetUserBestResultAsync();
        var model = new F1GameViewModel
        {
            IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
            BestResult = Map(bestResult)
        };

        return View(model);
    }

    [HttpPost("/F1Game/SaveBestResult")]
    public async Task<IActionResult> SaveBestResult([FromBody] SaveBestResultRequest request)
    {
        var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!isAuthenticated || string.IsNullOrWhiteSpace(userId))
        {
            return Ok(new { saved = false, isAuthenticated, bestResult = (BestResultViewModel?)null });
        }

        var existing = await _dbContext.UserBestResults.FirstOrDefaultAsync(r => r.UserId == userId);
        var incomingLap = request?.BestLapTime;
        var hasValidLap = incomingLap.HasValue && !double.IsNaN(incomingLap.Value) && incomingLap.Value > 0;

        var updated = false;
        if (hasValidLap && (existing == null || incomingLap!.Value < existing.BestLapTime || existing.BestLapTime <= 0))
        {
            if (existing == null)
            {
                existing = new UserBestResult { UserId = userId };
                _dbContext.UserBestResults.Add(existing);
            }

            existing.BestLapTime = incomingLap!.Value;
            existing.TotalTime = request?.TotalTime;
            existing.TrackKey = request?.TrackKey;
            existing.TrackName = request?.TrackName;
            existing.UpdatedAt = DateTime.UtcNow;
            updated = true;
        }

        if (updated)
        {
            await _dbContext.SaveChangesAsync();
        }

        var bestResult = Map(existing);
        return Ok(new { saved = updated, isAuthenticated, bestResult });
    }

    private Task<UserBestResult?> GetUserBestResultAsync()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false)) return Task.FromResult<UserBestResult?>(null);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Task.FromResult<UserBestResult?>(null);
        return _dbContext.UserBestResults.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId);
    }

    private static BestResultViewModel? Map(UserBestResult? entity)
    {
        if (entity == null) return null;
        return new BestResultViewModel
        {
            BestLapTime = entity.BestLapTime,
            TotalTime = entity.TotalTime,
            TrackKey = entity.TrackKey,
            TrackName = entity.TrackName,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public class SaveBestResultRequest
    {
        public double? BestLapTime { get; set; }
        public double? TotalTime { get; set; }
        public string? TrackKey { get; set; }
        public string? TrackName { get; set; }
    }
}
