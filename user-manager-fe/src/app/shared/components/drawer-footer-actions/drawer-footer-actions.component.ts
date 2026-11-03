import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-drawer-footer-actions',
  standalone: true,
  template: `
    <footer class="flex shrink-0 justify-end gap-2 border-t border-line bg-surface-2/50 px-6 py-3.5">
      <button type="button" class="btn-secondary" (click)="cancel.emit()">{{ cancelLabel() }}</button>
      <button type="button" class="btn-primary" [disabled]="loading() || disabled()" (click)="save.emit()">
        @if (loading()) { <span class="spinner" aria-hidden="true"></span> }
        {{ loading() ? loadingLabel() : saveLabel() }}
      </button>
    </footer>
  `
})
export class DrawerFooterActionsComponent {
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
  cancelLabel = input<string>('Cancel');
  saveLabel = input<string>('Save');
  loadingLabel = input<string>('Saving…');
  loading = input(false);
  disabled = input(false);

  cancel = output<void>();
  save = output<void>();
}
