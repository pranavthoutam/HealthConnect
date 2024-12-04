using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class DailyCoService
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<string> GenerateDailyRoomLink(string roomName, DateTime startTime, DateTime expiryTime)
    {
        var apiUrl = "https://api.daily.co/v1/rooms";
        var apiKey = "4a4fc26ec92516206c8229b9dd214817dd3df57f82c4d1a329d37241d9923127";  // Replace with your API key from Daily.co

        var requestBody = new
        {
            properties = new
            {
                enable_screen_sharing = true,
                enable_chat = true,
                max_participants = 10,
                start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                end_time = expiryTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }
        };

        // Create HttpRequestMessage
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
        };

        // Add Authorization header to the request
        requestMessage.Headers.Add("Authorization", "Bearer " + apiKey);

        var response = await client.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();

        // Parse the response and get the meeting link (room URL)
        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
        return jsonResponse.url;
    }
}
