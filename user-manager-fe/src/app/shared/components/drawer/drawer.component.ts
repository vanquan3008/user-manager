import { Component, HostListener, inject, input, output } from '@angular/core';
import { TranslateService } from '../../../core/services/translate.service';

@Component({
  selector: 'app-drawer',
  standalone: true,
  templateUrl: './drawer.component.html'
})
export class DrawerComponent {
  readonly translate = inject(TranslateService);

  title = input.required<string>();
  subtitle = input<string>('');
  closing = input(false);
  /** @deprecated Theme is handled by CSS tokens; kept for template compatibility. */
  theme = input<'light' | 'dark'>('dark');

  close = output<void>();

  @HostListener('document:keydown.escape')
  onEscape() { this.close.emit(); }
}
