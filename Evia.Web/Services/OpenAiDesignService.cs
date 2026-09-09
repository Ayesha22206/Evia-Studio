using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Evia.Web.Services
{
    // Calls OpenAI's image generation endpoint to turn the customer's
    // fabric/color/pattern/style choices into a photorealistic preview image.
    //
    // To use this for real:
    //  1. Get an API key from https://platform.openai.com (or swap this class
    //     for a Stability AI / Replicate / Azure OpenAI client — same interface).
    //  2. Add it to appsettings.json -> "AiDesign:ApiKey" (or better, user-secrets
    //     / an environment variable so the key never gets committed to source control).
    //  3. That's it — DesignController and the Design/Index view already call
    //     IAiDesignService, so no other code needs to change.
    //public class OpenAiDesignService : IAiDesignService
    //{
    //    private readonly HttpClient _http;
    //    private readonly IConfiguration _config;
    //    private readonly ILogger<OpenAiDesignService> _logger;

    //    public OpenAiDesignService(HttpClient http, IConfiguration config, ILogger<OpenAiDesignService> logger)
    //    {
    //        _http = http;
    //        _config = config;
    //        _logger = logger;
    //    }

    //    public async Task<AiDesignResult> GenerateOutfitImageAsync(AiDesignRequest request, CancellationToken ct = default)
    //    {
    //        var apiKey = _config["AiDesign:ApiKey"];
    //        if (string.IsNullOrWhiteSpace(apiKey))
    //        {
    //            return new AiDesignResult(false, null,
    //                "AI image generation isn't configured yet. Add your API key to appsettings.json under AiDesign:ApiKey.");
    //        }

    //        var prompt = BuildPrompt(request);

    //        var body = new
    //        {
    //            model = "dall-e-3",
    //            prompt,
    //            n = 1,
    //            size = "1024x1024"
    //        };

    //        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations")
    //        {
    //            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
    //        };
    //        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    //        try
    //        {
    //            var response = await _http.SendAsync(httpRequest, ct);
    //            var json = await response.Content.ReadAsStringAsync(ct);

    //            if (!response.IsSuccessStatusCode)
    //            {
    //                _logger.LogWarning("AI image generation failed: {Status} {Body}", response.StatusCode, json);
    //                return new AiDesignResult(false, null, "The AI service couldn't generate an image right now. Please try again.");
    //            }

    //            using var doc = JsonDocument.Parse(json);
    //            var url = doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString();
    //            return new AiDesignResult(true, url, null);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "AI image generation threw an exception");
    //            return new AiDesignResult(false, null, "Unexpected error while generating the preview image.");
    //        }
    //    }

    //    // Turns the structured selections from the Design Your Own form into a
    //    // natural-language prompt for a text-to-image model.
    //    private static string BuildPrompt(AiDesignRequest r)
    //    {
    //        var patternText = r.Pattern == "none" ? "a plain, solid texture" : $"a subtle {r.Pattern} pattern";

    //        return
    //            $"A professional e-commerce fashion product photo of a {r.Style.ToLower()} outfit, " +
    //            $"made of {r.Fabric.ToLower()} fabric, in {r.Color} color, with {patternText}. " +
    //            $"Size {r.Size}. Studio lighting, plain light grey background, full garment visible, " +
    //            "front view, realistic fabric texture, no human model, no text or logos.";
    //    }
    //}
}
