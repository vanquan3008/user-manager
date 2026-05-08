import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../../core/services/confirm.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-dialog.component.html'
})
export class ConfirmDialogComponent {
  private confirm = inject(ConfirmService);
  theme = inject(ThemeService).theme;

  vm = this.confirm.vm;

  cancel() {
    this.confirm.cancel();
  }

  accept() {
    this.confirm.accept();
  }

  icon() {
    const variant = this.vm().options?.variant;
    return variant === 'danger' ? 'warning' : 'help';
  }
}

