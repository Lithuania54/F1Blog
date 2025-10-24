using System.Text.Json;

namespace F1.Web.Services;

public class NewsletterService
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public NewsletterService(IWebHostEnvironment env)
    {
        var storage = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storage);
        _filePath = Path.Combine(storage, "newsletter.json");
        if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
    }

    public bool Subscribe(string email)
    {
        lock (_lock)
        {
            var list = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_filePath)) ?? new();
            if (list.Contains(email, StringComparer.OrdinalIgnoreCase)) return false;
            list.Add(email);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
            return true;
        }
    }
}
