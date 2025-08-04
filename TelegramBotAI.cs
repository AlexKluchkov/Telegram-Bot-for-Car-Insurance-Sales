using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class TelegramBotAI()
{
    private string apiKey = Environment.GetEnvironmentVariable("Telegram_Bot_AI");
    public async Task<string?> GetAiResponseAsync(string prompt)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var body = new
        {
            model = "openrouter/horizon-alpha",
            messages = new[]
            {
                new { role = "system", content = "You are an AI consultant selling car insurance. Answer briefly, professionally and clearly." },
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await http.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);

        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString();
    }


}

