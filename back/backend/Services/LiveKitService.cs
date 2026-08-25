using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Jose;

namespace backend.Services
{
    public class LiveKitService
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _host;
        private readonly HttpClient _http;

        public LiveKitService(HttpClient http)
        {
            _http = http;
            _apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
            _apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
            _host = Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880";
        }

        public string GenerateToken(string identity, string roomName, bool canPublish, bool canSubscribe)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var payload = new Dictionary<string, object>
            {
                { "iss", _apiKey },
                { "sub", identity },
                { "name", identity },
                { "nbf", now },
                { "exp", now + 7200 },
                { "video", new Dictionary<string, object>
                    {
                        { "roomJoin", true },
                        { "room", roomName },
                        { "canPublish", canPublish },
                        { "canSubscribe", canSubscribe }
                    }
                }
            };

            return JWT.Encode(payload, Encoding.UTF8.GetBytes(_apiSecret), JwsAlgorithm.HS256);
        }

        public async Task<string> CreateRoom(string roomName)
        {
            var token = GenerateAdminToken(roomName);
            var reqBody = new { name = roomName };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_host}/twirp/livekit.RoomService/CreateRoom")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteRoom(string roomName)
        {
            var token = GenerateAdminToken(roomName);
            var reqBody = $"{{\"room\":\"{roomName}\"}}";
            var content = new StringContent(reqBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_host}/twirp/livekit.RoomService/DeleteRoom")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> StartEgress(string roomName)
        {
            var token = GenerateAdminToken("*");
            var reqBody = new
            {
                room_name = roomName,
                audio_only = true,
                file = new { filepath = $"/recordings/{roomName}.ogg" }
            };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_host}/twirp/livekit.Egress/StartRoomCompositeEgress")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("egress_id").GetString() ?? string.Empty;
        }

        public async Task<string> StopEgress(string roomName)
        {
            var token = GenerateAdminToken(roomName);
            var reqBody = new { room_name = roomName };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_host}/twirp/livekit.Egress/StopEgress")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> StopEgressById(string egressId)
        {
            var token = GenerateAdminToken("*");
            var reqBody = $"{{\"egress_id\":\"{egressId}\"}}";
            var content = new StringContent(reqBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_host}/twirp/livekit.Egress/StopEgress")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private string GenerateAdminToken(string roomName)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var payload = new Dictionary<string, object>
            {
                { "iss", _apiKey },
                { "sub", _apiKey },
                { "nbf", now },
                { "exp", now + 300 },
                { "video", new Dictionary<string, object>
                    {
                        { "roomAdmin", true },
                        { "room", roomName },
                        { "roomRecord", true }
                    }
                }
            };

            return JWT.Encode(payload, Encoding.UTF8.GetBytes(_apiSecret), JwsAlgorithm.HS256);
        }
    }
}