import { Component, effect, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { NotificationService } from '../core/notification.service';
import { PushNotificationService } from '../core/push-notification.service';
import { TranslatePipe } from '../core/translate.pipe';
import { NotificationBell } from '../shared/notification-bell';
import { SettingsControls } from '../shared/settings-controls';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, TranslatePipe, SettingsControls, NotificationBell],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  private auth = inject(AuthService);
  private router = inject(Router);
  private push = inject(PushNotificationService);
  private notifications = inject(NotificationService);

  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly email = this.auth.email;

  constructor() {
    // Once an admin is signed in: register for push and start polling the notification feed.
    effect(() => {
      if (this.isLoggedIn()) {
        this.push.init();
        this.notifications.start();
      } else {
        this.notifications.stop();
      }
    });
  }

  async logout(): Promise<void> {
    // Unregister the device token while the JWT is still valid, then clear the session.
    this.notifications.stop();
    await this.push.disable();
    this.auth.logout();
    this.router.navigate(['/admin/login']);
  }
}
