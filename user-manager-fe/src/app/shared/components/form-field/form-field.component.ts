import { Component, input, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

let nextFieldId = 0;

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="mb-1.5 flex items-center justify-between gap-3">
        <label class="text-[13px] font-medium text-fg" [attr.for]="fieldId">
          {{ label() }}@if (required()) {<span class="ml-0.5 text-bad">*</span>}
        </label>
        @if (hint()) { <span class="text-xs text-fg-3">{{ hint() }}</span> }
      </div>

      @if (type() === 'textarea') {
        <textarea [id]="fieldId" class="input resize-y" [class.input-invalid]="invalid()"
          [ngModel]="model()" (ngModelChange)="onModelChange($event)" [placeholder]="placeholder()"
          [readOnly]="readonly()" [disabled]="disabled()" [rows]="rows()" [attr.aria-invalid]="invalid()"></textarea>
      } @else if (useCompositeField()) {
        <div class="flex w-full min-w-0 overflow-hidden rounded-lg border bg-surface shadow-xs transition focus-within:border-brand focus-within:ring-4 focus-within:ring-brand/15"
             [class]="invalid() ? 'border-bad' : 'border-line-strong'">
          <input [id]="fieldId" [type]="type()" class="h-9 w-full min-w-0 border-0 bg-transparent px-3 text-sm text-fg outline-none placeholder:text-fg-3"
            [ngModel]="displayForInput()" (ngModelChange)="onInputChange($event)" [placeholder]="resolvedPlaceholder()"
            [readonly]="readonly()" [disabled]="disabled()" [min]="min()" [max]="max()" [attr.aria-invalid]="invalid()" />
          @if (suffix()) {
            <span class="flex shrink-0 items-center border-l border-line bg-surface-2 px-3 text-xs font-medium tabular-nums text-fg-2">{{ suffix() }}</span>
          }
        </div>
      } @else {
        <input [id]="fieldId" [type]="type()" class="input" [class.input-invalid]="invalid()"
          [ngModel]="model()" (ngModelChange)="onModelChange($event)" [placeholder]="placeholder()"
          [readonly]="readonly()" [disabled]="disabled()" [min]="min()" [max]="max()" [attr.aria-invalid]="invalid()" />
      }

      @if (invalid() && error()) { <p class="error-text">{{ error() }}</p> }
    </div>
  `
})
export class FormFieldComponent {
  readonly fieldId = `ff-${++nextFieldId}`;
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
  /** @deprecated kept for template compatibility */
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
      return 'Leave empty for unlimited';
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




}
