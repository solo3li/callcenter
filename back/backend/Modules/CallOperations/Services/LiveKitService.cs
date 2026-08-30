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

            if (_apiKey == "devkey" || _apiSecret == "secret")
                Console.WriteLine("WARNING: LiveKitService is using fallback API credentials. Set LIVEKIT_API_KEY and LIVEKIT_API_SECRET environment variables.");
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

        // ── v0 SIP calling ────────────────────────────────────────────────

        /// <summary>
        /// Explicitly assigns a named agent worker to a room (auto-dispatch is
        /// disabled for named agents), carrying routing metadata.
        /// </summary>
        public async Task<string> CreateAgentDispatch(string roomName, string agentName, string metadataJson)
        {
            var token = GenerateSuperAdminToken();
            var reqBody = new { room = roomName, agent_name = agentName, metadata = metadataJson };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_host}/twirp/livekit.AgentDispatchService/CreateAgentDispatch")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task RemoveParticipant(string roomName, string identity)
        {
            var token = GenerateSuperAdminToken();
            var reqBody = new { room = roomName, identity };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_host}/twirp/livekit.RoomService/RemoveParticipant")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LiveKit] RemoveParticipant({roomName},{identity}) -> {(int)response.StatusCode}: {body}");
            }
        }

        /// <summary>
        /// Dials a SIP target through an outbound trunk and bridges it into the
        /// room as a participant (destination transfer leg).
        /// </summary>
        public async Task<string?> CreateSipParticipant(string trunkId, string callTo,
            string roomName, string identity, string? displayName)
        {
            var token = GenerateSuperAdminToken();
            var reqBody = new
            {
                sip_trunk_id = trunkId,
                sip_call_to = callTo,
                room_name = roomName,
                participant_identity = identity,
                participant_name = displayName ?? identity,
                play_ringback = true
            };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_host}/twirp/livekit.SIP/CreateSIPParticipant")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LiveKit] CreateSipParticipant failed ({(int)response.StatusCode}): {body}");
                return null;
            }
            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.TryGetProperty("participant_identity", out var idEl)
                ? idEl.GetString()
                : identity;
        }

        /// <summary>Returns identities of participants currently in the room.</summary>
        public async Task<List<string>> ListParticipantIdentities(string roomName)
        {
            var result = new List<string>();
            try
            {
                var token = GenerateSuperAdminToken();
                var reqBody = new { room = roomName };
                var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"{_host}/twirp/livekit.RoomService/ListParticipants")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return result;
                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("participants", out var arr) && arr.ValueKind == JsonValueKind.Array)
                    foreach (var p in arr.EnumerateArray())
                        if (p.TryGetProperty("identity", out var idEl))
                        {
                            var id = idEl.GetString();
                            if (!string.IsNullOrEmpty(id)) result.Add(id);
                        }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiveKit] ListParticipants failed: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Validates a LiveKit server webhook (HS256-signed JWT) and returns the
        /// signed payload (event/room/participant claims), or null when invalid.
        /// </summary>
        public JsonDocument? ValidateWebhook(string? authorizationHeader, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(authorizationHeader)) return null;
                var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorizationHeader["Bearer ".Length..].Trim()
                    : authorizationHeader.Trim();
                var payloadJson = JWT.Decode(token, Encoding.UTF8.GetBytes(_apiSecret), JwsAlgorithm.HS256);
                return JsonDocument.Parse(payloadJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiveKit] Webhook validation failed: {ex.Message}");
                return null;
            }
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

        private string GenerateSuperAdminToken()
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
                        { "room", "*" },
                        { "roomRecord", true },
                        { "sip", true }
                    }
                }
            };

            return JWT.Encode(payload, Encoding.UTF8.GetBytes(_apiSecret), JwsAlgorithm.HS256);
        }
    }
}