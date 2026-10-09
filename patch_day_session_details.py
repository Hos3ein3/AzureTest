import re

def insert_details_html():
    return """
                        let detailsHtml = '';
                        if (tObj && tObj.id) {
                            let desc = tObj.description ? `<p class="text-xs text-slate-500 mt-2 italic">${tObj.description}</p>` : '';
                            let eq = (tObj.equipments && tObj.equipments.length > 0) ? `<div class="mt-2 flex flex-wrap gap-1">${tObj.equipments.map(e => `<span class="bg-slate-100 text-slate-600 text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border border-slate-200">${e}</span>`).join('')}</div>` : '';
                            let mediaHtml = '';
                            if (tObj.mediaUrl) {
                                if (tObj.mediaUrl.includes('youtube.com') || tObj.mediaUrl.includes('youtu.be')) {
                                    let videoId = tObj.mediaUrl.split('v=')[1] || tObj.mediaUrl.split('youtu.be/')[1];
                                    if(videoId) videoId = videoId.split('&')[0];
                                    mediaHtml = `<div class="mt-2 aspect-video rounded overflow-hidden border border-slate-200"><iframe class="w-full h-full" src="https://www.youtube.com/embed/${videoId}" frameborder="0" allowfullscreen></iframe></div>`;
                                } else if (tObj.mediaUrl.match(/\\.(jpeg|jpg|gif|png)$/i)) {
                                    mediaHtml = `<div class="mt-2"><img src="${tObj.mediaUrl}" class="rounded max-h-32 object-cover border border-slate-200" /></div>`;
                                } else {
                                    mediaHtml = `<div class="mt-2"><a href="${tObj.mediaUrl}" target="_blank" class="text-[10px] text-brand-600 font-bold uppercase"><i class="fa-solid fa-link"></i> View Media</a></div>`;
                                }
                            }
                            detailsHtml = `<div class="mt-2 bg-slate-50 p-2 rounded-lg border border-slate-100">` + desc + eq + mediaHtml + `</div>`;
                        }
"""

# For Day.cshtml
path_day = "src/AzureTest.MVC/Views/Workout/Day.cshtml"
with open(path_day, "r") as f: c_day = f.read()

c_day = c_day.replace(
    "let isDuration = tObj && tObj.type === 1; // 1 = DurationAndRest",
    "let isDuration = tObj && tObj.type === 1; // 1 = DurationAndRest\n" + insert_details_html()
)

c_day = c_day.replace(
    """${options}
                                    </select>
                                </td>""",
    """${options}
                                    </select>
                                    ${detailsHtml}
                                </td>"""
)

with open(path_day, "w") as f: f.write(c_day)


# For Session.cshtml
path_session = "src/AzureTest.MVC/Views/Workout/Session.cshtml"
with open(path_session, "r") as f: c_session = f.read()

# Replace the existing mediaHtml logic
media_logic = """                    let mediaHtml = '';
                    if (tDef.mediaUrl) {
                        if (tDef.mediaUrl.includes('youtube.com') || tDef.mediaUrl.includes('youtu.be')) {
                            let videoId = tDef.mediaUrl.split('v=')[1] || tDef.mediaUrl.split('youtu.be/')[1];
                            if(videoId) videoId = videoId.split('&')[0];
                            mediaHtml = `<div class="mt-4 aspect-video rounded-xl overflow-hidden border border-slate-200 shadow-sm"><iframe class="w-full h-full" src="https://www.youtube.com/embed/${videoId}" frameborder="0" allowfullscreen></iframe></div>`;
                        } else if (tDef.mediaUrl.match(/\\.(jpeg|jpg|gif|png)$/)) {
                            mediaHtml = `<div class="mt-4"><img src="${tDef.mediaUrl}" class="rounded-xl max-h-48 object-cover border border-slate-200 shadow-sm" /></div>`;
                        } else {
                            mediaHtml = `<div class="mt-4"><a href="${tDef.mediaUrl}" target="_blank" class="text-brand-500 font-medium hover:underline"><i class="fa-solid fa-link mr-1"></i> View Media</a></div>`;
                        }
                    }"""

new_media_logic = """                    let detailsHtml = '';
                    if (tDef && tDef.id) {
                        let desc = tDef.description ? `<p class="text-sm text-slate-500 mt-3 bg-white p-3 rounded-lg border border-slate-100 shadow-inner italic">${tDef.description}</p>` : '';
                        let eq = (tDef.equipments && tDef.equipments.length > 0) ? `<div class="mt-3 flex flex-wrap gap-2">${tDef.equipments.map(e => `<span class="bg-indigo-50 text-indigo-700 text-xs font-bold uppercase px-2.5 py-1 rounded-md border border-indigo-100 shadow-sm"><i class="fa-solid fa-dumbbell mr-1"></i>${e}</span>`).join('')}</div>` : '';
                        let mediaHtml = '';
                        if (tDef.mediaUrl) {
                            if (tDef.mediaUrl.includes('youtube.com') || tDef.mediaUrl.includes('youtu.be')) {
                                let videoId = tDef.mediaUrl.split('v=')[1] || tDef.mediaUrl.split('youtu.be/')[1];
                                if(videoId) videoId = videoId.split('&')[0];
                                mediaHtml = `<div class="mt-4 aspect-video rounded-xl overflow-hidden border border-slate-200 shadow-sm"><iframe class="w-full h-full" src="https://www.youtube.com/embed/${videoId}" frameborder="0" allowfullscreen></iframe></div>`;
                            } else if (tDef.mediaUrl.match(/\\.(jpeg|jpg|gif|png)$/i)) {
                                mediaHtml = `<div class="mt-4"><img src="${tDef.mediaUrl}" class="rounded-xl max-h-48 object-cover border border-slate-200 shadow-sm" /></div>`;
                            } else {
                                mediaHtml = `<div class="mt-4"><a href="${tDef.mediaUrl}" target="_blank" class="text-brand-600 font-bold hover:underline bg-brand-50 px-3 py-1.5 rounded-lg border border-brand-100 inline-block"><i class="fa-solid fa-link mr-1"></i> View Media</a></div>`;
                            }
                        }
                        detailsHtml = desc + eq + mediaHtml;
                    }"""

c_session = c_session.replace(media_logic, new_media_logic)

# Then replace ${mediaHtml} with ${detailsHtml}
c_session = c_session.replace("${mediaHtml}", "${detailsHtml}")

with open(path_session, "w") as f: f.write(c_session)

