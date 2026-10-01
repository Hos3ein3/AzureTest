import os
import re

dirs_to_check = ['src/AzureTest.Application', 'src/AzureTest.MVC', 'src/AzureTest.API']

for d in dirs_to_check:
    if not os.path.exists(d):
        continue
    for root, dirs, files in os.walk(d):
        for file in files:
            if file.endswith('.cs'):
                filepath = os.path.join(root, file)
                with open(filepath, 'r') as f:
                    content = f.read()
                
                # Check if it uses Result but doesn't have the using
                if 'Result' in content and 'using AzureTest.Application.Abstractions;' not in content:
                    content = 'using AzureTest.Application.Abstractions;\n' + content
                    with open(filepath, 'w') as f:
                        f.write(content)
