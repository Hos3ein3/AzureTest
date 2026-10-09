import os

gym_admin_index = """
@model IEnumerable<AzureTest.Domain.Documents.Gym.Training>

@{
    ViewData["Title"] = "Gym Trainings";
    string search = ViewBag.Search as string ?? "";
    string sort = ViewBag.Sort as string ?? "Name";
    string dir = ViewBag.Dir as string ?? "asc";
    int pageNo = ViewBag.Page ?? 1;
    int totalPages = ViewBag.TotalPages ?? 1;
}

<div class="mb-6 flex justify-between items-center">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Gym Trainings</h1>
    <a asp-action="CreateTraining" class="btn-primary flex items-center gap-2">
        <i class="fa-solid fa-plus"></i> Add Training
    </a>
</div>

<div class="card p-6">
    <form method="get" class="mb-4 flex gap-4 items-center">
        <input type="hidden" name="sort" value="@sort" />
        <input type="hidden" name="dir" value="@dir" />
        <input type="text" name="search" value="@search" placeholder="Search trainings..." class="form-input max-w-sm" />
        <button type="submit" class="btn-secondary">Search</button>
        @if(!string.IsNullOrEmpty(search)) {
            <a asp-action="Index" class="text-sm text-gray-500 hover:text-gray-700">Clear</a>
        }
    </form>

    <div class="overflow-x-auto">
        <table class="table w-full">
            <thead>
                <tr>
                    <th>
                        <a asp-action="Index" asp-route-search="@search" asp-route-sort="Name" asp-route-dir="@(sort == "Name" && dir == "asc" ? "desc" : "asc")" class="flex items-center gap-1">
                            Name
                            @if(sort == "Name") { <i class="fa-solid fa-sort-@(dir == "asc" ? "up" : "down")"></i> }
                        </a>
                    </th>
                    <th>
                        <a asp-action="Index" asp-route-search="@search" asp-route-sort="Type" asp-route-dir="@(sort == "Type" && dir == "asc" ? "desc" : "asc")" class="flex items-center gap-1">
                            Type
                            @if(sort == "Type") { <i class="fa-solid fa-sort-@(dir == "asc" ? "up" : "down")"></i> }
                        </a>
                    </th>
                    <th>Base Calories</th>
                    <th class="text-right">Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Model)
                {
                    <tr>
                        <td class="font-medium">@item.Name</td>
                        <td>
                            <span class="badge @(item.Type == AzureTest.Domain.Enums.TrainingType.SetAndRest ? "bg-blue-100 text-blue-800" : "bg-green-100 text-green-800")">
                                @item.Type
                            </span>
                        </td>
                        <td>@item.BaseCaloriesBurn kcal</td>
                        <td class="text-right flex justify-end gap-2">
                            <a asp-action="EditTraining" asp-route-id="@item.Id" class="text-blue-500 hover:text-blue-700 p-2">
                                <i class="fa-solid fa-pen-to-square"></i>
                            </a>
                        </td>
                    </tr>
                }
                @if(!Model.Any()) {
                    <tr><td colspan="4" class="text-center py-4 text-gray-500">No trainings found.</td></tr>
                }
            </tbody>
        </table>
    </div>

    @if (totalPages > 1)
    {
        <div class="mt-4 flex justify-between items-center">
            <span class="text-sm text-gray-600 dark:text-gray-400">Page @pageNo of @totalPages</span>
            <div class="flex gap-2">
                @if (pageNo > 1) {
                    <a asp-action="Index" asp-route-search="@search" asp-route-sort="@sort" asp-route-dir="@dir" asp-route-page="@(pageNo - 1)" class="btn-secondary px-3 py-1">Previous</a>
                }
                @if (pageNo < totalPages) {
                    <a asp-action="Index" asp-route-search="@search" asp-route-sort="@sort" asp-route-dir="@dir" asp-route-page="@(pageNo + 1)" class="btn-secondary px-3 py-1">Next</a>
                }
            </div>
        </div>
    }
</div>
"""

