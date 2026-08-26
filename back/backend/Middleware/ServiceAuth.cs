using System;

namespace backend.Middleware;

/// <summary>
/// Shared-service authentication for machine-to-machine calls
/// (python-ai-worker -> backend shims, LiveKit -> webhook).
/// When BACKEND_SERVICE_TOKEN is unset (dev), access is allowed.
/// </summary>
public static class ServiceAuth
{
    public const string HeaderName = "X-Service-Token";

    public static bool IsConfiguredOrValid(HttpContext http)
    {
        var expected = Environment.GetEnvironmentVariable("BACKEND_SERVICE_TOKEN");
        if (string.IsNullOrEmpty(expected)) return true;
        var provided = http.Request.Headers[HeaderName].FirstOrDefault();
        return string.Equals(provided, expected, StringComparison.Ordinal);
    }

    public static bool IsConfigured => !string.IsNullOrEmpty(
        Environment.GetEnvironmentVariable("BACKEND_SERVICE_TOKEN"));
}
