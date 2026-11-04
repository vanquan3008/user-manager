import { Component, input } from '@angular/core';

@Component({
  selector: 'app-icon-button',
  standalone: true,
  template: `
    <button type="button" [class]="buttonClasses()" [disabled]="disabled()" [attr.title]="title() || null" [attr.aria-label]="ariaLabel() || title()">
      <span class="material-symbols-outlined" [style.font-size.px]="size() === 'sm' ? 18 : 20">{{ icon() }}</span>
    </button>
  `
})
export class IconButtonComponent {
  icon = input.required<string>();
  variant = input<'primary' | 'secondary' | 'ghost' | 'danger'>('ghost');
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
  title = input<string>('');
  ariaLabel = input<string>('');
  disabled = input(false);
  size = input<'sm' | 'md'>('md');

  buttonClasses(): string {
    const size = this.size() === 'md' ? 'h-9 w-9' : 'h-8 w-8';
    const variant = {
      primary: 'btn-primary',
      secondary: 'btn-secondary',
      ghost: 'btn-ghost',
      danger: 'btn-danger-ghost'
    }[this.variant()];
    return `${variant} ${size} px-0`;
  }
}