gym_courses_index = """
@model IEnumerable<AzureTest.Domain.Documents.Gym.TrainingCourse>

@{
    ViewData["Title"] = "Gym Courses";
    string search = ViewBag.Search as string ?? "";
    string sort = ViewBag.Sort as string ?? "Name";
    string dir = ViewBag.Dir as string ?? "asc";
    int pageNo = ViewBag.Page ?? 1;
    int totalPages = ViewBag.TotalPages ?? 1;
}

<div class="mb-6 flex justify-between items-center">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Gym Courses</h1>
    <a asp-action="Create" class="btn-primary flex items-center gap-2">
        <i class="fa-solid fa-plus"></i> Add Course
    </a>
</div>

<div class="card p-6">
    <form method="get" class="mb-4 flex gap-4 items-center">
        <input type="hidden" name="sort" value="@sort" />
        <input type="hidden" name="dir" value="@dir" />
        <input type="text" name="search" value="@search" placeholder="Search courses..." class="form-input max-w-sm" />
        <button type="submit" class="btn-secondary">Search</button>
        @if(!string.IsNullOrEmpty(search)) {
            <a asp-action="Index" class="text-sm text-gray-500 hover:text-gray-700">Clear</a>
        }
    </form>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @foreach (var item in Model)
        {
            <div class="border rounded-xl p-5 hover:shadow-lg transition-shadow bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                <div class="flex justify-between items-start mb-2">
                    <h3 class="text-xl font-bold text-gray-900 dark:text-white">@item.Name</h3>
                    <a asp-action="Edit" asp-route-id="@item.Id" class="text-blue-500 hover:text-blue-700"><i class="fa-solid fa-pen-to-square"></i></a>
                </div>
                <p class="text-gray-600 dark:text-gray-400 text-sm mb-4 line-clamp-2">@item.Description</p>
                
                <div class="flex flex-wrap gap-2 mb-4">
                    @foreach(var tag in item.Tags) {
                        <span class="px-2 py-1 bg-gray-100 dark:bg-gray-700 text-xs rounded-md">@tag</span>
                    }
                </div>
                
                <div class="text-sm text-gray-500">
                    <i class="fa-regular fa-calendar"></i> @item.Weeks.Count Weeks
                </div>
            </div>
        }
        @if(!Model.Any()) {
            <div class="col-span-full text-center py-8 text-gray-500">No courses found.</div>
        }
    </div>

    @if (totalPages > 1)
    {
        <div class="mt-6 flex justify-between items-center">
            <span class="text-sm text-gray-600 dark:text-gray-400">Page @pageNo of @totalPages</span>
            <div class="flex gap-2">
                @if (pageNo > 1) {
                    <a asp-action="Index" asp-route-search="@search" asp-route-sort="@sort" asp-route-dir="@dir" asp-route-page="@(pageNo - 1)" class="btn-secondary px-3 py-1">Previous</a>
                }
                @if (pageNo < totalPages) {
                    <a asp-action="Index" asp-route-search="@search" asp-route-sort="@sort" asp-route-dir="@dir" asp-route-page="@(pageNo + 1)" class="btn-secondary px-3 py-1">Next</a>
                }
            </div>
        </div>
    }
</div>
"""

with open("src/AzureTest.MVC/Views/GymAdmin/Index.cshtml", "w") as f:
    f.write(gym_admin_index)

with open("src/AzureTest.MVC/Views/GymCourses/Index.cshtml", "w") as f:
    f.write(gym_courses_index)

# Update POST urls in GymCourses/Create and Edit to use /GymCourses/ instead of GymAdmin/Course
for fname, rep_from, rep_to in [("src/AzureTest.MVC/Views/GymCourses/Create.cshtml", "/GymAdmin/CourseCreate", "/GymCourses/Create"), 
                               ("src/AzureTest.MVC/Views/GymCourses/Edit.cshtml", "/GymAdmin/CourseEdit", "/GymCourses/Edit")]:
    with open(fname, "r") as f:
        c = f.read()
    c = c.replace(rep_from, rep_to)
    with open(fname, "w") as f:
        f.write(c)

print("Views updated")
