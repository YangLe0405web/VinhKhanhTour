using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslateController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public TranslateController(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Nội dung cần dịch không được để trống.");

        // ── 1. Lấy API Key (Ưu tiên Config > Secret File) ──────────
        string geminiKey = _config["Gemini:ApiKey"] ?? "";

        // Nếu trong config trống, mới tìm trong file (Render Secrets hoặc local)
        if (string.IsNullOrEmpty(geminiKey))
        {
            var renderSecretPath = "/etc/secrets/gemini-key.txt";
            var localSecretPath = Path.Combine(AppContext.BaseDirectory, "gemini-key.txt");

            if (System.IO.File.Exists(renderSecretPath))
                geminiKey = (await System.IO.File.ReadAllTextAsync(renderSecretPath)).Trim();
            else if (System.IO.File.Exists(localSecretPath))
                geminiKey = (await System.IO.File.ReadAllTextAsync(localSecretPath)).Trim();
        }

        // Kiểm tra cơ bản xem key có hợp lệ không (tránh dùng nhầm file code C#)
        if (string.IsNullOrEmpty(geminiKey) || geminiKey.Contains("namespace") || geminiKey.Length < 20)
            return StatusCode(500, "Lỗi: Không tìm thấy Gemini API Key hợp lệ. Vui lòng kiểm tra lại cấu hình.");
        // ─────────────────────────────────────────────────────────────

        var prompt = "Dịch đoạn thuyết minh du lịch ẩm thực sau từ tiếng Việt sang 4 ngôn ngữ.\n" +
                     "Giữ nguyên phong cách thuyết minh, tự nhiên.\n" +
                     "Chỉ trả về JSON thuần: {\"en\": \"...\", \"zh\": \"...\", \"ja\": \"...\", \"ko\": \"...\"}\n\n" +
                     "Nội dung: " + req.Text;

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        try
        {
            using var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(60); // Tăng timeout lên 1 phút do dịch đa ngôn ngữ có thể chậm

            // Đổi lại sang endpoint v1beta do v1 đang bị lỗi 503 (High Demand) tại Google
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiKey}";

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Gemini API báo lỗi ({response.StatusCode}): {err}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);
            
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
                return StatusCode(500, "Gemini không trả về kết quả dịch.");

            var rawText = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Trích xuất JSON từ phản hồi của AI
            var start = rawText.IndexOf('{');
            var end = rawText.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                rawText = rawText.Substring(start, end - start + 1);
                return Ok(JsonSerializer.Deserialize<object>(rawText));
            }
            
            return StatusCode(500, "Không trích xuất được định dạng JSON từ phản hồi của AI.");
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, "Yêu cầu dịch bị quá hạn (Timeout). Vui lòng thử lại với nội dung ngắn hơn.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}

public class TranslateRequest { public string Text { get; set; } = ""; }