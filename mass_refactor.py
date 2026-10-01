import os
import re

def process_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    original_content = content
    changed = False

    # 1. Update ICommand<T> -> ICommand<Result<T>>
    # Example: ICommand<Guid> -> ICommand<Result<Guid>>
    # Note: if it's already ICommand<Result<T>> don't double wrap.
    def replace_icommand(match):
        t = match.group(1)
        if t.startswith('Result<') or t == 'Result':
            return f"ICommand<{t}>"
        return f"ICommand<Result<{t}>>"

    content = re.sub(r'ICommand<([a-zA-Z0-9_<>]+)>', replace_icommand, content)

    # 2. Update BaseCommandHandler<TCommand, TResponse>
    # Example: BaseCommandHandler<CreateTicketCommand, Guid> -> BaseCommandHandler<CreateTicketCommand, Result<Guid>>
    def replace_base_handler(match):
        cmd = match.group(1)
        resp = match.group(2)
        if resp.startswith('Result<') or resp == 'Result':
            return f"BaseCommandHandler<{cmd}, {resp}>"
        return f"BaseCommandHandler<{cmd}, Result<{resp}>>"

    content = re.sub(r'BaseCommandHandler<([a-zA-Z0-9_]+),\s*([a-zA-Z0-9_<>]+)>', replace_base_handler, content)

    # 3. Update ICommandHandler<TCommand, TResponse>
    def replace_ihandler(match):
        cmd = match.group(1)
        resp = match.group(2)
        if resp.startswith('Result<') or resp == 'Result':
            return f"ICommandHandler<{cmd}, {resp}>"
        return f"ICommandHandler<{cmd}, Result<{resp}>>"

    content = re.sub(r'ICommandHandler<([a-zA-Z0-9_]+),\s*([a-zA-Z0-9_<>]+)>', replace_ihandler, content)

    # 4. Update Task<T> ExecuteAsync / HandleAsync
    def replace_task(match):
        method_type = match.group(1)
        t = match.group(2)
        method_name = match.group(3)
        if t.startswith('Result<') or t == 'Result':
            return match.group(0)
        return f"{method_type} Task<Result<{t}>> {method_name}"

    content = re.sub(r'(protected override async|public async|protected override|public)\s+Task<([a-zA-Z0-9_<>]+)>\s+(ExecuteAsync|HandleAsync)', replace_task, content)

    # Update void/Task ExecuteAsync to return Task<Result>
    content = re.sub(r'(protected override async|public async|protected override|public)\s+Task\s+(ExecuteAsync|HandleAsync)', r'\1 Task<Result> \2', content)

    # We need to make sure Result wrapper is imported.
    if 'AzureTest.Application.Abstractions' not in content:
        content = 'using AzureTest.Application.Abstractions;\n' + content

    if content != original_content:
        with open(filepath, 'w') as f:
            f.write(content)
        print(f"Updated {filepath}")

for root, dirs, files in os.walk('src'):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            # Skip some base files we already fixed
            if "BaseCommandHandler.cs" in filepath or "ICommand" in filepath:
                continue
            process_file(filepath)
