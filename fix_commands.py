import re

def fix_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    original = content
    # For HandleAsync returning Result
    content = re.sub(
        r'(public async Task<Result> HandleAsync\([^{]+{.*?)(?=\s*})',
        lambda m: m.group(1) + ('' if 'return' in m.group(1) else '\n        return Result.Success();'),
        content,
        flags=re.DOTALL
    )

    # For ExecuteAsync returning Result (not Result<T>)
    content = re.sub(
        r'(protected override async Task<Result> ExecuteAsync\([^{]+{.*?)(?=\s*})',
        lambda m: m.group(1) + ('' if 'return' in m.group(1) else '\n        return Result.Success();'),
        content,
        flags=re.DOTALL
    )

    # For ExecuteAsync returning Result<T> when it just returns a variable or value
    # E.g. return t.Id; -> return Result.Success(t.Id);
    # This is tricky with regex, we can just look for `return [^;]+;` and wrap it if it's inside ExecuteAsync returning Result<T>
    # It's safer to just do it manually if there are only 20 errors.
    
    if content != original:
        with open(filepath, 'w') as f:
            f.write(content)

import os
for root, dirs, files in os.walk('src/AzureTest.Application'):
    for file in files:
        if file.endswith('.cs'):
            fix_file(os.path.join(root, file))
