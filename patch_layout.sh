# Add id to aside and transition class
sed -i '' 's/<aside class="w-64 bg-slate-900/<aside id="sidebar" class="w-64 transition-all duration-300 bg-slate-900/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml

# Add a toggle button in the header of aside
sed -i '' 's/<span class="text-xl font-bold tracking-wider">Tracker<span class="text-brand-500">Pro<\/span><\/span>/<span class="sidebar-text text-xl font-bold tracking-wider overflow-hidden whitespace-nowrap">Tracker<span class="text-brand-500">Pro<\/span><\/span><button id="sidebarToggle" class="ml-auto text-slate-400 hover:text-white"><i class="fa-solid fa-bars"><\/i><\/button>/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml

# Replace ml-2 with ml-2 sidebar-text overflow-hidden whitespace-nowrap in all nav spans
sed -i '' 's/<span class="ml-2 font-medium">/<span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml

