import re

file = "src/AzureTest.MVC/Views/Dashboard/Builder.cshtml"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""                                 data-w="@comp.DefaultWidth" 
                                 data-h="@comp.DefaultHeight"
                                 data-img="@comp.ImagePath"
                                 ondragstart="handleDragStart(event)">""",
"""                                 data-w="@comp.DefaultWidth" 
                                 data-h="@comp.DefaultHeight"
                                 data-img="@comp.ImagePath"
                                 data-minw="@comp.MinWidth"
                                 data-maxw="@comp.MaxWidth"
                                 data-minh="@comp.MinHeight"
                                 data-maxh="@comp.MaxHeight"
                                 data-resizable="@comp.IsResizable.ToString().ToLower()"
                                 ondragstart="handleDragStart(event)">"""
)

with open(file, "w") as f:
    f.write(content)
