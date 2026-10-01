const CACHE_NAME = 'trackerpro-cache-v1';
const urlsToCache = [
  '/',
  '/css/site.css',
  '/js/site.js',
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
        // We use addAll but wrap in try/catch or individually to prevent one failure from stopping all
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

// Fetch event - network first, fallback to cache strategy for maximum freshness
self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET') return;
  
  event.respondWith(
    fetch(event.request)
      .then(response => {
        // If valid response, clone it and cache it
        if (response && response.status === 200 && response.type === 'basic') {
          const responseToCache = response.clone();
          caches.open(CACHE_NAME)
            .then(cache => {
              cache.put(event.request, responseToCache);
            });
        }
        return response;
      })
      .catch(() => {
        // Network failed, try to serve from cache
        return caches.match(event.request);
      })
  );
});
