import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  Injector,
  afterNextRender,
  computed,
  forwardRef,
  inject,
  input,
  signal,
  viewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SearchableMultiSelectOption {
  value: string;
  label: string;
  description?: string;
}

@Component({
  selector: 'app-searchable-multi-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchableMultiSelectComponent),
      multi: true
    }
  ],
  templateUrl: './searchable-multi-select.component.html'
})
export class SearchableMultiSelectComponent implements ControlValueAccessor {
  private cdr = inject(ChangeDetectorRef);
  private injector = inject(Injector);

  filterInput = viewChild<ElementRef<HTMLInputElement>>('filterInput');

  options = input<SearchableMultiSelectOption[]>([]);
  placeholder = input<string>('-- Chọn --');
  searchPlaceholder = input<string>('Tìm kiếm…');
  disabled = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');

  open = signal(false);
  query = signal('');
  selected = signal<string[]>([]);
  private controlDisabled = signal(false);
  /** Bỏ qua một vòng document:click sau khi mở (tránh đóng ngay vì thay input trong cùng chuỗi sự kiện). */
  private ignoreDocumentClick = false;
  private onChange: (v: string[]) => void = () => {};
  private onTouched: () => void = () => {};

  readonly isDisabled = computed(() => this.disabled() || this.controlDisabled());

  summaryText = computed(() => {
    const sel = this.selected();
    if (sel.length === 0) return '';
    if (sel.length === 1) return sel[0];
    return `${sel.length} mục đã chọn`;
  });

  filteredOptions = computed(() => {
    const q = this.query().toLowerCase().trim();
    const list = this.options();
    if (!q) return list;
    return list.filter(
      (o) =>
        o.value.toLowerCase().includes(q) ||
        o.label.toLowerCase().includes(q) ||
        (o.description?.toLowerCase().includes(q) ?? false)
    );
  });

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.ignoreDocumentClick) return;
    const target = event.target;
    if (!(target instanceof HTMLElement)) {
      this.closePanel();
      return;
    }
    if (!target.closest('.app-searchable-multi')) {
      this.closePanel();
    }
  }

  private armIgnoreOutsideClick() {
    this.ignoreDocumentClick = true;
    setTimeout(() => {
      this.ignoreDocumentClick = false;
    }, 0);
  }

  closePanel() {
    this.open.set(false);
    this.query.set('');
    this.cdr.markForCheck();
  }

  onMainFocus() {
    if (this.isDisabled()) return;
    if (!this.open()) {
      this.armIgnoreOutsideClick();
      this.open.set(true);
      this.query.set('');
      afterNextRender(
        () => {
          this.filterInput()?.nativeElement?.focus();
        },
        { injector: this.injector }
      );
    }
    this.onTouched();
    this.cdr.markForCheck();
  }

  toggleChevron(e: Event) {
    e.stopPropagation();
    e.preventDefault();
    if (this.isDisabled()) return;
    if (this.open()) this.closePanel();
    else {
      this.armIgnoreOutsideClick();
      this.open.set(true);
      this.query.set('');
      this.onTouched();
      afterNextRender(
        () => {
          this.filterInput()?.nativeElement?.focus();
        },
        { injector: this.injector }
      );
    }
    this.cdr.markForCheck();
  }

  isSelected(value: string): boolean {
    return this.selected().some((v) => v.toLowerCase() === value.toLowerCase());
  }

  toggleOption(value: string, e: Event) {
    e.stopPropagation();
    const cur = this.selected();
    const idx = cur.findIndex((v) => v.toLowerCase() === value.toLowerCase());
    const next = idx >= 0 ? cur.filter((_, i) => i !== idx) : [...cur, value];
    this.selected.set(next);
    this.onChange(next);
    this.cdr.markForCheck();
  }

  writeValue(v: string[] | null): void {
    this.selected.set(v?.length ? [...v] : []);
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (v: string[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.controlDisabled.set(isDisabled);
  }
}
