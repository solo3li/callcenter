using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jose; // jose-jwt

var builder = WebApplication.CreateBuilder(args);

// Add CORS policies to ensure the API is reachable
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/token", (string identity = null) => {
    if (string.IsNullOrEmpty(identity)) {
        identity = "web-user-" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    // Default development keys used in docker-compose.yml
    var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var roomName = "sip-room"; // The room AI joins

    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var exp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();

    var payload = new Dictionary<string, object>
    {
        { "iss", apiKey },
        { "sub", identity },
        { "name", identity },
        { "nbf", now },
        { "exp", exp },
        { "video", new Dictionary<string, object>
            {
                { "roomJoin", true },
                { "room", roomName },
                { "canPublish", true },
                { "canSubscribe", true }
            }
        }
    };

    var secretKey = Encoding.UTF8.GetBytes(apiSecret);
    var tokenString = JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
    
    // We return localhost for local testing, but ideally this should be the public IP or domain of the LiveKit server.
    // Given the network_mode is host, we can return the relative path if reverse proxied, or absolute URL.
    var host = app.Configuration["LIVEKIT_URL"] ?? "ws://127.0.0.1:7880";

    return Results.Json(new { token = tokenString, url = host });
});

app.Run("http://0.0.0.0:5000");
