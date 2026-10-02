import os

for root, dirs, files in os.walk('src'):
    for f in files:
        if 'ClassMap' in f or 'DependencyInjection' in f:
            print(os.path.join(root, f))
