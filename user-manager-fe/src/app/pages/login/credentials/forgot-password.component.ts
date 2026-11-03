import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  template: `
    <app-auth-shell>
      <div class="mb-8">
        <div class="mb-5 flex h-11 w-11 items-center justify-center rounded-xl bg-brand-soft text-brand-text"><span class="material-symbols-outlined">lock_reset</span></div>
        <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ t().forgotTitle }}</h1>
        <p class="mt-1.5 text-sm text-fg-2">{{ t().forgotSubtitle }}</p>
      </div>

      @if (sent()) {
        <div class="alert-ok" role="status"><span class="material-symbols-outlined">mark_email_read</span><span>{{ t().forgotSent }}</span></div>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
          <div>
            <label class="label" for="fp-email">{{ t().authEmail }}</label>
            <input id="fp-email" type="email" class="input input-lg" formControlName="email" autocomplete="email" placeholder="you@company.com"
                   [class.input-invalid]="form.controls.email.invalid && form.controls.email.touched" autofocus />
            @if (form.controls.email.invalid && form.controls.email.touched) { <p class="error-text">{{ t().authInvalidEmail }}</p> }
          </div>
          <button type="submit" class="btn-primary btn-lg btn-block" [disabled]="loading()">
            @if (loading()) { <span class="spinner"></span> } {{ t().forgotSubmit }}
          </button>
        </form>
      }

      <a routerLink="/login" class="mt-6 inline-flex items-center gap-1 text-[13px] font-medium text-fg-2 hover:text-fg">
        <span class="material-symbols-outlined text-[18px]">arrow_back</span>{{ t().backToSignIn }}
      </a>
    </app-auth-shell>
  `
})
export class ForgotPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  readonly t = inject(TranslateService).t;

  readonly loading = signal(false);
  readonly sent = signal(false);

  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]] });

  ngOnInit() {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) this.form.patchValue({ email });
  }

  submit() {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    // Same outcome either way so the page cannot be used to discover accounts.
    const done = () => { this.loading.set(false); this.sent.set(true); };
    this.auth.requestPasswordReset(this.form.getRawValue().email).subscribe({ next: done, error: done });
  }
}
