import re

# Read the broken file
with open('test.yaml', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix patterns:
# 1. Find strings like: - "[Field] = "value""
#    Replace with: - '[Field] = "value"'
# 2. Handle multi-line strings properly

lines = content.split('\n')
fixed_lines = []

for line in lines:
    # Check if this is an expected formula line
    if line.strip().startswith('- "') and line.strip().endswith('""'):
        # This line has broken quotes
        # Extract the content between the first " and last "
        match = re.match(r'(\s*)- "(.*?)""', line)
        if match:
            indent = match.group(1)
            formula = match.group(2)
            # Wrap in single quotes since formula contains double quotes
            fixed_line = f"{indent}- '{formula}'"
            fixed_lines.append(fixed_line)
        else:
            fixed_lines.append(line)
    elif ' = ""' in line or ' != ""' in line:
        # Fix empty string comparisons
        # These should be: [Field] = ""  (which is valid)
        # But YAML interprets "" as empty, so we need to handle this
        # Actually, the original had '', which should become ""
        # In YAML with single quotes: - '[Field] = ""'
        fixed_lines.append(line.replace(' = ""', ' = ""').replace(' != ""', ' != ""'))
    else:
        fixed_lines.append(line)

# Write fixed content
with open('test_fixed.yaml', 'w', encoding='utf-8') as f:
    f.write('\n'.join(fixed_lines))

print("Fixed YAML written to test_fixed.yaml")
