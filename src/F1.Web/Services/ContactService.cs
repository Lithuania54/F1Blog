using System.Text.Json;

namespace F1.Web.Services;

public record ContactMessage(string Name, string Email, string Message, DateTime ReceivedUtc);

public class ContactService
{
    private readonly string _filePath;
    private readonly ILogger<ContactService> _logger;
    private readonly object _lock = new();

    public ContactService(IWebHostEnvironment env, ILogger<ContactService> logger)
    {
        _logger = logger;
        var storage = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storage);
        _filePath = Path.Combine(storage, "messages.json");
        if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
    }

    public void Save(ContactMessage message)
    {
        lock (_lock)
        {
            var list = JsonSerializer.Deserialize<List<ContactMessage>>(File.ReadAllText(_filePath)) ?? new();
            list.Add(message);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
        _logger.LogInformation("Saved contact message from {Email}", message.Email);
    }
}
