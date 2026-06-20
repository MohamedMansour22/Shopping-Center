import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../core/notification.service';
import { Notification } from '../core/models';
import { TranslatePipe } from '../core/translate.pipe';

// Admin header bell: shows the unread badge and a dropdown of recent notifications.
// Each item is clickable and redirects to the order details screen.
@Component({
  selector: 'app-notification-bell',
  imports: [DatePipe, TranslatePipe],
  template: `
    <div class="bell">
      <button
        type="button"
        class="bell-btn"
        [class.has-unread]="notifications.unreadCount() > 0"
        (click)="toggle()"
        [attr.aria-label]="'notif.title' | translate"
        [attr.aria-expanded]="open()"
      >
        <svg
          class="icon"
          aria-hidden="true"
          viewBox="0 0 24 24"
          width="1em"
          height="1em"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        @if (notifications.unreadCount() > 0) {
          <span class="badge">{{ notifications.unreadCount() > 99 ? '99+' : notifications.unreadCount() }}</span>
        }
      </button>

      @if (open()) {
        <div class="backdrop" (click)="close()"></div>
        <div class="panel" role="menu">
          <div class="panel-head">
            <span class="panel-title">{{ 'notif.title' | translate }}</span>
            @if (notifications.unreadCount() > 0) {
              <button type="button" class="link" (click)="markAllRead()">
                {{ 'notif.markAllRead' | translate }}
              </button>
            }
          </div>

          @if (notifications.items().length === 0) {
            <p class="empty">{{ 'notif.empty' | translate }}</p>
          } @else {
            <ul class="list">
              @for (n of notifications.items(); track n.id) {
                <li class="item" [class.unread]="!n.isRead" (click)="openNotification(n)">
                  <div class="row1">
                    <span class="title">{{ n.title }}</span>
                    @if (!n.isRead) {
                      <span class="dot" aria-hidden="true"></span>
                    }
                  </div>
                  <div class="msg">{{ n.message }}</div>
                  <div class="time">{{ n.createdAtUtc | date: 'short' }}</div>
                </li>
              }
            </ul>
          }
        </div>
      }
    </div>
  `,
  styleUrl: './notification-bell.scss',
})
export class NotificationBell {
  readonly notifications = inject(NotificationService);
  private router = inject(Router);

  readonly open = signal(false);

  toggle(): void {
    const next = !this.open();
    this.open.set(next);
    if (next) {
      // Load the latest list each time the dropdown is opened.
      this.notifications.loadList();
    }
  }

  close(): void {
    this.open.set(false);
  }

  openNotification(n: Notification): void {
    this.notifications.markRead(n.id);
    this.close();
    if (n.orderId) {
      this.router.navigate(['/admin/orders', n.orderId]);
    }
  }

  markAllRead(): void {
    this.notifications.markAllRead();
  }
}
