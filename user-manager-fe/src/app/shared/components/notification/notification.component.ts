import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';
import type { NotificationType } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  templateUrl: './notification.component.html'
})
export class NotificationComponent {
  private notificationService = inject(NotificationService);
  notifications = this.notificationService.list;

  dismiss(id: number): void {
    this.notificationService.dismiss(id);
  }

  iconFor(type: NotificationType): string {
    switch (type) {
      case 'success': return 'check_circle';
      case 'error': return 'error';
      case 'warning':
      case 'validation': return 'warning';
      default: return 'info';
    }
  }

  toneClass(type: NotificationType): string {
    switch (type) {
      case 'success': return 'text-ok';
      case 'error': return 'text-bad';
      case 'warning':
      case 'validation': return 'text-warn';
      default: return 'text-info';
    }
  }
}
