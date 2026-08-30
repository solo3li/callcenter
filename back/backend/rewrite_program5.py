with open('Program.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for line in lines:
    if 'app.MapLiveKitWebhookEndpoints();' in line:
        new_lines.append(line)
        new_lines.append('app.MapLegacyShimsEndpoints();\n')
        continue
    
    if 'Legacy AI Worker Compat Shims' in line:
        skip = True
        continue
        
    if skip and 'app.MapHub<CallHub>("/hubs/call");' in line:
        skip = False
        new_lines.append(line)
        continue
        
    if 'public class TransferShimDto' in line:
        skip = True
        continue
        
    if skip and 'public class SummaryShimDto' in line:
        skip = False
        continue
        
    if not skip:
        new_lines.append(line)

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
