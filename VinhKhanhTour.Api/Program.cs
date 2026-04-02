using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VinhKhanhTour.Api.Services;

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

// ── 2. Cấu hình CORS (Phải thật chuẩn để CMS không bị chặn) ──
builder.Services.AddCors(opt => opt.AddPolicy("CmsPolicy", p =>
    p.AllowAnyOrigin()
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

app.UseSwagger();
app.UseSwaggerUI();

// 1. Routing trước
app.UseRouting();

// 2. CORS phải đặt TRƯỚC HttpsRedirection để preflight OPTIONS không bị redirect
app.UseCors("CmsPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();