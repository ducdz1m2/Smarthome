self.addEventListener('install', (event) => {
    console.log('[Service Worker] Install');
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    console.log('[Service Worker] Activate');
    event.waitUntil(self.clients.claim());
});

self.addEventListener('push', (event) => {
    console.log('[Service Worker] Push received', event);

    let data = {
        title: 'Smarthome Notification',
        body: 'Bạn có thông báo mới',
        icon: '/favicon.png',
        badge: '/favicon.png',
        data: {}
    };

    if (event.data) {
        try {
            const payload = event.data.json();
            data = {
                title: payload.title || 'Smarthome Notification',
                body: payload.body || 'Bạn có thông báo mới',
                icon: payload.icon || '/favicon.png',
                badge: payload.badge || '/favicon.png',
                data: {
                    actionUrl: payload.actionUrl,
                    ...payload
                }
            };
        } catch (e) {
            console.error('[Service Worker] Error parsing push data:', e);
        }
    }

    const options = {
        body: data.body,
        icon: data.icon,
        badge: data.badge,
        data: data.data,
        requireInteraction: true,
        vibrate: [200, 100, 200]
    };

    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});

self.addEventListener('notificationclick', (event) => {
    console.log('[Service Worker] Notification clicked', event);

    event.notification.close();

    const actionUrl = event.notification.data?.actionUrl || '/';

    event.waitUntil(
        clients.matchAll({ type: 'window' }).then((clientList) => {
            // Check if there's already a window open
            for (const client of clientList) {
                if (client.url === actionUrl && 'focus' in client) {
                    return client.focus();
                }
            }

            // If no window is open, open a new one
            if (clients.openWindow) {
                return clients.openWindow(actionUrl);
            }
        })
    );
});

self.addEventListener('pushsubscriptionchange', (event) => {
    console.log('[Service Worker] Subscription changed', event);
    
    event.waitUntil(
        (async () => {
            const newSubscription = await event.newSubscription;
            const oldSubscription = await event.oldSubscription;

            // Send new subscription to server
            if (newSubscription) {
                await fetch('/api/pushsubscription/subscribe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        endpoint: newSubscription.endpoint,
                        keys: {
                            p256dh: newSubscription.getKey('p256dh'),
                            auth: newSubscription.getKey('auth')
                        }
                    })
                });
            }

            // Remove old subscription from server
            if (oldSubscription) {
                await fetch('/api/pushsubscription/unsubscribe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        endpoint: oldSubscription.endpoint
                    })
                });
            }
        })()
    );
});
