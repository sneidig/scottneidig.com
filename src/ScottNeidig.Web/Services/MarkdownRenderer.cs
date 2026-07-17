using Markdig;

namespace ScottNeidig.Web.Services;

public class MarkdownRenderer : IMarkdownRenderer
{
    // Built once and reused: the pipeline is immutable and thread-safe, so there's no reason
    // to rebuild it per render. Advanced extensions add tables, auto-links, and the like.
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public string ToHtml(string markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? "" : Markdown.ToHtml(markdown, Pipeline);
}
