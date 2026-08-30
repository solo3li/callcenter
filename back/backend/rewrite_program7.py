import re

with open('Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

di_start = 'builder.Services.AddSingleton<HttpClient>();'
di_end = 'var app = builder.Build();'
if di_start in content and di_end in content:
    new_di = """builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddSingleton<backend.Modules.Identity.Services.TokenService>();
builder.Services.AddScoped<backend.Modules.Identity.Services.ApiKeyValidationService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<CallConfigurationService>();
builder.Services.AddScoped<CallTransferService>();
builder.Services.AddScoped<CallHandoffService>();
builder.Services.AddScoped<InboundRoutingService>();
builder.Services.AddScoped<CallRecordingService>();
builder.Services.AddScoped<LiveKitService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddSingleton<StorageService>();

"""
    start_idx = content.find(di_start)
    end_idx = content.find(di_end)
    content = content[:start_idx] + new_di + content[end_idx:]

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
