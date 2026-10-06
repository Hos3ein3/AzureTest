import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

# 1. Inject Preferences Query
inj_target = """@inject Microsoft.AspNetCore.Hosting.IWebHostEnvironment _HostEnv"""
inj_replacement = """@inject Microsoft.AspNetCore.Hosting.IWebHostEnvironment _HostEnv
@inject AzureTest.Application.Messaging.IQueryHandler<AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery, AzureTest.Domain.Documents.UserPreference> _PrefQuery
@{
    bool isDarkMode = false;
    if (_CurrentUser != null && _CurrentUser.Id != Guid.Empty)
    {
        try {
            var pref = _PrefQuery.HandleAsync(new AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery(_CurrentUser.Id), default).GetAwaiter().GetResult();
            isDarkMode = pref?.IsDarkMode ?? false;
        } catch {}
    }
}"""
if "@inject AzureTest.Application.Messaging.IQueryHandler" not in content:
    content = content.replace(inj_target, inj_replacement)

# 2. Add dark class to html
if 'class="h-full bg-gray-50"' in content:
    content = content.replace('class="h-full bg-gray-50"', 'class="h-full bg-gray-50 @(isDarkMode ? "dark" : "")"')

# 3. Add tailwind dark mode
if 'darkMode: \'class\'' not in content:
    content = content.replace('theme: {', 'darkMode: \'class\',\n        theme: {')

# 4. Replace bell icon and add dark mode and profile link
nav_target = """                <button class="text-slate-400 hover:text-brand-500 transition-colors">
                    <i class="fa-regular fa-bell text-lg"></i>
                </button>
                <a asp-action="Manage" asp-controller="Dashboard" class="text-slate-400 hover:text-brand-500 transition-colors">
                    
                    <i class="fab fa-windows"></i>
                </a>
                <form asp-controller="Auth" asp-action="Logout" method="post" class="inline">
                    <button type="submit" class="text-slate-400 hover:text-brand-500 transition-colors" title="Logout">
                        <i class="fa-solid fa-arrow-right-from-bracket text-lg"></i>
                    </button>
                </form>"""

nav_replacement = """                <!-- Notification Bell -->
                <div class="relative" id="notificationDropdownContainer">
                    <button id="notificationBtn" onclick="document.getElementById('notificationDropdown').classList.toggle('hidden')" class="text-slate-400 hover:text-brand-500 transition-colors relative">
                        <i class="fa-regular fa-bell text-lg"></i>
                        <span id="notificationBadge" class="absolute -top-1 -right-1 bg-red-500 text-white text-[10px] rounded-full h-4 w-4 flex items-center justify-center hidden">0</span>
                    </button>
                    <!-- Dropdown -->
                    <div id="notificationDropdown" class="hidden absolute right-0 mt-2 w-80 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg shadow-xl z-50 flex flex-col">
                        <div class="p-3 border-b border-slate-100 dark:border-slate-700 font-semibold text-slate-700 dark:text-slate-200">Notifications</div>
                        <div id="notificationList" class="max-h-64 overflow-y-auto p-2 space-y-1">
                            <div class="text-sm text-slate-500 dark:text-slate-400 text-center py-4" id="noNotifications">No new notifications</div>
                        </div>
                    </div>
                </div>
                
                <button onclick="toggleGlobalDarkMode()" class="text-slate-400 hover:text-brand-500 transition-colors" title="Toggle Dark Mode">
                    <i class="fa-solid @(isDarkMode ? "fa-sun" : "fa-moon") text-lg" id="darkModeIcon"></i>
                </button>

                <a asp-action="Manage" asp-controller="Dashboard" class="text-slate-400 hover:text-brand-500 transition-colors" title="Dashboards">
                    <i class="fab fa-windows text-lg"></i>
                </a>
                
                <a asp-action="Index" asp-controller="Profile" class="text-slate-400 hover:text-brand-500 transition-colors flex items-center" title="My Profile">
                    @if(!string.IsNullOrEmpty(_CurrentUser.ProfileImage)){
                        <img src="@_CurrentUser.ProfileImage" class="w-7 h-7 rounded-full object-cover border border-slate-200" />
                    } else {
                        <i class="fa-solid fa-circle-user text-xl"></i>
                    }
                </a>

                <form asp-controller="Auth" asp-action="Logout" method="post" class="inline">
                    <button type="submit" class="text-slate-400 hover:text-brand-500 transition-colors" title="Logout">
                        <i class="fa-solid fa-arrow-right-from-bracket text-lg"></i>
                    </button>
                </form>"""

if "id=\"notificationDropdownContainer\"" not in content:
    content = content.replace(nav_target, nav_replacement)

# 5. Add scripts at the end of body
scripts = """
    <!-- SignalR & Notifications -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.12/signalr.min.js"></script>
    <script>
        let unreadCount = 0;
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .build();

        connection.on("ReceiveTicketNotification", (title, message) => {
            // Play sound
            const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
            audio.play().catch(e => console.log("Audio play blocked by browser", e));

            // Update UI
            document.getElementById('noNotifications')?.remove();
            unreadCount++;
            const badge = document.getElementById('notificationBadge');
            badge.innerText = unreadCount;
            badge.classList.remove('hidden');

            const list = document.getElementById('notificationList');
            const item = document.createElement('div');
            item.className = "p-2 rounded bg-blue-50 dark:bg-blue-900/30 text-sm border-l-4 border-blue-500 mb-1 cursor-pointer";
            item.innerHTML = `<p class="font-semibold text-blue-700 dark:text-blue-300">${title}</p><p class="text-slate-600 dark:text-slate-400 text-xs">${message}</p>`;
            item.onclick = () => { window.location.href = '/Ticket'; };
            list.prepend(item);
        });

        connection.start().catch(err => console.error(err.toString()));

        // Hide dropdown when clicking outside
        document.addEventListener('click', function(e) {
            const container = document.getElementById('notificationDropdownContainer');
            if (container && !container.contains(e.target)) {
                document.getElementById('notificationDropdown').classList.add('hidden');
            }
        });

        // Dark Mode Toggle
        function toggleGlobalDarkMode() {
            const html = document.documentElement;
            const isDark = html.classList.toggle('dark');
            const icon = document.getElementById('darkModeIcon');
            if (isDark) {
                icon.classList.remove('fa-moon');
                icon.classList.add('fa-sun');
            } else {
                icon.classList.remove('fa-sun');
                icon.classList.add('fa-moon');
            }

            // Save to UserPreferences
            fetch('/UserPreference/Update', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({ IsDarkMode: isDark })
            });
        }
    </script>
"""

if "signalR.HubConnectionBuilder" not in content:
    content = content.replace("</body>", scripts + "\n</body>")

with open(file, "w") as f:
    f.write(content)

print("Layout Patched")
