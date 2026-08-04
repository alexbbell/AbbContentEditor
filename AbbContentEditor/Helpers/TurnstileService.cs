using AbbContentEditor.Models.Account;

namespace AbbContentEditor.Services
{
    public interface ITurnstileService
    {
        Task<bool> VerifyTokenAsync(string token, string? remoteIp = null);
    }

    public class TurnstileService : ITurnstileService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TurnstileService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> VerifyTokenAsync(string token, string? remoteIp = null)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secretKey = _configuration["CloudflareTurnstile:SecretKey"];

            var postData = new Dictionary<string, string>
            {
                { "secret", secretKey ?? "" },
                { "response", token }
            };

            if (!string.IsNullOrEmpty(remoteIp))
            {
                postData.Add("remoteip", remoteIp);
            }

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>();
            return result?.Success ?? false;
        }
    }
}