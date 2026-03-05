using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllers();

builder.Services.AddScoped<Backend.Services.ICompatibilityStrategy, Backend.Services.SimpleCompatibilityStrategy>();

var app = builder.Build();

var supportedCultures = new[] { "uk", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.MapControllers();
app.Run();