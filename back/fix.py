import re

with open("backend/Migrations/Patches/000_base.sql", "r", encoding="utf-8") as f:
    text = f.read()

def to_snake(match):
    word = match.group(0)
    if word.isupper(): return word
    if word.islower(): return word
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", word)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).lower()

lines = text.split("\n")
new_lines = []
for line in lines:
    if line.startswith("--"): 
        new_lines.append(line)
        continue
    new_line = re.sub(r"\b[A-Z][a-z0-9]+[A-Za-z0-9]*\b", to_snake, line)
    new_lines.append(new_line)

with open("backend/Migrations/Patches/000_base.sql", "w", encoding="utf-8") as f:
    f.write("\n".join(new_lines))

