with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'r') as f:
    content = f.read()

import re
# Ensure every a tag is wrapped in exactly one li tag. 
# It's easier to just do a manual replace for the malformed section.
malformed_section = """                <li>
                    <a asp-area="" asp-controller="IdeaTracker" asp-action="Index" data-title="Idea Tracker" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdeaTracker")">
                        <i class="fa-solid fa-brain w-6 @GetActiveIconClasses("IdeaTracker")"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Idea Tracker</span>
                    </a>
                </li>

                    <a asp-area="" asp-controller="JobCrawler" asp-action="Index" data-title="Job Crawler" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("JobCrawler")">"""

fixed_section = """                <li>
                    <a asp-area="" asp-controller="IdeaTracker" asp-action="Index" data-title="Idea Tracker" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdeaTracker")">
                        <i class="fa-solid fa-brain w-6 @GetActiveIconClasses("IdeaTracker")"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Idea Tracker</span>
                    </a>
                </li>
                <li>
                    <a asp-area="" asp-controller="JobCrawler" asp-action="Index" data-title="Job Crawler" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("JobCrawler")">"""

content = content.replace(malformed_section, fixed_section)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'w') as f:
    f.write(content)
