using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Tailwind.Mail.Middleware;

/// <summary>
/// Middleware to add security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        
        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self';");
        
        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // XSS Protection
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        
        // Referrer Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Set cookie policy
        context.Response.Headers.Append("Set-Cookie", "Path=/; HttpOnly; Secure; SameSite=Strict");
        
        // Permissions Policy
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        
        await _next(context);
    }
}

/// <summary>
/// Extension method to add the security headers middleware to the pipeline
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
