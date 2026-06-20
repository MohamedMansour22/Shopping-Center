import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { FirebaseApp, initializeApp } from 'firebase/app';
import { Messaging, deleteToken, getMessaging, getToken, onMessage } from 'firebase/messaging';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotificationService } from './notification.service';

// Registers the admin's browser for Firebase Cloud Messaging and forwards the device token to the
// API. Notifications are shown + made clickable by public/firebase-messaging-sw.js; this service
// only handles permission, token registration, and the foreground (tab-focused) message path.
@Injectable({ providedIn: 'root' })
export class PushNotificationService {
  private http = inject(HttpClient);
  private feed = inject(NotificationService);
  private readonly baseUrl = `${environment.apiUrl}/notifications/device-tokens`;

  private messaging: Messaging | null = null;
  private app: FirebaseApp | null = null;
  private currentToken: string | null = null;
  private started = false;

  // Disabled (and the rest of the app unaffected) until a Firebase project is configured.
  private get configured(): boolean {
    return !!environment.firebase?.apiKey && !!environment.firebaseVapidKey;
  }

  // Call once an admin is logged in. Idempotent and best-effort — any failure leaves the app working.
  async init(): Promise<void> {
    if (this.started || !this.configured) return;
    if (!('serviceWorker' in navigator) || !('Notification' in window)) return;

    this.started = true;
    try {
      const permission = await Notification.requestPermission();
      if (permission !== 'granted') {
        this.started = false;
        return;
      }

      const registration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');

      this.app ??= initializeApp(environment.firebase);
      this.messaging ??= getMessaging(this.app);

      const token = await getToken(this.messaging, {
        vapidKey: environment.firebaseVapidKey,
        serviceWorkerRegistration: registration,
      });
      if (!token) {
        this.started = false;
        return;
      }

      this.currentToken = token;
      await firstValueFrom(this.http.post(this.baseUrl, { token }));

      // Foreground messages are data-only and won't auto-display; show one ourselves so the click
      // still routes through the service worker (→ /admin/orders/:id).
      onMessage(this.messaging, (payload) => {
        const data = (payload.data ?? {}) as Record<string, string>;
        // Keep the in-app bell in sync immediately (don't wait for the next poll).
        this.feed.loadUnreadCount();
        registration.showNotification(data['title'] || 'New order received', {
          body: data['body'] || '',
          icon: '/favicon.ico',
          tag: data['orderId'] ? `order-${data['orderId']}` : 'order',
          data: { url: data['url'] || '/admin/orders' },
        });
      });
    } catch (e) {
      // Permission denied, unsupported browser, network error, etc. — never fatal.
      console.warn('Push notifications unavailable:', e);
      this.started = false;
    }
  }

  // Call before clearing the auth token on logout (the DELETE needs the still-valid JWT).
  async disable(): Promise<void> {
    const token = this.currentToken;
    this.started = false;
    this.currentToken = null;
    if (!token) return;

    try {
      await firstValueFrom(this.http.request('delete', this.baseUrl, { body: { token } }));
      if (this.messaging) await deleteToken(this.messaging);
    } catch {
      /* best-effort cleanup */
    }
  }
}
