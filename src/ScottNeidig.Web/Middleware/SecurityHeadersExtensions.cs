namespace ScottNeidig.Web.Middleware;

/// <summary>
/// Response security headers. HSTS is handled separately by UseHsts (it belongs to the HTTPS
/// story); this covers the rest: CSP, nosniff, referrer and framing policy.
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // CSP: everything defaults to same-origin. Scripts allow 'unsafe-inline' because the
            // deferred-CSS trick uses an inline onload handler and the JSON-LD is an inline
            // block; there are no external or user-generated scripts, so the exposure is small.
            // Styles stay strict ('self') since every stylesheet is a file, no inline styles.
            // Images allow data: for any inlined SVG. Framing is denied outright.
            headers.ContentSecurityPolicy =
                "default-src 'self'; " +
                "base-uri 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'; " +
                "img-src 'self' data:; " +
                "style-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "object-src 'none'";

            // Don't let a browser second-guess a declared content type (an XSS vector).
            headers["X-Content-Type-Options"] = "nosniff";

            // Send the origin but not the path to other sites; full URL stays same-origin.
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // frame-ancestors above is the modern control; this covers older browsers.
            headers["X-Frame-Options"] = "DENY";

            // Turn off powerful features this site never uses.
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            await next();
        });
}
