// TrackerPro Offline Enhancement & Background Sync
// Handles offline logging, creation, edits, and background synchronization for Habits, Events, and Gym Training
// EXPLICIT RESTRICTION: NEVER for weight logs/analytics or resumes per specification.

(function () {
    const DB_NAME = 'TrackerProOfflineDB';
    const DB_VERSION = 1;
    const STORE_NAME = 'offline_sync_queue';

    function openDB() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(DB_NAME, DB_VERSION);
            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                if (!db.objectStoreNames.contains(STORE_NAME)) {
                    db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true });
                }
            };
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    function isForbiddenOfflineUrl(url) {
        if (!url) return true;
        const lower = url.toLowerCase();
        // Strictly forbidden per user instructions: weight analytics and resumes
        return lower.includes('weight') || lower.includes('resume');
    }

    function isAllowedOfflineUrl(url) {
        if (!url || isForbiddenOfflineUrl(url)) return false;
        const lower = url.toLowerCase();
        return lower.includes('/habit/') ||
               lower.includes('/event/') ||
               lower.includes('/gymadmin') ||
               lower.includes('/gymcourses') ||
               lower.includes('/workout/finishsession') ||
               lower.includes('/workout/updateday') ||
               lower.includes('/workout/register');
    }

    async function enqueueOfflineItem(item) {
        if (!item || !item.url || isForbiddenOfflineUrl(item.url)) {
            return;
        }

        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_NAME, 'readwrite');
            const store = tx.objectStore(STORE_NAME);
            const req = store.add(item);
            req.onsuccess = () => {
                showOfflineToast(`Offline: ${item.type || 'Action'} saved locally. Will sync automatically when back online.`);
                updateOfflineBadge();
                resolve(req.result);
            };
            req.onerror = () => reject(req.error);
        });
    }

    async function getAllQueuedItems() {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_NAME, 'readonly');
            const store = tx.objectStore(STORE_NAME);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result || []);
            req.onerror = () => reject(req.error);
        });
    }

    async function deleteQueuedItem(id) {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_NAME, 'readwrite');
            const store = tx.objectStore(STORE_NAME);
            const req = store.delete(id);
            req.onsuccess = () => resolve();
            req.onerror = () => reject(req.error);
        });
    }

    function showOfflineToast(message, isSuccess = false) {
        let toast = document.getElementById('trackerOfflineToast');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'trackerOfflineToast';
            toast.className = 'fixed bottom-20 left-1/2 -translate-x-1/2 z-50 px-4 py-2.5 rounded-2xl shadow-xl text-xs font-semibold flex items-center gap-2 transition-all transform translate-y-4 opacity-0 pointer-events-none';
            document.body.appendChild(toast);
        }

        if (isSuccess) {
            toast.className = 'fixed bottom-20 left-1/2 -translate-x-1/2 z-50 px-4 py-2.5 rounded-2xl shadow-xl text-xs font-semibold flex items-center gap-2 bg-emerald-600 text-white transition-all transform translate-y-0 opacity-100 pointer-events-auto';
            toast.innerHTML = `<i class="fa-solid fa-cloud-arrow-up text-sm"></i><span>${message}</span>`;
        } else {
            toast.className = 'fixed bottom-20 left-1/2 -translate-x-1/2 z-50 px-4 py-2.5 rounded-2xl shadow-xl text-xs font-semibold flex items-center gap-2 bg-slate-900 text-white border border-slate-700 transition-all transform translate-y-0 opacity-100 pointer-events-auto';
            toast.innerHTML = `<i class="fa-solid fa-wifi text-amber-400 text-sm"></i><span>${message}</span>`;
        }

        setTimeout(() => {
            toast.classList.remove('translate-y-0', 'opacity-100', 'pointer-events-auto');
            toast.classList.add('translate-y-4', 'opacity-0', 'pointer-events-none');
        }, 4500);
    }

    async function updateOfflineBadge() {
        try {
            const items = await getAllQueuedItems();
            const count = items.length;
            window.offlineQueueCount = count;
            window.dispatchEvent(new CustomEvent('offlineQueueUpdated', { detail: { count } }));
        } catch {}
    }

    // Background Sync execution
    async function syncOfflineQueue() {
        if (!navigator.onLine) return;
        let items = [];
        try {
            items = await getAllQueuedItems();
        } catch {
            return;
        }

        if (!items || items.length === 0) return;

        let syncedCount = 0;
        const currentToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

        for (const item of items) {
            try {
                const headers = {
                    'X-Requested-With': 'XMLHttpRequest'
                };

                let bodyData;
                if (item.contentType === 'application/json') {
                    headers['Content-Type'] = 'application/json';
                    if (currentToken) {
                        headers['RequestVerificationToken'] = currentToken;
                    }
                    bodyData = typeof item.payload === 'string' ? item.payload : JSON.stringify(item.payload);
                } else if (item.formDataEntries) {
                    const fd = new FormData();
                    let tokenAdded = false;
                    item.formDataEntries.forEach(([k, v]) => {
                        if (k === '__RequestVerificationToken') {
                            fd.append(k, currentToken || v);
                            tokenAdded = true;
                        } else {
                            fd.append(k, v);
                        }
                    });
                    if (!tokenAdded && currentToken) {
                        fd.append('__RequestVerificationToken', currentToken);
                    }
                    bodyData = fd;
                }

                const response = await fetch(item.url, {
                    method: item.method || 'POST',
                    headers: headers,
                    body: bodyData,
                    credentials: 'include'
                });

                const isLoginRedirect = response.redirected && response.url && response.url.toLowerCase().includes('/auth/login');
                const isSuccess = (response.ok || (response.status >= 200 && response.status < 400)) && !isLoginRedirect;

                if (isSuccess) {
                    await deleteQueuedItem(item.id);
                    syncedCount++;
                } else if (response.status === 400 && response.statusText === 'Bad Request' && !currentToken) {
                    // Stale CSRF token without active page token; retain in queue for next sync
                    console.warn('Pending sync deferred due to authentication state.');
                }
            } catch (err) {
                console.warn('Failed to sync offline item:', item, err);
            }
        }

        if (syncedCount > 0) {
            const msg = `Synced ${syncedCount} offline ${syncedCount === 1 ? 'item' : 'items'} successfully.`;
            showOfflineToast(msg, true);
            updateOfflineBadge();

            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes('habit') || currentPath.includes('event') || currentPath.includes('gym') || currentPath.includes('workout')) {
                setTimeout(() => window.location.reload(), 1200);
            }
        }
    }

    // Intercept form submissions when offline
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form || !form.action) return;

        const actionUrl = form.action.toLowerCase();
        if (!isAllowedOfflineUrl(actionUrl)) {
            return;
        }

        if (!navigator.onLine) {
            e.preventDefault();

            const formData = new FormData(form);
            const entries = [];
            for (let [k, v] of formData.entries()) {
                entries.push([k, v]);
            }

            const isHabit = actionUrl.includes('/habit/');
            const isEvent = actionUrl.includes('/event/');
            const itemType = isHabit ? 'Habit Action' : isEvent ? 'Event Action' : 'Gym Workout';

            enqueueOfflineItem({
                url: form.action,
                method: form.method || 'POST',
                formDataEntries: entries,
                type: itemType,
                timestamp: Date.now()
            });

            // Optimistic UI updates
            if (actionUrl.includes('/habit/create')) {
                const habitName = form.querySelector('[name="name"]')?.value || 'New Habit';
                const habitDesc = form.querySelector('[name="description"]')?.value || '';
                const habitType = form.querySelector('[name="type"]')?.value || 'Boolean';

                const listContainer = document.getElementById('habitsListContainer');
                if (listContainer) {
                    const tempCard = document.createElement('div');
                    tempCard.className = 'bg-white rounded-xl shadow-sm border border-brand-300 bg-brand-50/20 p-6 relative transition-all animate-pulse';
                    tempCard.innerHTML = `
                        <div class="flex justify-between items-start gap-4">
                            <div>
                                <div class="flex items-center gap-2 flex-wrap">
                                    <span class="text-lg font-semibold text-slate-800">${escapeHtml(habitName)}</span>
                                    <span class="text-xs px-2.5 py-0.5 rounded-full bg-slate-100 text-slate-700 font-medium">${escapeHtml(habitType)}</span>
                                    <span class="inline-flex items-center text-xs font-bold text-amber-700 bg-amber-100 px-2.5 py-0.5 rounded-full border border-amber-300">
                                        <i class="fa-solid fa-cloud-arrow-up mr-1 text-amber-600"></i> Saved Offline (Pending Sync)
                                    </span>
                                </div>
                                ${habitDesc ? `<p class="text-sm text-slate-500 mt-1">${escapeHtml(habitDesc)}</p>` : ''}
                            </div>
                        </div>
                    `;
                    listContainer.prepend(tempCard);
                }
                form.reset();
            } else if (actionUrl.includes('/habit/log')) {
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    submitBtn.classList.add('bg-emerald-500', 'text-white');
                    const textSpan = form.querySelector('span.text-sm');
                    if (textSpan) {
                        textSpan.textContent = 'Completed Today! (Saved Offline)';
                    }
                }
            }

            // Close any open modals
            const modal = form.closest('[role="dialog"]') || form.closest('.fixed.inset-0');
            if (modal && !modal.id?.includes('Layout')) {
                modal.classList.add('hidden');
            }
        }
    });

    // Helper to safely escape HTML strings for optimistic DOM insertion
    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    }

    // Intercept window.fetch for direct AJAX calls when offline (e.g. gym workout sessions and habits)
    const originalFetch = window.fetch;
    window.fetch = async function (resource, options = {}) {
        const url = (typeof resource === 'string' ? resource : (resource?.url || '')).toLowerCase();
        const method = (options.method || 'GET').toUpperCase();

        if (method === 'GET' || !isAllowedOfflineUrl(url)) {
            return originalFetch.apply(this, arguments);
        }

        // If currently offline, queue immediately
        if (!navigator.onLine) {
            return queueFetchRequest(resource, options);
        }

        try {
            return await originalFetch.apply(this, arguments);
        } catch (err) {
            // If network request failed due to disconnection, queue it
            if (!navigator.onLine || err.name === 'TypeError') {
                return queueFetchRequest(resource, options);
            }
            throw err;
        }
    };

    async function queueFetchRequest(resource, options) {
        const url = typeof resource === 'string' ? resource : resource.url;
        const method = options.method || 'POST';
        let payload = null;
        let contentType = 'application/json';
        let formDataEntries = null;

        if (options.body instanceof FormData) {
            formDataEntries = Array.from(options.body.entries());
            contentType = 'multipart/form-data';
        } else if (typeof options.body === 'string') {
            try {
                payload = JSON.parse(options.body);
            } catch {
                payload = options.body;
            }
        }

        await enqueueOfflineItem({
            url: url,
            method: method,
            payload: payload,
            formDataEntries: formDataEntries,
            contentType: contentType,
            type: url.includes('workout') || url.includes('gym') ? 'Gym Session' : 'Habit Operation',
            timestamp: Date.now()
        });

        return new Response(JSON.stringify({ success: true, offline: true, message: 'Saved offline. Will sync when online.' }), {
            status: 200,
            headers: { 'Content-Type': 'application/json' }
        });
    }

    // Connection state listeners
    window.addEventListener('online', () => {
        showOfflineToast('Connection restored. Syncing pending data...', true);
        syncOfflineQueue();
    });

    window.addEventListener('offline', () => {
        showOfflineToast('You are currently offline. Habits and gym workouts will be saved locally.');
    });

    // PWA Install prompt capture
    let deferredInstallPrompt = null;
    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredInstallPrompt = e;
        window.deferredInstallPrompt = e;
        const installSection = document.getElementById('sidebarInstallSection');
        if (installSection) installSection.style.display = 'block';
    });

    window.triggerPwaInstall = function () {
        if (deferredInstallPrompt) {
            deferredInstallPrompt.prompt();
            deferredInstallPrompt.userChoice.then((choiceResult) => {
                if (choiceResult.outcome === 'accepted') {
                    console.log('User accepted TrackerPro PWA installation.');
                }
                deferredInstallPrompt = null;
                window.deferredInstallPrompt = null;
            });
        } else {
            // Inform user about browser install options (e.g. Chrome menu > Install TrackerPro)
            const isIos = /iphone|ipad|ipod/.test(window.navigator.userAgent.toLowerCase());
            if (isIos) {
                alert('To install TrackerPro on iOS: tap the Share button in Safari, then select "Add to Home Screen".');
            } else {
                alert('To install TrackerPro: tap the 3-dot menu in Chrome and select "Install TrackerPro" or "Add to Home Screen". Note: Chrome requires HTTPS or localhost for PWA installation.');
            }
        }
    };

    // On page ready
    window.addEventListener('load', () => {
        updateOfflineBadge();
        if (navigator.onLine) {
            syncOfflineQueue();
        }
    });

    window.enqueueOfflineItem = enqueueOfflineItem;
    window.syncOfflineQueue = syncOfflineQueue;
})();
