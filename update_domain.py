import re

def add_xy(file_path):
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Check if X already exists
    if "public int X " in content: return
    
    prop_block = """
    // Layout
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 4;
"""
    
    # Insert before public bool IsDeleted
    content = content.replace("public bool IsDeleted", prop_block + "\n    public bool IsDeleted")
    with open(file_path, 'w') as f:
        f.write(content)

add_xy('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Domain/Documents/IdeaPage.cs')
add_xy('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Domain/Documents/IdeaSection.cs')
