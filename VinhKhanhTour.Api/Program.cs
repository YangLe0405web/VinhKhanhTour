using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VinhKhanhTour.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Firebase Configuration ──
var keyPath = "/etc/secrets/firebase-key.json";
if (!File.Exists(keyPath))
    keyPath = Path.Combine(AppContext.BaseDirectory, "firebase-key.json");

if (File.Exists(keyPath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.GetApplicationDefault()
        });
    }
}

// ── 2. CORS Policy ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddHttpClient();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// THỨ TỰ MIDDLEWARE QUAN TRỌNG
app.UseForwardedHeaders();

// Đưa UseCors lên trước UseRouting để xử lý Preflight tốt nhất trên một số Hosting
app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Vinh Khanh Tour API is running (CORS Standard)!");
app.MapControllers();

app.Run();