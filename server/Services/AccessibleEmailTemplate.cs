namespace Tailwind.Mail.Services;

/// <summary>
/// Provides accessible HTML email template wrapping with screen reader support
/// and semantic structure for improved accessibility.
/// </summary>
public static class AccessibleEmailTemplate
{
    /// <summary>
    /// Wraps email content in an accessible HTML template with proper structure,
    /// ARIA attributes, and live regions for dynamic content.
    /// </summary>
    /// <param name="subject">The email subject for the title tag</param>
    /// <param name="preview">Preview text for email clients</param>
    /// <param name="bodyHtml">The main HTML content of the email</param>
    /// <param name="lang">Language code for the email (default: "en")</param>
    /// <returns>Complete accessible HTML email</returns>
    public static string Wrap(string subject, string preview, string bodyHtml, string lang = "en")
    {
        return $@"<!DOCTYPE html>
<html lang=""{lang}"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>{EscapeHtml(subject)}</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:AllowPNG/>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
    <style type=""text/css"">
        /* Accessibility-focused styles */
        body {{
            margin: 0;
            padding: 0;
            min-width: 100%;
            background-color: #f4f4f4;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            font-size: 16px;
            line-height: 1.5;
            color: #333333;
        }}
        
        /* High contrast link styles for accessibility */
        a {{
            color: #0066cc;
            text-decoration: underline;
        }}
        
        a:hover, a:focus {{
            color: #004499;
            text-decoration: underline;
        }}
        
        /* Focus visible styles for keyboard navigation */
        a:focus {{
            outline: 2px solid #0066cc;
            outline-offset: 2px;
        }}
        
        /* Ensure proper heading hierarchy */
        h1, h2, h3, h4, h5, h6 {{
            color: #1a1a1a;
            margin-top: 0;
        }}
        
        h1 {{ font-size: 24px; line-height: 1.2; }}
        h2 {{ font-size: 20px; line-height: 1.3; }}
        h3 {{ font-size: 18px; line-height: 1.4; }}
        
        /* Screen reader only class */
        .sr-only {{
            position: absolute;
            width: 1px;
            height: 1px;
            padding: 0;
            margin: -1px;
            overflow: hidden;
            clip: rect(0, 0, 0, 0);
            white-space: nowrap;
            border: 0;
        }}
        
        /* Skip link for keyboard navigation */
        .skip-link {{
            position: absolute;
            top: -40px;
            left: 0;
            background: #0066cc;
            color: #ffffff;
            padding: 8px;
            z-index: 100;
        }}
        
        .skip-link:focus {{
            top: 0;
        }}
        
        /* Email container */
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
        }}
        
        /* Content wrapper */
        .content {{
            padding: 20px;
        }}
        
        /* Accessible button styles */
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #0066cc;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
        }}
        
        .button:hover, .button:focus {{
            background-color: #004499;
        }}
        
        /* Responsive adjustments */
        @media screen and (max-width: 600px) {{
            .email-container {{
                width: 100% !important;
            }}
            .content {{
                padding: 15px !important;
            }}
        }}
        
        /* High contrast mode support */
        @media (prefers-contrast: high) {{
            body {{
                background-color: #ffffff;
                color: #000000;
            }}
            a {{
                color: #0000ee;
            }}
        }}
        
        /* Reduced motion support */
        @media (prefers-reduced-motion: reduce) {{
            * {{
                animation: none !important;
                transition: none !important;
            }}
        }}
    </style>
</head>
<body>
    <!-- Hidden preview text for email clients -->
    <div style=""display:none;font-size:1px;color:#f4f4f4;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;"" aria-hidden=""true"">
        {EscapeHtml(preview)}
    </div>
    
    <!-- Skip to main content link for keyboard users -->
    <a href=""#main-content"" class=""skip-link sr-only"">Skip to main content</a>
    
    <!-- Main email wrapper with semantic structure -->
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #f4f4f4;"">
        <tr>
            <td align=""center"" style=""padding: 20px 10px;"">
                <!-- Email container -->
                <table role=""presentation"" class=""email-container"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" style=""background-color: #ffffff;"">
                    <!-- Header section -->
                    <tr>
                        <td style=""padding: 20px; text-align: center; background-color: #0066cc;"">
                            <header role=""banner"">
                                <h1 style=""color: #ffffff; margin: 0; font-size: 24px;"">{EscapeHtml(subject)}</h1>
                            </header>
                        </td>
                    </tr>
                    
                    <!-- Main content section with live region for dynamic updates -->
                    <tr>
                        <td class=""content"">
                            <main id=""main-content"" role=""main"" aria-label=""Email content"" aria-live=""polite"" aria-atomic=""true"">
                                <article>
                                    {bodyHtml}
                                </article>
                            </main>
                        </td>
                    </tr>
                    
                    <!-- Footer section -->
                    <tr>
                        <td style=""padding: 20px; text-align: center; background-color: #f4f4f4; border-top: 1px solid #dddddd;"">
                            <footer role=""contentinfo"">
                                <nav aria-label=""Email actions"">
                                    <p style=""margin: 0; font-size: 14px; color: #666666;"">
                                        <span class=""sr-only"">Unsubscribe from this mailing list: </span>
                                        <a href=""{{{{unsubscribe_url}}}}"" style=""color: #666666;"" aria-label=""Unsubscribe from mailing list"">Unsubscribe</a>
                                    </p>
                                </nav>
                                <p style=""margin: 10px 0 0 0; font-size: 12px; color: #999999;"">
                                    © Tailwind Traders. All rights reserved.
                                </p>
                            </footer>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// Escapes HTML special characters to prevent XSS
    /// </summary>
    private static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;");
    }
    
    /// <summary>
    /// Creates an accessible status message div for dynamic content updates.
    /// Use this when content changes dynamically to announce updates to screen readers.
    /// </summary>
    /// <param name="message">The status message to announce</param>
    /// <param name="isAssertive">If true, uses assertive live region (for important updates)</param>
    /// <returns>HTML for accessible status announcement</returns>
    public static string CreateStatusMessage(string message, bool isAssertive = false)
    {
        var ariaLive = isAssertive ? "assertive" : "polite";
        return $@"<div role=""status"" aria-live=""{ariaLive}"" aria-atomic=""true"" class=""sr-only"">{EscapeHtml(message)}</div>";
    }
    
    /// <summary>
    /// Creates an accessible alert for important messages that require immediate attention.
    /// </summary>
    /// <param name="message">The alert message</param>
    /// <returns>HTML for accessible alert</returns>
    public static string CreateAlert(string message)
    {
        return $@"<div role=""alert"" aria-live=""assertive"" style=""padding: 15px; background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; color: #856404;"">{EscapeHtml(message)}</div>";
    }
}
