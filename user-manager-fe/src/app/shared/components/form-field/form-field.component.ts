import { Component, input, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-2.5">
      <div class="flex items-center justify-between gap-3">
        <label
          class="block text-sm font-semibold"
          [ngClass]="theme() === 'light' ? 'text-slate-700' : 'text-slate-200'"
        >
          {{ label() }}
          @if (required()) {
            <span class="text-rose-500">*</span>
          }
        </label>
        @if (hint()) {
          <span class="text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">{{ hint() }}</span>
        }
      </div>

      @if (type() === 'textarea') {
        <textarea
          [ngModel]="model()"
          (ngModelChange)="onModelChange($event)"
          [placeholder]="placeholder()"
          [readOnly]="readonly()"
          [disabled]="disabled()"
          [rows]="rows()"
          [attr.aria-invalid]="invalid()"
          class="w-full resize-y rounded-lg border px-4 py-3 text-sm leading-6 outline-none transition focus:border-sky-400 focus:ring-2 focus:ring-sky-500/20 disabled:opacity-70"
          [ngClass]="fieldClasses()"
        ></textarea>
      } @else if (useCompositeField()) {
        <div
          class="flex w-full min-w-0 overflow-hidden rounded-lg border transition focus-within:border-sky-400 focus-within:ring-2 focus-within:ring-sky-500/20 disabled:opacity-70"
          [ngClass]="compositeContainerClasses()"
        >
          <div class="relative min-w-0 flex-1">
            <input
              [type]="type()"
              [ngModel]="displayForInput()"
              (ngModelChange)="onInputChange($event)"
              [placeholder]="resolvedPlaceholder()"
              [readonly]="readonly()"
              [disabled]="disabled()"
              [min]="min()"
              [max]="max()"
              [attr.aria-invalid]="invalid()"
              class="w-full min-w-0 border-0 bg-transparent px-4 py-3 text-sm outline-none ring-0 focus:ring-0 disabled:opacity-70"
              [ngClass]="innerInputClasses()"
            />
          </div>
          @if (suffix()) {
            <span
              class="flex shrink-0 items-center border-l px-3 py-2 text-xs font-medium tabular-nums"
              [ngClass]="suffixSpanClasses()"
            >
              {{ suffix() }}
            </span>
          }
        </div>
      } @else {
        <input
          [type]="type()"
          [ngModel]="model()"
          (ngModelChange)="onModelChange($event)"
          [placeholder]="placeholder()"
          [readonly]="readonly()"
          [disabled]="disabled()"
          [min]="min()"
          [max]="max()"
          [attr.aria-invalid]="invalid()"
          class="w-full rounded-lg border px-4 py-3 text-sm outline-none transition focus:border-sky-400 focus:ring-2 focus:ring-sky-500/20 disabled:opacity-70"
          [ngClass]="fieldClasses()"
        />
      }

      @if (invalid() && error()) {
        <p class="text-xs font-medium" [ngClass]="theme() === 'light' ? 'text-rose-600' : 'text-rose-300'">
          {{ error() }}
        </p>
      }
    </div>
  `
})
export class FormFieldComponent {
  label = input.required<string>();
  model = model<string>('');
  type = input<'text' | 'email' | 'number' | 'date' | 'password' | 'textarea'>('text');
  placeholder = input<string>('');
  required = input(false);
  readonly = input(false);
  disabled = input(false);
  min = input<number | string | undefined>(undefined);
  max = input<number | string | undefined>(undefined);
  hint = input<string>('');
  invalid = input(false);
  error = input<string>('');
  rows = input(4);
  theme = input<'light' | 'dark'>('dark');

  /** Shown on the right inside the field border (e.g. VND, đơn vị). */
  suffix = input<string>('');

  /**
   * Khi set (ví dụ -1): ô trống + placeholder; không nhập số thì model lưu sentinel.
   */
  unlimitedSentinel = input<number | null>(null);

  onModelChange(value: string | number | null): void {
    if (value === null || value === undefined) {
      this.model.set('');
      return;
    }
    this.model.set(typeof value === 'number' ? String(value) : value);
  }

  /** Chuỗi hiển thị trong ô (sentinel → hiện như trống để thấy placeholder). */
  displayForInput(): string {
    if (!this.shouldUseUnlimitedEmptyDisplay()) {
      return this.stringModel();
    }
    const sent = this.unlimitedSentinel()!;
    const v = this.stringModel().trim();
    if (v === '') return '';
    const n = Number(v);
    if (!Number.isNaN(n) && n === sent) return '';
    return this.stringModel();
  }

  onInputChange(value: string | number | null): void {
    if (!this.shouldUseUnlimitedEmptyDisplay()) {
      this.onModelChange(value);
      return;
    }
    const sent = this.unlimitedSentinel()!;
    if (value === '' || value === null || value === undefined) {
      this.model.set(String(sent));
      return;
    }
    const s = typeof value === 'number' ? String(value) : String(value).trim();
    if (s === '') {
      this.model.set(String(sent));
      return;
    }
    this.model.set(s);
  }

  resolvedPlaceholder(): string {
    const p = this.placeholder();
    if (p) return p;
    if (this.shouldUseUnlimitedEmptyDisplay()) {
      return 'Nhập số (để trống = tất cả)';
    }
    return '';
  }

  shouldUseUnlimitedEmptyDisplay(): boolean {
    return this.unlimitedSentinel() !== null && this.type() === 'number';
  }

  private stringModel(): string {
    const v = this.model() as unknown;
    if (v === null || v === undefined) return '';
    return String(v);
  }

  useCompositeField(): boolean {
    return (
      !!this.suffix() ||
      (this.unlimitedSentinel() !== null && this.type() === 'number')
    );
  }

  innerInputClasses(): string {
    const themeText = this.theme() === 'light' ? 'text-slate-900' : 'text-slate-100';
    const placeholder = this.theme() === 'light' ? 'placeholder:text-slate-400' : 'placeholder:text-slate-500';
    return `${themeText} ${placeholder}`;
  }

  compositeContainerClasses(): string {
    const light = this.theme() === 'light'
      ? 'border-slate-200 bg-white'
      : 'border-slate-600 bg-slate-900/80';

    if (!this.invalid()) {
      return light;
    }

    return `${light} motion-invalid border-rose-300 bg-rose-50/60 ${this.theme() === 'light' ? '' : 'border-rose-500/40 bg-rose-500/10'}`;
  }

  suffixSpanClasses(): string {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-slate-50 text-slate-600'
      : 'border-slate-600 bg-slate-800/80 text-slate-400';
  }

  fieldClasses(): string {
    const base = this.theme() === 'light'
      ? 'border-slate-200 bg-white text-slate-900 placeholder:text-slate-400 disabled:bg-slate-100'
      : 'border-slate-600 bg-slate-900/80 text-slate-100 placeholder:text-slate-500 disabled:bg-slate-800';

    if (!this.invalid()) {
      return base;
    }

    return `${base} motion-invalid border-rose-300 bg-rose-50/60 focus:border-rose-400 focus:ring-rose-500/10 ${this.theme() === 'light' ? 'text-slate-900' : 'border-rose-500/40 bg-rose-500/10 text-slate-100'}`;
  }
}
