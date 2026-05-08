import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import type { Notification, NotificationType } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html'
})
export class NotificationComponent {
  private notificationService = inject(NotificationService);
  private themeService = inject(ThemeService);

  notifications = this.notificationService.list;
  theme = this.themeService.theme;

  dismiss(id: number): void {
    this.notificationService.dismiss(id);
  }

  iconFor(type: NotificationType): string {
    switch (type) {
      case 'success': return 'check_circle';
      case 'error': return 'error';
      case 'validation': return 'warning';
      default: return 'info';
    }
  }

  typeClass(type: NotificationType): string {
    switch (type) {
      case 'success':
        return this.theme() === 'light'
          ? 'border-emerald-200 bg-emerald-50 text-emerald-900'
          : 'border-emerald-500/30 bg-emerald-950/80 text-emerald-100';
      case 'error':
        return this.theme() === 'light'
          ? 'border-rose-200 bg-rose-50 text-rose-900'
          : 'border-rose-500/30 bg-rose-950/80 text-rose-100';
      case 'info':
      case 'validation':
        return this.theme() === 'light'
          ? 'border-amber-200 bg-amber-50 text-amber-900'
          : 'border-amber-500/30 bg-amber-950/80 text-amber-100';
      default:
        return '';
    }
  }

  stateClass(notification: Notification): string {
    return notification.leaving ? 'animate-[motion-toast-out_220ms_ease_forwards]' : 'animate-[motion-toast-in_260ms_cubic-bezier(0.22,1,0.36,1)]';
  }

  accentClass(type: NotificationType): string {
    switch (type) {
      case 'success':
        return this.theme() === 'light' ? 'bg-emerald-500' : 'bg-emerald-400';
      case 'error':
        return this.theme() === 'light' ? 'bg-rose-500' : 'bg-rose-400';
      case 'validation':
        return this.theme() === 'light' ? 'bg-amber-500' : 'bg-amber-400';
      default:
        return this.theme() === 'light' ? 'bg-sky-500' : 'bg-sky-400';
    }
  }
}
