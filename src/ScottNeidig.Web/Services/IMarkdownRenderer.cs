namespace ScottNeidig.Web.Services;

public interface IMarkdownRenderer
{
    /// <summary>
    /// Renders markdown to HTML. Posts are written only by the single admin, so the input is
    /// trusted; this doesn't sanitize, and the output is meant to be rendered raw.
    /// </summary>
    string ToHtml(string markdown);
}
