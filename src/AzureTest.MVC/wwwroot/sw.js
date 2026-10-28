const CACHE_NAME = 'trackerpro-cache-v5';
const urlsToCache = [
  '/offline.html',
  '/css/site.css',
  '/css/style.css',
  '/js/site.js',
  '/js/offline-sync.js',
  '/icon-192.png',
  '/icon-512.png',
  '/favicon.ico',
  '/favicon.png',
  '/manifest.json'
];

// Install event - cache core assets
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        return Promise.allSettled(urlsToCache.map(url => cache.add(url)));
      })
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME) {
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - network first with offline.html fallback for navigation
self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET') return;

  // Strictly allow only http and https schemes (silently reject chrome-extension://, moz-extension://, blob:, data:, etc.)
  const reqUrl = event.request.url || '';
  if (!reqUrl.startsWith('http://') && !reqUrl.startsWith('https://')) return;

  try {
    const url = new URL(reqUrl);
    if (url.protocol !== 'http:' && url.protocol !== 'https:') return;
  } catch (e) {
    return;
  }

  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request)
        .then(response => {
          if (response && response.status === 200) {
            const copy = response.clone();
            caches.open(CACHE_NAME).then(c => {
              c.put(event.request, copy).catch(() => {});
            }).catch(() => {});
          }
          return response;
        })
        .catch(async () => {
          const cachedPage = await caches.match(event.request);
          if (cachedPage) return cachedPage;
          const fallback = await caches.match('/offline.html');
          return fallback || new Response('Offline', { status: 200, headers: { 'Content-Type': 'text/html' } });
        })
    );
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then(response => {
        if (response && response.status === 200 && response.type === 'basic') {
          const responseToCache = response.clone();
          caches.open(CACHE_NAME).then(cache => {
            cache.put(event.request, responseToCache).catch(() => {});
          }).catch(() => {});
        }
        return response;
      })
      .catch(() => {
        return caches.match(event.request);
      })
  );
});

// Web Push Notification Handler (zero external paid services needed)
self.addEventListener('push', event => {
  let data = {};
  if (event.data) {
    try {
      data = event.data.json();
    } catch (e) {
      data = { title: 'TrackerPro Update', body: event.data.text() };
    }
  } else {
    data = { title: 'TrackerPro Update', body: 'You have new activity in your workspace.' };
  }

  const title = data.title || 'TrackerPro';
  const options = {
    body: data.body || 'You have new activity.',
    icon: data.icon || '/icon-192.png',
    badge: data.badge || '/icon-192.png',
    data: { url: data.url || '/' },
    vibrate: [100, 50, 100],
    requireInteraction: false
  };

  event.waitUntil(self.registration.showNotification(title, options));
});

// Notification Click Handler
self.addEventListener('notificationclick', event => {
  event.notification.close();
  const targetUrl = (event.notification.data && event.notification.data.url) ? event.notification.data.url : '/';

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
      for (const client of windowClients) {
        if (client.url === targetUrl && 'focus' in client) {
          return client.focus();
        }
      }
      if (clients.openWindow) {
        return clients.openWindow(targetUrl);
      }
    })
  );
});

// Background Sync Handler
self.addEventListener('sync', event => {
  if (event.tag === 'sync-offline-queue') {
    // Notify window clients to trigger synchronization
    event.waitUntil(
      clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
        windowClients.forEach(client => {
          client.postMessage({ type: 'TRIGGER_OFFLINE_SYNC' });
        });
      })
    );
  }
});
