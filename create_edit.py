icons = ["fa-lightbulb", "fa-rocket", "fa-brain", "fa-bolt", "fa-fire", "fa-star", "fa-heart", "fa-gem", "fa-crown", "fa-cube", "fa-code", "fa-bug", "fa-flask", "fa-chart-line", "fa-bullseye", "fa-trophy", "fa-compass", "fa-map", "fa-book", "fa-graduation-cap", "fa-pen", "fa-palette", "fa-camera", "fa-music", "fa-video", "fa-gamepad", "fa-mug-hot", "fa-pizza-slice", "fa-plane", "fa-car", "fa-bicycle", "fa-leaf", "fa-tree", "fa-paw", "fa-sun", "fa-moon", "fa-cloud", "fa-snowflake", "fa-wind", "fa-droplet", "fa-envelope", "fa-comment", "fa-users", "fa-briefcase", "fa-building", "fa-house", "fa-shop", "fa-cart-shopping", "fa-wallet", "fa-credit-card"]

icon_html = "".join([f'<button type="button" class="p-2 border border-slate-200 rounded hover:bg-brand-50 hover:text-brand-600 hover:border-brand-300 transition text-lg" onclick="selectIcon(\\\'fa-solid {icon}\\\')"><i class="fa-solid {icon}"></i></button>' for icon in icons])

