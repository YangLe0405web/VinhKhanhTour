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
            return BadRequest("Text is required");

        // ── Lấy API Key từ Secret File ──────────────────────
        string geminiKey = "";

        // 1. Kiểm tra đường dẫn trên Render
        var renderSecretPath = "/etc/secrets/gemini-key.txt";
        // 2. Kiểm tra đường dẫn tại thư mục dự án (Local)
        var localSecretPath = Path.Combine(AppContext.BaseDirectory, "gemini-key.txt");

        if (System.IO.File.Exists(renderSecretPath))
        {
            geminiKey = (await System.IO.File.ReadAllTextAsync(renderSecretPath)).Trim();
        }
        else if (System.IO.File.Exists(localSecretPath))
        {
            geminiKey = (await System.IO.File.ReadAllTextAsync(localSecretPath)).Trim();
        }
        else
        {
            // 3. Dự phòng cuối cùng: đọc từ appsettings.json
            geminiKey = _config["Gemini:ApiKey"] ?? "";
        }

        if (string.IsNullOrEmpty(geminiKey))
            return StatusCode(500, "Lỗi: Không tìm thấy Gemini API Key trong Secret File hoặc cấu hình.");
        // ──────────────────────────────────────────────────

        var prompt = "Dịch đoạn thuyết minh du lịch ẩm thực sau từ tiếng Việt sang 4 ngôn ngữ.\n" +
                     "Giữ nguyên phong cách thuyết minh, tự nhiên, phù hợp văn hóa từng nước.\n" +
                     "Chỉ trả về JSON thuần, không markdown, không giải thích thêm.\n" +
                     "Định dạng: {\"en\": \"...\", \"zh\": \"...\", \"ja\": \"...\", \"ko\": \"...\"}\n\n" +
                     "Nội dung: " + req.Text;

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var http = _httpFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash:generateContent?key={geminiKey}";
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Gemini lỗi: {err}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(resultJson);
            var rawText = result.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Tìm JSON trong response (đề phòng Gemini trả về thêm text dư thừa)
            var start = rawText.IndexOf('{');
            var end = rawText.LastIndexOf('}');
            if (start >= 0 && end > start)
                rawText = rawText.Substring(start, end - start + 1);

            var translated = JsonDocument.Parse(rawText);
            return Ok(translated.RootElement);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}

public class TranslateRequest
{
    public string Text { get; set; } = "";
}