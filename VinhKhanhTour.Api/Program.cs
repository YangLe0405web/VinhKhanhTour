using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VinhKhanhTour.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Firebase Init ──────────────────────────────────
var base64Key = Environment.GetEnvironmentVariable("FIREBASE_KEY_BASE64");

if (!string.IsNullOrEmpty(base64Key))
{
    // Production: đọc từ biến môi trường Railway
    var keyJson = System.Text.Encoding.UTF8.GetString(
        Convert.FromBase64String(base64Key));
    var credential = GoogleCredential.FromJson(keyJson);
    FirebaseApp.Create(new AppOptions { Credential = credential });
}
else
{
    // Local: đọc từ file
    var keyPath = Path.Combine(AppContext.BaseDirectory, "firebase-key.json");
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}

builder.Services.AddCors(opt => opt.AddPolicy("CmsPolicy", p =>
    p.WithOrigins(
        "https://localhost:7110",
        "http://localhost:7110",
        "https://vinhkhanhtour-c8e3f.web.app"   // ← thêm sau khi deploy CMS
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<StorageService>();

var app = builder.Build();
app.UseCors("CmsPolicy");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();