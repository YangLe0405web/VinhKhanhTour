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
        policy.SetIsOriginAllowed(_ => true) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Một số trình duyệt yêu cầu cái này nếu có header đặc biệt
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache(); // Thêm dịch vụ bộ nhớ đệm
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddHttpClient();

// Cấu hình Forwarded Headers chuẩn cho Render
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// ── CHIÊU CUỐI: Custom CORS Middleware ──
// Ép buộc thêm header CORS cho mọi túi tin trả về, kể cả khi server lỗi 500
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers.Origin.ToString() ?? "*");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }

    try
    {
        await next();
    }
    catch (Exception ex) when (ex.ToString().Contains("Quota exceeded") || ex.ToString().Contains("429"))
    {
        context.Response.StatusCode = 429;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { 
            error = "Firestore Quota Exceeded", 
            message = "Hạn mức Firebase miễn phí đã hết. Vui lòng quay lại sau 24h hoặc nâng cấp gói Blaze." 
        });
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapGet("/", () => "Vinh Khanh Tour API is running (CORS Secured)!");
app.MapControllers();

app.Run();