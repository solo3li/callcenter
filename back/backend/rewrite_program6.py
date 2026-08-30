import re

with open('Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace MapLiveKitWebhookEndpoints to add MapLegacyShimsEndpoints
content = content.replace('app.MapLiveKitWebhookEndpoints();\n', 'app.MapLiveKitWebhookEndpoints();\napp.MapLegacyShimsEndpoints();\n')

# Replace Legacy AI Worker Compat Shims block
start_marker = '// ── Legacy AI Worker Compat Shims'
end_marker = 'app.MapHub<CallHub>("/hubs/call");'
if start_marker in content and end_marker in content:
    start_idx = content.find(start_marker)
    end_idx = content.find(end_marker)
    if start_idx != -1 and end_idx != -1:
        content = content[:start_idx] + content[end_idx:]

# Remove Legacy DTOs
dtos_start = 'public class TransferShimDto'
dtos_end = 'public class SummaryShimDto { public required string RoomName { get; set; } public required string Summary { get; set; } }'
if dtos_start in content and dtos_end in content:
    start_idx = content.find(dtos_start)
    end_idx = content.find(dtos_end) + len(dtos_end)
    if start_idx != -1 and end_idx != -1:
        content = content[:start_idx] + content[end_idx:]

# Replace DI services
di_start = 'builder.Services.AddSingleton<HttpClient>();'
di_end = 'var app = builder.Build();'
if di_start in content and di_end in content:
    new_di = """builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<backend.Infrastructure.Services.EmbeddingService>();
builder.Services.AddSingleton<backend.Modules.Identity.Services.TokenService>();
builder.Services.AddScoped<backend.Modules.Identity.Services.ApiKeyValidationService>();
builder.Services.AddScoped<backend.Modules.Configuration.Services.ActionService>();
builder.Services.AddScoped<backend.Modules.Configuration.Services.CallConfigurationService>();
builder.Services.AddScoped<backend.Modules.CallOperations.Services.CallTransferService>();
builder.Services.AddScoped<backend.Modules.CallOperations.Services.CallHandoffService>();
builder.Services.AddScoped<backend.Modules.CallOperations.Services.InboundRoutingService>();
builder.Services.AddScoped<backend.Modules.CallOperations.Services.CallRecordingService>();
builder.Services.AddScoped<backend.Modules.CallOperations.Services.LiveKitService>();
builder.Services.AddScoped<backend.Modules.Analytics.Services.StatsService>();
builder.Services.AddSingleton<backend.Infrastructure.Services.StorageService>();

"""
    start_idx = content.find(di_start)
    end_idx = content.find(di_end)
    content = content[:start_idx] + new_di + content[end_idx:]

# Clean up any missed 'using backend.Services;'
content = content.replace('using backend.Services;\n', 'using backend.Infrastructure.Services;\n')

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
