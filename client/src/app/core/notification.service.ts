import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Notification } from './models';

// Drives the admin notification feed (the bell). Polls the unread count while an admin is signed in
// and loads the recent list on demand. Backed by the persisted Notifications table via the API.
const POLL_INTERVAL_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/notifications`;

  readonly items = signal<Notification[]>([]);
  readonly unreadCount = signal(0);

  private pollHandle: ReturnType<typeof setInterval> | null = null;

  // Begin polling the unread badge (called once an admin is logged in). Idempotent.
  start(): void {
    if (this.pollHandle !== null) return;
    this.loadUnreadCount();
    this.pollHandle = setInterval(() => this.loadUnreadCount(), POLL_INTERVAL_MS);
  }

  // Stop polling and clear state (called on logout).
  stop(): void {
    if (this.pollHandle !== null) {
      clearInterval(this.pollHandle);
      this.pollHandle = null;
    }
    this.items.set([]);
    this.unreadCount.set(0);
  }

  async loadUnreadCount(): Promise<void> {
    try {
      const res = await firstValueFrom(
        this.http.get<{ count: number }>(`${this.baseUrl}/unread-count`)
      );
      this.unreadCount.set(res.count);
    } catch {
      /* transient — keep the last known count */
    }
  }

  async loadList(): Promise<void> {
    try {
      const params = new HttpParams().set('take', 20);
      const items = await firstValueFrom(
        this.http.get<Notification[]>(this.baseUrl, { params })
      );
      this.items.set(items);
    } catch {
      /* ignore */
    }
  }

  async markRead(id: string): Promise<void> {
    // Optimistic: flip locally, then persist.
    let wasUnread = false;
    this.items.update((list) =>
      list.map((n) => {
        if (n.id === id && !n.isRead) wasUnread = true;
        return n.id === id ? { ...n, isRead: true } : n;
      })
    );
    if (wasUnread) this.unreadCount.update((c) => Math.max(0, c - 1));

    try {
      await firstValueFrom(this.http.put(`${this.baseUrl}/${id}/read`, {}));
    } catch {
      /* best-effort; the poll will reconcile the count */
    }
  }

  async markAllRead(): Promise<void> {
    this.items.update((list) => list.map((n) => ({ ...n, isRead: true })));
    this.unreadCount.set(0);
    try {
      await firstValueFrom(this.http.put(`${this.baseUrl}/read-all`, {}));
    } catch {
      /* best-effort */
    }
  }
}
