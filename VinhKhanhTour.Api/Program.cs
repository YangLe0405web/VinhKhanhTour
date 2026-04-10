using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VinhKhanhTour.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Dùng CreateEmptyBuilder để tránh lỗi inotify (status 134) trên Render Linux
var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args
});

// 1. Cấu hình Kestrel (Bắt buộc khi dùng EmptyBuilder)
builder.WebHost.UseKestrel();

// 2. Cấu hình nguồn App Settings (Tắt reloadOnChange để tránh lỗi inotify)
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// 3. Thêm các dịch vụ cơ bản
builder.Services.AddRouting();
builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// ── 1. Services Configuration ──
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<StorageService>();

// ── 2. CORS Policy ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── 3. JWT Authentication ──
var jwtKey = builder.Configuration["Jwt:Secret"] ?? "vinhkhanhtour_super_secret_key_2026_!@#";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Bật Swagger cho cả môi trường Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vinh Khanh Tour API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ── 4. Initialize Admin Account ──
// FirebaseApp.DefaultInstance sẽ được kiểm tra bên trong FirestoreService nếu cần,
// nhưng ta cần đảm bảo InitializeAdminAsync chạy khi startup.
using (var scope = app.Services.CreateScope())
{
    var firestore = scope.ServiceProvider.GetRequiredService<FirestoreService>();
    await firestore.InitializeAdminAsync();
}

app.MapGet("/", () => "Vinh Khanh Tour API is running (Fix Render Inotify v2)!");
app.MapControllers();

app.Run();