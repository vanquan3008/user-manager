import { Component, HostListener, inject } from '@angular/core';
import { ConfirmService } from '../../../core/services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html'
})
export class ConfirmDialogComponent {
  private confirm = inject(ConfirmService);
  vm = this.confirm.vm;

  @HostListener('document:keydown.escape')
  onEscape() {
    if (this.vm().open) this.cancel();
  }

  cancel() { this.confirm.cancel(); }
  accept() { this.confirm.accept(); }
  icon() { return this.vm().options?.variant === 'danger' ? 'warning' : 'help'; }
}
