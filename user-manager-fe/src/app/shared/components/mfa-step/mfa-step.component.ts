import { Component, ElementRef, ViewChild, AfterViewInit, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateService } from '../../../core/services/translate.service';

/** Second sign-in step: TOTP (6 digits) or a recovery code. */
@Component({
  selector: 'app-mfa-step',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="mb-8">
      <div class="mb-5 flex h-11 w-11 items-center justify-center rounded-xl bg-brand-soft text-brand-text">
        <span class="material-symbols-outlined">{{ recovery() ? 'key' : 'phonelink_lock' }}</span>
      </div>
      <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ t().mfaTitle }}</h1>
      <p class="mt-1.5 text-sm text-fg-2">{{ recovery() ? t().mfaRecoverySubtitle : t().mfaSubtitle }}</p>
      @if (email()) {
        <p class="mt-3 inline-flex items-center gap-1.5 rounded-md bg-surface-2 px-2 py-1 text-xs text-fg-2">
          <span class="material-symbols-outlined text-[14px]">person</span>{{ t().mfaSigningInAs }} <span class="font-medium text-fg">{{ email() }}</span>
        </p>
      }
    </div>

    <form (ngSubmit)="submit()" class="space-y-5" novalidate>
      <div>
        <label class="label" for="mfa-code">{{ recovery() ? t().mfaRecoveryCode : t().mfaCode }}</label>
        @if (recovery()) {
          <input #codeInput id="mfa-code" name="recoveryCode" class="input input-lg font-mono uppercase tracking-wider" autocomplete="off"
                 [(ngModel)]="code" placeholder="XXXXXX-XXXXXX" [disabled]="loading()" />
        } @else {
          <input #codeInput id="mfa-code" name="code" class="code-input" inputmode="numeric" autocomplete="one-time-code" maxlength="6"
                 [ngModel]="code" (ngModelChange)="onTotpInput($event)" placeholder="••••••" [disabled]="loading()" />
        }
      </div>

      @if (error()) {
        <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
      }

      <button type="submit" class="btn-primary btn-lg btn-block" [disabled]="loading() || !canSubmit()">
        @if (loading()) { <span class="spinner"></span> {{ t().mfaVerifying }} } @else { {{ t().mfaVerify }} }
      </button>
    </form>

    <div class="mt-6 flex items-center justify-between text-[13px]">
      <button type="button" class="link" (click)="toggleRecovery()">{{ recovery() ? t().mfaUseAuthenticator : t().mfaUseRecovery }}</button>
      <button type="button" class="font-medium text-fg-2 hover:text-fg" (click)="back.emit()">{{ t().mfaBack }}</button>
    </div>
  `
})
export class MfaStepComponent implements AfterViewInit {
  readonly t = inject(TranslateService).t;
  @ViewChild('codeInput') private codeInput?: ElementRef<HTMLInputElement>;

  email = input<string>('');
  loading = input(false);
  error = input<string>('');
  verify = output<string>();
  back = output<void>();

  readonly recovery = signal(false);
  code = '';

  ngAfterViewInit() { this.focus(); }

  canSubmit() {
    return this.recovery() ? this.code.replace(/[^a-z0-9]/gi, '').length >= 8 : /^\d{6}$/.test(this.code);
  }

  onTotpInput(value: string) {
    this.code = (value ?? '').replace(/\D/g, '').slice(0, 6);
    if (this.codeInput) this.codeInput.nativeElement.value = this.code;
    if (this.code.length === 6 && !this.loading()) this.submit();
  }

  toggleRecovery() {
    this.recovery.update(v => !v);
    this.code = '';
    setTimeout(() => this.focus());
  }

  submit() {
    if (!this.canSubmit() || this.loading()) return;
    this.verify.emit(this.code.trim());
  }

  private focus() { this.codeInput?.nativeElement.focus(); }
}