def get_template(is_edit):
    model_type = "UpdateIdeaDto" if is_edit else "CreateIdeaDto"
    title_var = "Edit Idea" if is_edit else "New Idea"
    h1_text = "Edit Idea" if is_edit else "Create a New Idea"
    p_text = 'Update properties for "@Model.Title".' if is_edit else "Setup the properties and write your idea."
    action = "Edit" if is_edit else "Create"
    back_btn = f'<a asp-action="Details" asp-route-id="@Model.Id" class="text-brand-600 font-medium hover:underline"><i class="fa-solid fa-arrow-left mr-1"></i> Back to Page</a>' if is_edit else f'<a asp-action="Index" class="text-brand-600 font-medium hover:underline"><i class="fa-solid fa-arrow-left mr-1"></i> Back</a>'
    
    id_input = '<input type="hidden" asp-for="Id" />' if is_edit else ''
    
    # Initialize ThemeColor and Icon defaults for Create
    default_color = "" if is_edit else 'value="#3b82f6"'
    default_icon = "" if is_edit else 'value="fa-solid fa-lightbulb"'
    
    return f"""@using AzureTest.Contracts.DTOs.IdeaTracker
@using AzureTest.Domain.Documents
@model {model_type}
@{{
    ViewData["Title"] = "{title_var}";
}}

<div class="max-w-5xl mx-auto py-10 px-6">
    <div class="mb-8 flex justify-between items-center">
        <div>
            <h1 class="text-3xl font-bold text-slate-900">{h1_text}</h1>
            <p class="text-slate-500 mt-1">{p_text}</p>
        </div>
        {back_btn}
    </div>

    <form asp-action="{action}" method="post" id="ideaForm" class="space-y-6 bg-white p-6 md:p-8 rounded-xl border border-slate-200 shadow-sm">
        {id_input}
        <input type="hidden" asp-for="ParentId" />
        
        <div asp-validation-summary="ModelOnly" class="text-red-500 text-sm mb-4"></div>

        <div>
            <label asp-for="Title" class="block text-sm font-medium text-slate-700 mb-1">Title</label>
            <input asp-for="Title" class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-brand-500 focus:border-brand-500 text-2xl font-bold" placeholder="Untitled" />
            <span asp-validation-for="Title" class="text-xs text-red-500 mt-1"></span>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
                <label asp-for="Icon" class="block text-sm font-medium text-slate-700 mb-1">Icon</label>
                <div class="relative">
                    <input type="hidden" asp-for="Icon" id="iconInput" {default_icon} />
                    <button type="button" onclick="document.getElementById('iconPicker').classList.toggle('hidden')" class="w-full flex items-center justify-between px-4 py-2 border border-slate-300 rounded-lg bg-white hover:bg-slate-50 focus:ring-brand-500 focus:border-brand-500 text-left">
                        <span class="flex items-center text-slate-700 font-medium">
                            <span class="w-8 h-8 rounded bg-slate-100 flex items-center justify-center mr-3 text-lg">
                                <i id="iconPreview" class="@(Model?.Icon ?? "fa-solid fa-lightbulb")"></i>
                            </span>
                            Choose Icon
                        </span>
                        <i class="fa-solid fa-chevron-down text-slate-400"></i>
                    </button>
                    
                    <div id="iconPicker" class="hidden absolute top-full left-0 z-20 mt-1 w-full p-3 bg-white border border-slate-200 shadow-xl rounded-lg max-h-64 overflow-y-auto">
                        <div class="grid grid-cols-6 sm:grid-cols-8 gap-2">
                            {icon_html}
                        </div>
                        <div class="mt-3 pt-3 border-t border-slate-100">
                            <label class="text-xs font-semibold text-slate-500 mb-1 block">Custom Class (FontAwesome)</label>
                            <input type="text" class="w-full px-3 py-1.5 border border-slate-300 rounded text-sm" placeholder="fa-solid fa-robot" onchange="selectIcon(this.value)" />
                        </div>
                    </div>
                </div>
            </div>
            <div>
                <label asp-for="ThemeColor" class="block text-sm font-medium text-slate-700 mb-1">Theme Color</label>
                <div class="flex items-center gap-3">
                    <input type="color" asp-for="ThemeColor" class="h-10 w-16 p-0.5 border border-slate-300 rounded cursor-pointer bg-white" {default_color} />
                    <span class="text-sm text-slate-500">Select a primary color for the cover banner.</span>
                </div>
            </div>
        </div>

        <div>
            <label asp-for="CoverImage" class="block text-sm font-medium text-slate-700 mb-1">Cover Image URL (Optional)</label>
            <input asp-for="CoverImage" class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-brand-500 focus:border-brand-500 text-sm" placeholder="https://..." />
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
                <label asp-for="Status" class="block text-sm font-medium text-slate-700 mb-1">Status</label>
                <select asp-for="Status" asp-items="Html.GetEnumSelectList<IdeaStatus>()" class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-brand-500 focus:border-brand-500 text-sm"></select>
            </div>
            <div>
                <label asp-for="Priority" class="block text-sm font-medium text-slate-700 mb-1">Priority</label>
                <select asp-for="Priority" asp-items="Html.GetEnumSelectList<IdeaPriority>()" class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-brand-500 focus:border-brand-500 text-sm"></select>
            </div>
        </div>

        <!-- TinyMCE Rich Text Editor -->
        <div class="mt-8 border-t border-slate-200 pt-6">
            <label asp-for="Content" class="block text-lg font-bold text-slate-900 mb-1">Page Content</label>
            <p class="text-xs text-slate-500 mb-4">A powerful Word-like editor. Drag and drop images directly into the text area!</p>
            
            <textarea asp-for="Content" id="Content"></textarea>
        </div>

        <div class="pt-4 border-t border-slate-100 flex justify-end gap-3 sticky bottom-0 bg-white py-4 z-10 shadow-[0_-10px_10px_-10px_rgba(0,0,0,0.05)]">
            <button type="submit" id="saveBtn" class="px-6 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-medium rounded-lg shadow-sm transition flex items-center">
                <i class="fa-solid fa-save mr-2"></i> Save Page
            </button>
        </div>
    </form>
</div>

@section Scripts {{
    <!-- TinyMCE CDN -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/tinymce/6.8.2/tinymce.min.js" referrerpolicy="origin"></script>

    <script>
        function selectIcon(iconClass) {{
            if (!iconClass) return;
            document.getElementById('iconInput').value = iconClass;
            document.getElementById('iconPreview').className = iconClass;
            document.getElementById('iconPicker').classList.add('hidden');
        }}

        document.addEventListener('DOMContentLoaded', () => {{
            
            // Close icon picker when clicking outside
            document.addEventListener('click', (e) => {{
                const picker = document.getElementById('iconPicker');
                const btn = picker.previousElementSibling;
                if (!picker.contains(e.target) && !btn.contains(e.target)) {{
                    picker.classList.add('hidden');
                }}
            }});

            // Initialize TinyMCE
            tinymce.init({{
                selector: '#Content',
                plugins: 'advlist autolink lists link image charmap preview anchor searchreplace visualblocks code fullscreen insertdatetime media table help wordcount',
                toolbar: 'undo redo | formatselect | ' +
                'bold italic forecolor backcolor | alignleft aligncenter ' +
                'alignright alignjustify | bullist numlist outdent indent | ' +
                'image media table | removeformat | help',
                images_upload_url: '/IdeaTracker/UploadImage',
                content_style: 'body {{ font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif; font-size: 16px; line-height: 1.6; color: #334155; }}',
                height: 600,
                promotion: false, // removes the upgrade button
                branding: false,
                image_title: true,
                automatic_uploads: true,
                file_picker_types: 'image',
                setup: function(editor) {{
                    editor.on('change', function() {{
                        editor.save();
                    }});
                }}
            }});

            document.getElementById('ideaForm').addEventListener('submit', () => {{
                const saveBtn = document.getElementById('saveBtn');
                saveBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin mr-2"></i> Saving...';
                saveBtn.classList.add('opacity-75', 'cursor-not-allowed');
            }});
        }});
    </script>
}}
"""

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Create.cshtml', 'w') as f:
    f.write(get_template(False))

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Edit.cshtml', 'w') as f:
    f.write(get_template(True))

