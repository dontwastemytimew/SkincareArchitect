using Serilog;
using Backend.Services;
using Backend;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

builder.Services.AddScoped<ICompatibilityStrategy, SimpleCompatibilityStrategy>();
builder.Services.AddScoped<SkincareFacade>();

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
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.MapControllers();
app.Run();