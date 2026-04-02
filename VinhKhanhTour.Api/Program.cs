using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VinhKhanhTour.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// ── 1. Cấu hình Firebase ──────────────────────────────────
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

// ── 1.5 Cấu hình ForwardedHeaders (Rất quan trọng cho Render/Proxy) ──
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


// ── 2. Cấu hình CORS (Dùng Default Policy cho chắc chắn) ──
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(origin => true) // Chấp nhận mọi origin, cực kỳ hiệu quả
     .AllowAnyHeader()
     .AllowAnyMethod()
));



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký các Service
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// 0. Forwarded Headers đầu tiên để lấy đúng IP/Scheme
app.UseForwardedHeaders();

app.UseRouting();

// 1. CORS đặt SAU Routing nhưng TRƯỚC MapControllers/Authorization
app.UseCors(); 

app.UseAuthorization();

app.MapGet("/", () => "Vinh Khanh Tour API is running!");
app.MapControllers();
app.Run();
