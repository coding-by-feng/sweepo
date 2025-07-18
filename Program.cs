using SweepoServer.Models;
using SweepoServer.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/sweepo-server-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure email settings
Log.Information("Configuring email settings...");
var emailConfig = new EmailConfiguration();
builder.Configuration.GetSection("EmailSettings").Bind(emailConfig);

// Load Gmail App password from environment variable for security
var envPassword = Environment.GetEnvironmentVariable("SWEEPO_FROM_EMAIL_PASSWORD");
if (!string.IsNullOrEmpty(envPassword))
{
    emailConfig.SmtpPassword = envPassword;
    Log.Information("Gmail App password loaded from environment variable SWEEPO_FROM_EMAIL_PASSWORD");
}
else if (string.IsNullOrEmpty(emailConfig.SmtpPassword))
{
    Log.Warning("No Gmail App password configured. Set SWEEPO_FROM_EMAIL_PASSWORD environment variable or SmtpPassword in appsettings.json");
}

Log.Information("Email configuration loaded: Server={Server}:{Port}, From={FromEmail}, Recipients={RecipientCount}, PasswordConfigured={PasswordConfigured}", 
    emailConfig.SmtpServer, emailConfig.SmtpPort, emailConfig.FromEmail, emailConfig.RecipientEmails.Count, !string.IsNullOrEmpty(emailConfig.SmtpPassword));
builder.Services.AddSingleton(emailConfig);

// Register services
Log.Information("Registering application services...");
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
Log.Information("Application services registered successfully");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "https://sweepo-ui.vercel.app",
            "https://sweepo.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => new { 
    service = "Sweepo Server", 
    version = "1.0.0", 
    status = "running",
    timestamp = DateTime.UtcNow 
});

try
{
    Log.Information("Starting Sweepo Server");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
