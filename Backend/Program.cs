using Serilog;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers();

builder.Services.AddScoped<ICompatibilityStrategy, SimpleCompatibilityStrategy>();

builder.Services.AddSingleton<SystemSettings>(sp => 
    SystemSettings.GetInstance(sp.GetRequiredService<ILogger<SystemSettings>>()));

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:5000") 
            .AllowAnyMethod() 
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

var supportedCultures = new[] { "uk", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.MapControllers();
app.Run();