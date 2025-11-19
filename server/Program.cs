
using Microsoft.OpenApi.Models;
using Tailwind.Data;
using Tailwind.Mail.Services;

//load up the config from env and appsettings
var config = Viper.Config("Integration");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDb, DB>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
if(config.Get("SEND_WORKER") == "local"){
    builder.Services.AddHostedService<BackgroundSend>();
}
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.2",
        Title = "📧 Tailwind Traders Mail Services API",
        Description = @"
## Welcome to Tailwind Traders Mail Services! 🚀

A powerful, modern email service platform for transactional and bulk email sending.

### Features
- **Transactional Emails**: Send individual emails triggered by user actions
- **Bulk Campaigns**: Send email broadcasts to your entire contact list or segments
- **Contact Management**: Manage subscribers with easy opt-in/opt-out functionality
- **Click Tracking**: Monitor engagement with link click tracking
- **Tag-based Organization**: Organize contacts with flexible tagging

### Getting Started
1. Use the `/signup` endpoint to add new contacts
2. Create broadcasts with the admin endpoints
3. Track engagement with click tracking
4. Manage your list with bulk operations

For more information, visit our [GitHub repository](https://github.com/scubaninja/dotNET-mail-demo).",
        Contact = new OpenApiContact
        {
            Name = "Rob Conery, Aaron Wislang, and the Tailwind Traders Team",
            Url = new Uri("https://tailwindtraders.dev")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });
});


var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Tailwind Traders Mail API";
    options.InjectStylesheet("/css/swagger-custom.css");
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
    options.ShowExtensions();
});
var conn = DB.Postgres();
Tailwind.Mail.Api.PublicRoutes.MapRoutes(app);
Tailwind.Mail.Api.Admin.BroadcastRoutes.MapRoutes(app);
Tailwind.Mail.Api.Admin.ContactRoutes.MapRoutes(app);
Tailwind.Mail.Api.Admin.BulkOperationRoutes.MapRoutes(app);

app.Run();

//this is for tests
public partial class Program { }