import re

with open('Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('app.MapLiveKitWebhookEndpoints();\n', 'app.MapLiveKitWebhookEndpoints();\napp.MapLegacyShimsEndpoints();\n')

pattern = re.compile(r'public class TransferShimDto.*?public class SummaryShimDto \{ public required string RoomName \{ get; set; \} public required string Summary \{ get; set; \} \}', re.DOTALL)
content = pattern.sub('', content)

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
