import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  ElementRef,
  Renderer2,
  ViewChild,
  computed,
  forwardRef,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { CommonModule, DOCUMENT } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslateService } from '../../../core/services/translate.service';

export interface MultiSelectOption {
  value: string;
  label: string;
  /** Role không có trong catalog — tô màu cảnh báo */
  variant?: 'default' | 'warning';
}

@Component({
  selector: 'app-multi-select-dropdown',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultiSelectDropdownComponent),
      multi: true
    }
  ],
  templateUrl: './multi-select-dropdown.component.html'
})
export class MultiSelectDropdownComponent implements ControlValueAccessor {
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);
  private document = inject(DOCUMENT);
  private renderer = inject(Renderer2);
  translate = inject(TranslateService);

  @ViewChild('triggerBtn', { read: ElementRef }) private triggerRef?: ElementRef<HTMLElement>;
  @ViewChild('hostRoot', { read: ElementRef }) private hostRef?: ElementRef<HTMLElement>;
  @ViewChild('dropdownPanel', { read: ElementRef }) private panelRef?: ElementRef<HTMLElement>;

  readonly panelStyle = signal<Record<string, string> | null>(null);
  private positionRetry = 0;

  options = input<MultiSelectOption[]>([]);
  placeholder = input<string>('-- Chọn --');
  disabled = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');

  valueChange = output<string[]>();

  open = signal(false);
  /** Giá trị đã chọn (value của option) */
  selected = signal<string[]>([]);
  private controlDisabled = signal(false);
  private onChange: (v: string[]) => void = () => {};
  private onTouched: () => void = () => {};
  readonly isDisabled = computed(() => this.disabled() || this.controlDisabled());

  constructor() {
    const reposition = () => {
      if (this.open()) {
        this.updatePanelPosition();
      }
    };
    const onPointerDown = (e: PointerEvent) => {
      const target = e.target as HTMLElement;
      const inside =
        target.closest?.('.app-multi-dropdown') || target.closest?.('.app-multi-dropdown-panel');
      if (!inside) {
        this.closePanel();
      }
    };
    this.document.addEventListener('scroll', reposition, true);
    this.document.addEventListener('pointerdown', onPointerDown, true);
    this.document.defaultView?.addEventListener('resize', reposition);
    this.destroyRef.onDestroy(() => {
      this.document.removeEventListener('scroll', reposition, true);
      this.document.removeEventListener('pointerdown', onPointerDown, true);
      this.document.defaultView?.removeEventListener('resize', reposition);
      this.attachPanelToHost();
    });
  }

  private attachPanelToBody(): void {
    const panel = this.panelRef?.nativeElement;
    const host = this.hostRef?.nativeElement;
    if (!panel || !host || panel.parentElement === this.document.body) return;
    this.renderer.appendChild(this.document.body, panel);
  }

  private attachPanelToHost(): void {
    const panel = this.panelRef?.nativeElement;
    const host = this.hostRef?.nativeElement;
    if (!panel || !host || panel.parentElement === host) return;
    this.renderer.appendChild(host, panel);
  }

  /**
   * Một role: chỉ tên (value), không ghép label dài.
   * Nhiều role: chỉ số lượng — không hiển thị CSV trên nút.
   */
  triggerSummary(): string {
    const vals = this.selected();
    const n = vals.length;
    if (n === 0) return '';
    if (n === 1) return vals[0];
    return `Đã chọn ${n} role`;
  }

  private closePanel(): void {
    if (!this.open()) return;
    this.open.set(false);
    this.attachPanelToHost();
    this.panelStyle.set(null);
    this.positionRetry = 0;
  }

  onTriggerClick(e: MouseEvent): void {
    e.stopPropagation();
    this.toggle();
  }

  isSelected(value: string): boolean {
    return this.selected().includes(value);
  }

  toggle() {
    if (this.isDisabled()) return;
    this.open.update(v => !v);
    if (this.open()) {
      this.positionRetry = 0;
      queueMicrotask(() => {
        this.attachPanelToBody();
        this.updatePanelPosition();
      });
    } else {
      this.attachPanelToHost();
      this.panelStyle.set(null);
      this.positionRetry = 0;
    }
    this.onTouched();
  }

  private updatePanelPosition(): void {
    const btn = this.triggerRef?.nativeElement;
    if (!btn) {
      if (this.open() && this.positionRetry < 20) {
        this.positionRetry++;
        requestAnimationFrame(() => this.updatePanelPosition());
      }
      return;
    }
    this.positionRetry = 0;
    const r = btn.getBoundingClientRect();
    const margin = 6;
    const spaceBelow = window.innerHeight - r.bottom - margin;
    const maxH = Math.max(120, Math.min(280, spaceBelow - 4, window.innerHeight * 0.6));
    this.panelStyle.set({
      top: `${r.bottom + margin}px`,
      left: `${r.left}px`,
      width: `${r.width}px`,
      maxHeight: `${maxH}px`
    });
    this.cdr.markForCheck();
  }

  toggleOption(opt: MultiSelectOption, e: Event) {
    e.stopPropagation();
    const cur = [...this.selected()];
    const i = cur.indexOf(opt.value);
    if (i >= 0) {
      cur.splice(i, 1);
    } else {
      cur.push(opt.value);
    }
    this.selected.set(cur);
    this.onChange(cur);
    this.valueChange.emit(cur);
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
    this.cdr.markForCheck();
  }
}
