using System.Text;
using Markdig;

namespace F1.Web.Services;

public class MarkdownService
{
    private readonly string _postsFolder;
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService(IWebHostEnvironment env)
    {
        _postsFolder = Path.Combine(env.ContentRootPath, "content", "posts");
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        Directory.CreateDirectory(_postsFolder);
    }

    public IEnumerable<RenderedPost> GetAllPosts()
    {
        var files = Directory.GetFiles(_postsFolder, "*.md").OrderByDescending(f => f);
        foreach (var f in files)
        {
            yield return Load(f);
        }
    }

    public RenderedPost Load(string filePath)
    {
        var text = File.ReadAllText(filePath, Encoding.UTF8);
        // Very small frontmatter parser (yaml style)
        var meta = new Dictionary<string, string>();
        var body = text;
        if (text.StartsWith("---"))
        {
            var end = text.IndexOf("---", 3);
            if (end > 0)
            {
                var fm = text.Substring(3, end - 3).Trim();
                foreach (var line in fm.Split('\n'))
                {
                    var idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        var k = line.Substring(0, idx).Trim();
                        var v = line.Substring(idx + 1).Trim().Trim('"');
                        meta[k] = v;
                    }
                }
                body = text.Substring(end + 3).Trim();
            }
        }

        var html = Markdig.Markdown.ToHtml(body, _pipeline);
        return new RenderedPost
        {
            Title = meta.GetValueOrDefault("title") ?? Path.GetFileNameWithoutExtension(filePath),
            Date = DateTime.TryParse(meta.GetValueOrDefault("date"), out var d) ? d : File.GetLastWriteTime(filePath),
            Excerpt = meta.GetValueOrDefault("excerpt"),
            Html = html,
            Source = Path.GetFileName(filePath),
            Slug = Path.GetFileNameWithoutExtension(filePath),
            ImageUrl = meta.GetValueOrDefault("image")
        };
    }
}

public record RenderedPost
{
    public string Title { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string? Excerpt { get; init; }
    public string Html { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty; // filename without extension
    public string? ImageUrl { get; set; }
}
