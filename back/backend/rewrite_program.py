import sys

with open('Program.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
for i, line in enumerate(lines):
    line_num = i + 1
    
    if line_num == 222:
        new_lines.append(line)
        new_lines.append('app.MapLegacyShimsEndpoints();\n')
        continue
        
    if 223 <= line_num <= 445:
        continue
        
    if 489 <= line_num <= 512:
        continue
        
    new_lines.append(line)

with open('Program.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
