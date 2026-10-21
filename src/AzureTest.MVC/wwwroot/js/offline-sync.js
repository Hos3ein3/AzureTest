// TrackerPro Offline Enhancement & Background Sync
// Handles offline logging and synchronization for Habits, Events, and Gym Training
// EXPLICIT RESTRICTION: NEVER for weight logs or resumes per specification.

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

    async function enqueueOfflineItem(item) {
        // Enforce exclusion rules: NOT for weight analytics or resumes
        if (!item || !item.url) return;
        const lowerUrl = item.url.toLowerCase();
        if (lowerUrl.includes('weight') || lowerUrl.includes('resume')) {
            console.warn('Offline queuing is strictly disabled for weight analytics and resumes.');
            return;
        }

        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_NAME, 'readwrite');
            const store = tx.objectStore(STORE_NAME);
            const req = store.add(item);
            req.onsuccess = () => {
                showOfflineToast(`Offline: ${item.type || 'Action'} saved locally. Will sync when back online.`);
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
        }, 4000);
    }

    async function updateOfflineBadge() {
        const items = await getAllQueuedItems();
        const count = items.length;
        window.offlineQueueCount = count;
        window.dispatchEvent(new CustomEvent('offlineQueueUpdated', { detail: { count } }));
    }

    async function syncOfflineQueue() {
        if (!navigator.onLine) return;
        const items = await getAllQueuedItems();
        if (items.length === 0) return;

        let syncedCount = 0;
        for (const item of items) {
            try {
                let bodyData;
                const headers = { 'X-Requested-With': 'XMLHttpRequest' };

                if (item.contentType === 'application/json') {
                    headers['Content-Type'] = 'application/json';
                    bodyData = JSON.stringify(item.payload);
                } else if (item.formDataEntries) {
                    const fd = new FormData();
                    item.formDataEntries.forEach(([k, v]) => fd.append(k, v));
                    bodyData = fd;
                }

                const response = await fetch(item.url, {
                    method: item.method || 'POST',
                    headers: headers,
                    body: bodyData
                });

                if (response.ok) {
                    await deleteQueuedItem(item.id);
                    syncedCount++;
                }
            } catch (err) {
                console.warn('Failed to sync item:', item, err);
            }
        }

        if (syncedCount > 0) {
            const msg = `Synced ${syncedCount} offline ${syncedCount === 1 ? 'entry' : 'entries'} successfully.`;
            showOfflineToast(msg, true);
            if (window.sendLocalNotification) {
                window.sendLocalNotification('Offline Sync Complete', { body: msg });
            }
            updateOfflineBadge();
            // Refresh current page if on habits or events list
            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes('habit') || currentPath.includes('event') || currentPath.includes('gym')) {
                setTimeout(() => window.location.reload(), 1500);
            }
        }
    }

    // Intercept form submissions when offline for Habit, Event, and Gym Training
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form || !form.action) return;

        const actionUrl = form.action.toLowerCase();

        // Strictly verify allowed types: Habits, Events, and Gym Training
        const isHabit = actionUrl.includes('/habit/log') || actionUrl.includes('/habit/create') || actionUrl.includes('/habit/undo');
        const isEvent = actionUrl.includes('/event/create');
        const isGym = (actionUrl.includes('/gymadmin/createtraining') || actionUrl.includes('/gymcourses')) && !actionUrl.includes('weight');

        // Strictly reject forbidden types: weight analytics and resumes
        const isForbidden = actionUrl.includes('weight') || actionUrl.includes('resume');

        if (isForbidden || (!isHabit && !isEvent && !isGym)) {
            return;
        }

        if (!navigator.onLine) {
            e.preventDefault();
            const formData = new FormData(form);
            const entries = [];
            for (let [k, v] of formData.entries()) {
                entries.push([k, v]);
            }

            const itemType = isHabit ? 'Habit Log' : isEvent ? 'Event' : 'Gym Training';
            enqueueOfflineItem({
                url: form.action,
                method: form.method || 'POST',
                formDataEntries: entries,
                type: itemType,
                timestamp: Date.now()
            });

            if (form.closest('.modal')) {
                form.closest('.modal').classList.add('hidden');
            }
        }
    });

    // Listen for connection changes
    window.addEventListener('online', () => {
        showOfflineToast('Connection restored. Syncing pending data...', true);
        syncOfflineQueue();
    });

    window.addEventListener('offline', () => {
        showOfflineToast('You are currently offline. Changes will be saved locally.');
    });

    // Initial check on load
    window.addEventListener('load', () => {
        updateOfflineBadge();
        if (navigator.onLine) {
            syncOfflineQueue();
        }
    });

    window.enqueueOfflineItem = enqueueOfflineItem;
    window.syncOfflineQueue = syncOfflineQueue;
})();
