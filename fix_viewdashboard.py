import re

file = "src/AzureTest.MVC/Views/Dashboard/ViewDashboard.cshtml"
with open(file, "r") as f:
    content = f.read()

# Replace the fetch logic for JSON config components
old_fetch = """                                                        fetch(config.endpointUrl)
                                                            .then(res => res.json())
                                                            .then(data => {"""

new_fetch = """                                                        let fetchUrl = config.endpointUrl;
                                                        const viewName = '@Html.Raw(rawComp.ViewName ?? "")';
                                                        if (viewName) {
                                                            fetchUrl += (fetchUrl.includes('?') ? '&' : '?') + 'viewName=' + encodeURIComponent(viewName);
                                                            fetch(fetchUrl)
                                                                .then(res => res.text())
                                                                .then(html => {
                                                                    document.getElementById('title-@rawComp.Id').style.display = 'none';
                                                                    document.getElementById('widget-@rawComp.Id').innerHTML = html;
                                                                })
                                                                .catch(err => {
                                                                    console.error('Error fetching view', err);
                                                                    document.getElementById('widget-@rawComp.Id').innerHTML = '<p class="text-red-500 text-xs p-2">Error loading view.</p>';
                                                                });
                                                            return;
                                                        }

                                                        fetch(config.endpointUrl)
                                                            .then(res => res.json())
                                                            .then(data => {"""

content = content.replace(old_fetch, new_fetch)

with open(file, "w") as f:
    f.write(content)
