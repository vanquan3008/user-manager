import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
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

export interface DropdownOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownComponent),
      multi: true
    }
  ],
  templateUrl: './dropdown.component.html'
})
export class DropdownComponent implements ControlValueAccessor {
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);
  private document = inject(DOCUMENT);
  private renderer = inject(Renderer2);
  translate = inject(TranslateService);

  @ViewChild('triggerBtn', { read: ElementRef }) private triggerRef?: ElementRef<HTMLElement>;
  @ViewChild('hostRoot', { read: ElementRef }) private hostRef?: ElementRef<HTMLElement>;
  @ViewChild('dropdownPanel', { read: ElementRef }) private panelRef?: ElementRef<HTMLElement>;

  /** fixed + px — panel gắn vào body nên tọa độ theo viewport */
  readonly panelStyle = signal<Record<string, string> | null>(null);
  private positionRetry = 0;

  options = input<DropdownOption[]>([]);
  placeholder = input<string>('-- Chọn --');
  disabled = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');

  valueChange = output<string>();

  open = signal(false);
  value = signal<string>('');
  private controlDisabled = signal(false);
  private onChange: (v: string) => void = () => {};
  private onTouched: () => void = () => {};
  readonly isDisabled = computed(() => this.disabled() || this.controlDisabled());

  constructor() {
    const reposition = () => {
      if (this.open()) {
        this.updatePanelPosition();
      }
    };
    this.document.addEventListener('scroll', reposition, true);
    this.document.defaultView?.addEventListener('resize', reposition);
    this.destroyRef.onDestroy(() => {
      this.document.removeEventListener('scroll', reposition, true);
      this.document.defaultView?.removeEventListener('resize', reposition);
      this.attachPanelToHost();
    });
  }

  /** Drawer/modal dùng `transform` → `position:fixed` con bị neo sai; gắn panel vào body. */
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

  get selectedLabel(): string {
    const v = this.value();
    if (!v) return '';
    const opt = this.options().find(o => o.value === v);
    return opt?.label ?? v;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent) {
    const target = e.target as HTMLElement;
    const inside =
      target.closest?.('.app-dropdown') || target.closest?.('.app-dropdown-panel');
    if (!inside) {
      if (this.open()) {
        this.attachPanelToHost();
        this.panelStyle.set(null);
      }
      this.open.set(false);
    }
  }

  toggle() {
    if (this.isDisabled()) return;
    this.open.update((v) => !v);
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
      maxHeight: `${maxH}px`,
    });
    this.cdr.markForCheck();
  }

  select(opt: DropdownOption) {
    this.value.set(opt.value);
    this.onChange(opt.value);
    this.valueChange.emit(opt.value);
    this.open.set(false);
    this.attachPanelToHost();
    this.panelStyle.set(null);
    this.cdr.markForCheck();
  }

  writeValue(v: string): void {
    this.value.set(v ?? '');
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (v: string) => void): void {
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
