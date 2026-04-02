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

// ── 3. Pipeline ──────────────────────────────────────────
var app = builder.Build();

// Thạnh: Đặt CORS ở ĐẦU TIÊN để xử lý Preflight nhanh nhất
app.UseCors("CmsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Render đã xử lý HTTPS ở tầng Load Balancer, tắt cái này để tránh lỗi Redirect CORS
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();