import re

file = "src/AzureTest.MVC/Views/Component/Index.cshtml"
with open(file, "r") as f:
    content = f.read()

# Replace Grid and add Type select
content = content.replace(
"""    <form asp-action="Index" method="get">
        <div class="grid grid-cols-1 md:grid-cols-6 gap-4">
            <div class="md:col-span-2">
                <label class="block text-sm font-medium text-slate-700 mb-1">Search</label>""",
"""    <form asp-action="Index" method="get">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div class="col-span-1">
                <label class="block text-sm font-medium text-slate-700 mb-1">Search</label>"""
)
content = content.replace(
"""            <div class="md:col-span-2">
                <label class="block text-sm font-medium text-slate-700 mb-1">Category</label>""",
"""            <div class="col-span-1">
                <label class="block text-sm font-medium text-slate-700 mb-1">Category</label>"""
)
content = content.replace(
"""            <div class="md:col-span-2 flex items-end justify-start gap-2">
                <button type="submit" class="inline-flex justify-center items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-lg text-white bg-brand-600 hover:bg-brand-700 focus:outline-none transition-colors">""",
"""            <div class="col-span-1">
                <label class="block text-sm font-medium text-slate-700 mb-1">Type</label>
                <select name="type" class="block w-full pl-3 pr-10 py-2 border border-slate-300 rounded-lg leading-5 bg-white focus:outline-none focus:ring-1 focus:ring-brand-500 focus:border-brand-500 sm:text-sm">
                    <option value="">All Types</option>
                    <option value="Standard" selected="@(ViewBag.CurrentType == "Standard" ? "selected" : null)">Standard View</option>
                    <option value="RawHtml" selected="@(ViewBag.CurrentType == "RawHtml" ? "selected" : null)">Raw HTML</option>
                    <option value="JsonConfig" selected="@(ViewBag.CurrentType == "JsonConfig" ? "selected" : null)">JSON Config</option>
                </select>
            </div>
            
            <div class="col-span-1 flex items-end justify-start gap-2">
                <button type="submit" class="inline-flex justify-center items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-lg text-white bg-brand-600 hover:bg-brand-700 focus:outline-none transition-colors">"""
)

# Update pagination/sort links
content = content.replace('asp-route-category="@ViewBag.Category"', 'asp-route-category="@ViewBag.Category" asp-route-type="@ViewBag.CurrentType"')

with open(file, "w") as f:
    f.write(content)
