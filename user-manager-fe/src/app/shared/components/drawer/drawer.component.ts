import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateService } from '../../../core/services/translate.service';

@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drawer.component.html'
})
export class DrawerComponent {
  readonly translate = inject(TranslateService);

  /** Tiêu đề drawer */
  title = input.required<string>();
  /** Đang đóng (animation slide out) */
  closing = input(false);
  /** Theme light/dark */
  theme = input<'light' | 'dark'>('dark');

  /** Emit khi user click đóng (backdrop hoặc nút X) */
  close = output<void>();
}
