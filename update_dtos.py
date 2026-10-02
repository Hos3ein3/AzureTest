import re

def update_dtos(file_path):
    with open(file_path, 'r') as f:
        content = f.read()
    
    if "int X" in content: return

    # In IdeaPageDto
    content = re.sub(r'public record (IdeaPageDto.*?)\(\n(.*?)DateTime\? UpdatedAt\n\);', r'public record \1(\n\2int X,\n    int Y,\n    int Width,\n    int Height,\n    DateTime? UpdatedAt\n);', content, flags=re.DOTALL)
    
    # In IdeaSectionDto
    content = re.sub(r'public record (IdeaSectionDto.*?)\(\n(.*?)DateTime\? UpdatedAt\n\);', r'public record \1(\n\2int X,\n    int Y,\n    int Width,\n    int Height,\n    DateTime? UpdatedAt\n);', content, flags=re.DOTALL)
    
    with open(file_path, 'w') as f:
        f.write(content)

update_dtos('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Contracts/DTOs/IdeaTracker/IdeaPageDtos.cs')
update_dtos('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Contracts/DTOs/IdeaTracker/IdeaSectionDtos.cs')
