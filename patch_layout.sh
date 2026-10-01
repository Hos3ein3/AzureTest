sed -i '' '/<link href="https:\/\/cdnjs.cloudflare.com\/ajax\/libs\/font-awesome/a\
    <style>\
        /* Hide scrollbar for Chrome, Safari and Opera */\
        .scrollbar-hide::-webkit-scrollbar {\
            display: none;\
        }\
        /* Hide scrollbar for IE, Edge and Firefox */\
        .scrollbar-hide {\
            -ms-overflow-style: none;  /* IE and Edge */\
            scrollbar-width: none;  /* Firefox */\
        }\
    </style>\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml

sed -i '' 's/<nav class="flex-1 overflow-y-auto py-4">/<nav class="flex-1 overflow-y-auto py-4 scrollbar-hide">/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml

