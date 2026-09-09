using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Evia.Web.Services
{
    // Calls Google Gemini Imagen API to generate photorealistic outfit preview images.
    // API Key is stored in appsettings.json under GeminiDesign:ApiKey.
    // Uses the Gemini imagen-3.0-generate-002 model for high-quality fashion renders.
    //public class GeminiDesignService : IAiDesignService
    //{
    //    private readonly HttpClient _http;
    //    private readonly IConfiguration _config;
    //    private readonly ILogger<GeminiDesignService> _logger;

    //    public GeminiDesignService(HttpClient http, IConfiguration config, ILogger<GeminiDesignService> logger)
    //    {
    //        _http = http;
    //        _config = config;
    //        _logger = logger;
    //    }

    //    public async Task<AiDesignResult> GenerateOutfitImageAsync(AiDesignRequest request, CancellationToken ct = default)
    //    {
    //        var apiKey = _config["GeminiDesign:ApiKey"];
    //        if (string.IsNullOrWhiteSpace(apiKey))
    //        {
    //            return new AiDesignResult(false, null,
    //                "AI image generation is not configured. Add your Gemini API key to appsettings.json under GeminiDesign:ApiKey.");
    //        }

    //        var prompt = BuildPrompt(request);

    //        // Gemini Imagen 4 - image generation endpoint
    //        var url = $"https://generativelanguage.googleapis.com/v1beta/models/imagen-4.0-generate-001:predict?key={apiKey}";

    //        var body = new
    //        {
    //            instances = new[]
    //            {
    //                new { prompt }
    //            },
    //            parameters = new
    //            {
    //                sampleCount = 1,
    //                aspectRatio = "1:1",
    //                safetyFilterLevel = "block_few",
    //                personGeneration = "dont_allow"
    //            }
    //        };

    //        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
    //        {
    //            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
    //        };

    //        try
    //        {
    //            var response = await _http.SendAsync(httpRequest, ct);
    //            var json = await response.Content.ReadAsStringAsync(ct);

    //            if (!response.IsSuccessStatusCode)
    //            {
    //                _logger.LogWarning("Gemini Imagen API failed: {Status} {Body}", response.StatusCode, json);
    //                return new AiDesignResult(false, null, "The AI service couldn't generate an image right now. Please try again.");
    //            }

    //            using var doc = JsonDocument.Parse(json);
    //            // Gemini Imagen returns base64-encoded image data
    //            var predictions = doc.RootElement.GetProperty("predictions");
    //            if (predictions.GetArrayLength() == 0)
    //            {
    //                return new AiDesignResult(false, null, "No image was returned by the AI service.");
    //            }

    //            var base64Image = predictions[0].GetProperty("bytesBase64Encoded").GetString();
    //            var mimeType = predictions[0].TryGetProperty("mimeType", out var mt) ? mt.GetString() : "image/png";

    //            // Return as data URI so it can be displayed directly in the browser
    //            var dataUri = $"data:{mimeType};base64,{base64Image}";
    //            return new AiDesignResult(true, dataUri, null);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Gemini Imagen API threw an exception");
    //            return new AiDesignResult(false, null, "Unexpected error while generating the preview image.");
    //        }
    //    }

    //    private static string BuildPrompt(AiDesignRequest r)
    //    {
    //        var patternText = r.Pattern == "none" ? "solid plain fabric with no pattern" : $"a subtle {r.Pattern} pattern on the fabric";

    //        return
    //            $"A professional studio fashion product photograph of a {r.Style.ToLower()} clothing item, " +
    //            $"made of {r.Fabric.ToLower()} fabric, color {r.Color}, with {patternText}. " +
    //            $"Size {r.Size}. Clean white or light grey studio background, professional lighting, " +
    //            "full garment visible, front view, realistic fabric texture, no human model, no text or logos, " +
    //            "high-end e-commerce product photo style, ultra-detailed.";
    //    }
    //}
}
