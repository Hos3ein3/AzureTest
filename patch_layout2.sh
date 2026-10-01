sed -i '' '/<\/body>/i\
<script>\
    document.getElementById("sidebarToggle")?.addEventListener("click", function() {\
        var sb = document.getElementById("sidebar");\
        var texts = document.querySelectorAll(".sidebar-text");\
        if(sb.classList.contains("w-64")) {\
            sb.classList.replace("w-64", "w-16");\
            texts.forEach(t => { t.style.opacity = "0"; t.style.width = "0"; });\
        } else {\
            sb.classList.replace("w-16", "w-64");\
            texts.forEach(t => { t.style.opacity = "1"; t.style.width = "auto"; });\
        }\
    });\
</script>\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml
