import re

file = "src/AzureTest.Persistence/Seeding/FakeDataGenerator.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
    ", DefaultWidth = 3, DefaultHeight = 2 }",
    ", DefaultWidth = 3, DefaultHeight = 2, MinWidth = 2, MinHeight = 2, IsResizable = true }"
)

content = content.replace(
    ", DefaultWidth = 4, DefaultHeight = 4 }",
    ", DefaultWidth = 4, DefaultHeight = 4, MinWidth = 2, MinHeight = 2, IsResizable = true }"
)

content = content.replace(
    ", DefaultWidth = 8, DefaultHeight = 4 }",
    ", DefaultWidth = 8, DefaultHeight = 4, MinWidth = 2, MinHeight = 2, IsResizable = true }"
)

content = content.replace(
    ", DefaultWidth = 6, DefaultHeight = 4 }",
    ", DefaultWidth = 6, DefaultHeight = 4, MinWidth = 2, MinHeight = 2, IsResizable = true }"
)

with open(file, "w") as f:
    f.write(content)
