import os

def find_file(filename, search_path):
    for root, dirs, files in os.walk(search_path):
        if filename in files:
            print(os.path.join(root, filename))

find_file('AzureTestDbContext.cs', 'src')
