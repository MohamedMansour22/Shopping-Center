/* Firebase Cloud Messaging service worker.
 *
 * Served at the site root (/firebase-messaging-sw.js) so it controls the whole origin — this is
 * required for background push to register. It receives messages while no app tab is focused,
 * shows the notification, and handles the click → /admin/orders/:id redirect.
 *
 * Service workers can't import the Angular environment, so the Firebase web config below must be
 * kept in sync with `src/environments/environment.ts`. Leaving it blank simply means no background
 * notifications (the app still runs). The compat SDK version should match the installed `firebase`
 * package.
 */
importScripts('https://www.gstatic.com/firebasejs/12.15.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/12.15.0/firebase-messaging-compat.js');

const firebaseConfig = {
  apiKey: '',
  authDomain: '',
  projectId: '',
  messagingSenderId: '',
  appId: '',
};

// Only wire up messaging once a project is configured.
if (firebaseConfig.apiKey) {
  firebase.initializeApp(firebaseConfig);
  const messaging = firebase.messaging();

  // Data-only messages don't auto-display, so we build the notification ourselves here. This keeps
  // a single notification (no duplicate from the SDK) and lets us attach the click-through URL.
  messaging.onBackgroundMessage((payload) => {
    const data = payload.data || {};
    self.registration.showNotification(data.title || 'New order received', {
      body: data.body || '',
      icon: '/favicon.ico',
      tag: data.orderId ? 'order-' + data.orderId : 'order',
      data: { url: data.url || '/admin/orders' },
    });
  });
}

// Clicking the notification focuses an existing app tab (navigating it to the order) or opens one.
self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const url = (event.notification.data && event.notification.data.url) || '/admin/orders';
  const target = new URL(url, self.location.origin).href;

  event.waitUntil(
    (async () => {
      const windows = await clients.matchAll({ type: 'window', includeUncontrolled: true });
      for (const client of windows) {
        if (client.url.startsWith(self.location.origin)) {
          await client.focus();
          if ('navigate' in client) {
            try {
              await client.navigate(target);
            } catch (e) {
              /* cross-origin or detached — fall through to openWindow */
            }
          }
          return;
        }
      }
      await clients.openWindow(target);
    })()
  );
});
