import { Component, computed, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateService } from '../../../core/services/translate.service';
import { totalListPages } from '../../../core/utils/pagination';

@Component({
  selector: 'app-table-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './table-pagination.component.html',
  host: {
    class: 'block w-full shrink-0',
  },
})
export class TablePaginationComponent {
  readonly translate = inject(TranslateService);

  totalItems = input.required<number>();
  page = input.required<number>();
  pageSize = input.required<number>();
  pageSizeOptions = input<number[]>([10, 25, 50, 100]);
  theme = input<'light' | 'dark'>('dark');
  pageChange = output<number>();
  pageSizeChange = output<number>();

  totalPages = computed(() => totalListPages(this.totalItems(), this.pageSize()));

  rangeLabel = computed(() => {
    const t = this.translate.t();
    const total = this.totalItems();
    const size = this.pageSize();
    const p = Math.max(1, this.page());
    if (total === 0) {
      return t.paginationShowing.replace('{{from}}', '0').replace('{{to}}', '0').replace('{{total}}', '0');
    }
    const from = (p - 1) * size + 1;
    const to = Math.min(p * size, total);
    return t.paginationShowing.replace('{{from}}', String(from)).replace('{{to}}', String(to)).replace('{{total}}', String(total));
  });

  canPrev = computed(() => this.page() > 1);
  canNext = computed(() => this.page() < this.totalPages());

  onPrev(): void {
    if (!this.canPrev()) {
      return;
    }
    this.pageChange.emit(this.page() - 1);
  }

  onNext(): void {
    if (!this.canNext()) {
      return;
    }
    this.pageChange.emit(this.page() + 1);
  }

  onPageSizeSelect(value: string): void {
    const n = parseInt(value, 10);
    if (!Number.isFinite(n)) {
      return;
    }
    this.pageSizeChange.emit(n);
  }
}
