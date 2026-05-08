import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-icon-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      type="button"
      class="inline-flex items-center justify-center rounded-2xl transition-all duration-200 active:scale-[0.98] disabled:pointer-events-none disabled:opacity-60"
      [ngClass]="buttonClasses()"
      [disabled]="disabled()"
      [attr.title]="title()"
      [attr.aria-label]="ariaLabel() || title()"
    >
      <span class="material-symbols-outlined" [ngClass]="size() === 'md' ? 'text-[20px]' : 'text-[18px]'">{{ icon() }}</span>
    </button>
  `
})
export class IconButtonComponent {
  icon = input.required<string>();
  variant = input<'primary' | 'secondary' | 'ghost'>('primary');
  theme = input<'light' | 'dark'>('dark');
  title = input<string>('');
  ariaLabel = input<string>('');
  disabled = input(false);
  size = input<'sm' | 'md'>('md');

  buttonClasses(): string {
    const sizeClass = this.size() === 'md' ? 'h-11 w-11' : 'h-9 w-9';

    if (this.variant() === 'primary') {
      return `${sizeClass} bg-slate-900 text-white shadow-sm hover:bg-slate-700 ${this.theme() === 'dark' ? 'bg-sky-500 hover:bg-sky-400' : ''}`;
    }

    if (this.variant() === 'ghost') {
      return `${sizeClass} ${this.theme() === 'light'
        ? 'text-slate-500 hover:bg-slate-100 hover:text-slate-900'
        : 'text-slate-400 hover:bg-slate-800 hover:text-slate-100'}`;
    }

    return `${sizeClass} border ${this.theme() === 'light'
      ? 'border-slate-200 bg-white text-slate-600 shadow-sm hover:bg-slate-50 hover:text-slate-900'
      : 'border-slate-700 bg-slate-800/80 text-slate-300 hover:bg-slate-700 hover:text-slate-100'}`;
  }
}
