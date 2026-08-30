import re

with open('Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix the Legacy markers
pattern = re.compile(r'// .*? Legacy AI Worker Compat Shims .*?app\.MapHub<CallHub>\("/hubs/call"\);', re.DOTALL)
content = pattern.sub('app.MapHub<CallHub>("/hubs/call");', content)

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
