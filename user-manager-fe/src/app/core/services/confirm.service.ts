import { Injectable, signal } from '@angular/core';

export type ConfirmVariant = 'default' | 'danger';

export interface ConfirmOptions {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  variant?: ConfirmVariant;
}

type ConfirmState = {
  open: boolean;
  options: ConfirmOptions | null;
  resolve: ((value: boolean) => void) | null;
};

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private state = signal<ConfirmState>({ open: false, options: null, resolve: null });
  readonly vm = this.state.asReadonly();

  confirm(options: ConfirmOptions): Promise<boolean> {
    // Close any existing modal (resolve false).
    const prev = this.state();
    if (prev.open && prev.resolve) prev.resolve(false);

    return new Promise<boolean>((resolve) => {
      this.state.set({
        open: true,
        options: {
          title: options.title?.trim() || 'Xác nhận',
          message: options.message,
          confirmText: options.confirmText?.trim() || 'OK',
          cancelText: options.cancelText?.trim() || 'Hủy',
          variant: options.variant || 'default'
        },
        resolve
      });
    });
  }

  cancel() {
    const { resolve } = this.state();
    if (resolve) resolve(false);
    this.state.set({ open: false, options: null, resolve: null });
  }

  accept() {
    const { resolve } = this.state();
    if (resolve) resolve(true);
    this.state.set({ open: false, options: null, resolve: null });
  }
}

