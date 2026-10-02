import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', 'r') as f:
    content = f.read()

# I need to inject IIdeaSectionQueryUnit _sectionQueries.
# Wait, I can just inject it from HttpContext.RequestServices instead of changing constructor to avoid breaking things simply.

patch = """
        // Get all sections for this page
        var sectionQueries = HttpContext.RequestServices.GetService(typeof(IIdeaSectionQueryUnit)) as IIdeaSectionQueryUnit;
        if (sectionQueries != null) {
            var sectionsResult = await sectionQueries.GetIdeaSections.HandleAsync(new GetIdeaSectionsQuery(_currentUser.Id, id), ct);
            if (sectionsResult.IsSuccess) ViewBag.Sections = sectionsResult.Value;
        }

        return View(result.Value);
"""

content = content.replace("return View(result.Value);", patch)
with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', 'w') as f:
    f.write(content)
