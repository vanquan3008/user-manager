import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-drawer-footer-actions',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="flex shrink-0 justify-end gap-3 px-6 py-4"
      [ngClass]="theme() === 'light' ? 'bg-slate-50/70' : 'bg-slate-900/75'"
    >
      <button
        type="button"
        class="rounded-lg border px-4 py-2.5 text-sm font-medium transition"
        [ngClass]="theme() === 'light'
          ? 'border-slate-200 text-slate-700 hover:bg-slate-100'
          : 'border-slate-600 text-slate-200 hover:bg-slate-700'"
        (click)="cancel.emit()"
      >
        {{ cancelLabel() }}
      </button>
      <button
        type="button"
        class="inline-flex items-center justify-center gap-2 rounded-lg px-5 py-2.5 text-sm font-medium text-white shadow-sm transition hover:shadow disabled:opacity-70 bg-sky-500 hover:bg-sky-600"
        [disabled]="loading()"
        (click)="save.emit()"
      >
        @if (loading()) {
          <span
            class="inline-flex size-[1em] shrink-0 items-center justify-center self-center"
            aria-hidden="true"
          >
            <span class="material-symbols-outlined block animate-spin text-[1em] leading-none">
              progress_activity
            </span>
          </span>
        }
        {{ loading() ? loadingLabel() : saveLabel() }}
      </button>
    </div>
  `
})
export class DrawerFooterActionsComponent {
  theme = input<'light' | 'dark'>('dark');
  cancelLabel = input<string>('Hủy');
  saveLabel = input<string>('Lưu');
  loadingLabel = input<string>('Đang xử lý…');
  loading = input(false);

  cancel = output<void>();
  save = output<void>();
}
