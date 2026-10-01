sed -i '' '/<a asp-area="" asp-controller="JobCrawler"/i\
                <li>\
                    <a asp-area="" asp-controller="IdeaTracker" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdeaTracker")">\
                        <i class="fa-solid fa-brain w-6 @GetActiveIconClasses("IdeaTracker")"><\/i>\
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Idea Tracker<\/span>\
                    <\/a>\
                <\/li>\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml
