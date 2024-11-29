using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HealthConnect.Services
{
    public class ZoomService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public ZoomService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _clientId = configuration["Zoom:ClientId"];
            _clientSecret = configuration["Zoom:ClientSecret"];
        }

        public async Task<string> CreateMeetingAsync(string userEmail, string topic, DateTime startTime, int duration)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var meetingData = new
            {
                topic,
                type = 2, // Scheduled meeting
                start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                duration,
                timezone = "UTC",
                settings = new
                {
                    join_before_host = true,
                    mute_upon_entry = true
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(meetingData), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"https://api.zoom.us/v2/users/{userEmail}/meetings", content);

            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            var meeting = JsonSerializer.Deserialize<ZoomMeetingResponse>(responseData);

            return meeting.JoinUrl;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://zoom.us/oauth/token");
            var authHeader = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            tokenRequest.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

            var response = await _httpClient.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<ZoomTokenResponse>(responseData);

            return tokenResponse.AccessToken;
        }
    }

    public class ZoomTokenResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
    public class ZoomMeetingResponse
    {
        public string JoinUrl { get; set; }
    }
}
