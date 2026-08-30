import sys
import re

with open('Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace MapLiveKitWebhookEndpoints to add MapLegacyShimsEndpoints
content = content.replace(
    'app.MapLiveKitWebhookEndpoints();\n',
    'app.MapLiveKitWebhookEndpoints();\napp.MapLegacyShimsEndpoints();\n'
)

# Remove legacy endpoints
start_marker = '// ── Legacy AI Worker Compat Shims ──────────────────────────────────────────────'
end_marker = 'app.MapHub<CallHub>("/hubs/call");'

start_idx = content.find(start_marker)
end_idx = content.find(end_marker)

if start_idx != -1 and end_idx != -1:
    content = content[:start_idx] + content[end_idx:]
else:
    print("Could not find markers for legacy endpoints.")

# Remove DTOs
dtos_start = 'public class TransferShimDto'
dtos_end = 'public class SummaryShimDto { public required string RoomName { get; set; } public required string Summary { get; set; } }'

start_idx = content.find(dtos_start)
end_idx = content.find(dtos_end) + len(dtos_end)

if start_idx != -1 and end_idx != -1:
    content = content[:start_idx] + content[end_idx:]
else:
    print("Could not find markers for DTOs.")

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
