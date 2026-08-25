using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Services
{
    public class EmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public EmbeddingService(HttpClient http)
        {
            _http = http;
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
            _model = "text-embedding-3-small";
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return Enumerable.Range(0, 1536).Select(_ => (float)Random.Shared.NextDouble()).ToArray();
            }

            var body = new { input = text, model = _model };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
            return embedding.EnumerateArray().Select(e => e.GetSingle()).ToArray();
        }

        public string FormatVectorLiteral(float[] embedding)
        {
            return "[" + string.Join(",", embedding.Select(f => f.ToString("F6"))) + "]";
        }
    }
}