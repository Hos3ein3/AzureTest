import re

file = "src/AzureTest.MVC/Views/Dashboard/Builder.cshtml"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""                                    existingWidgets.push({
                                        x: @c.X,
                                        y: @c.Y,
                                        w: @c.Width,
                                        h: @c.Height,
                                        id: '@rawCompInfo.Id',
                                        content: `""",
"""                                    existingWidgets.push({
                                        x: @c.X,
                                        y: @c.Y,
                                        w: @c.Width,
                                        h: @c.Height,
                                        id: '@rawCompInfo.Id',
                                        minW: @(rawCompInfo.MinWidth > 0 ? rawCompInfo.MinWidth.ToString() : "undefined"),
                                        maxW: @(rawCompInfo.MaxWidth > 0 ? rawCompInfo.MaxWidth.ToString() : "undefined"),
                                        minH: @(rawCompInfo.MinHeight > 0 ? rawCompInfo.MinHeight.ToString() : "undefined"),
                                        maxH: @(rawCompInfo.MaxHeight > 0 ? rawCompInfo.MaxHeight.ToString() : "undefined"),
                                        noResize: @(rawCompInfo.IsResizable ? "false" : "true"),
                                        content: `"""
)

with open(file, "w") as f:
    f.write(content)
