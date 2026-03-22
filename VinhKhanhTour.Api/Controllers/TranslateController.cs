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

    // POST api/translate
    // Body: { "text": "nội dung tiếng Việt" }
    // Returns: { "en": "...", "zh": "...", "ja": "...", "ko": "..." }
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Text is required");

        var geminiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(geminiKey))
            return StatusCode(500, "Gemini API key chưa được cấu hình");

        var prompt = "Dịch đoạn thuyết minh du lịch ẩm thực sau từ tiếng Việt sang 4 ngôn ngữ.\n" +
                     "Giữ nguyên phong cách thuyết minh, tự nhiên, phù hợp văn hóa từng nước.\n" +
                     "Chỉ trả về JSON thuần, không markdown, không giải thích thêm.\n" +
                     "Định dạng: {\"en\": \"...\", \"zh\": \"...\", \"ja\": \"...\", \"ko\": \"...\"}\n\n" +
                     "Nội dung tiếng Việt:\n" + req.Text;

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.3 }
        };

        try
        {
            var http = _httpFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiKey}"; var json = JsonSerializer.Serialize(requestBody);
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

            // Tìm JSON trong response
            var start = rawText.IndexOf('{');
            var end = rawText.LastIndexOf('}');
            if (start >= 0 && end > start)
                rawText = rawText.Substring(start, end - start + 1);

            var translated = JsonDocument.Parse(rawText);
            return Ok(translated.RootElement);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi: {ex.Message}");
        }
    }
}

public class TranslateRequest
{
    public string Text { get; set; } = "";
}         