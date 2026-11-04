import { Component, ElementRef, HostListener, computed, inject, input, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateService } from '../../../core/services/translate.service';

export type PaletteNavItem = { path: string; icon: string; label: string };
export type PaletteNavGroup = { label: string; items: PaletteNavItem[] };

type Command = { id: string; group: string; icon: string; label: string; hint?: string; keywords: string; run: () => void };

/**
 * Keyboard-first navigation (⌘K / Ctrl+K): jump to any console page or run an account action.
 * Arrow keys move the selection, Enter runs it, Escape closes.
 */
@Component({
  selector: 'app-command-palette',
  standalone: true,
  template: `
    @if (isOpen()) {
      <div class="overlay-backdrop" (click)="close()" aria-hidden="true"></div>
      <div class="fixed inset-x-0 top-[12vh] z-[61] mx-auto w-[calc(100%-2rem)] max-w-xl overflow-hidden rounded-2xl border border-line bg-surface shadow-pop animate-scale-in"
           role="dialog" aria-modal="true" [attr.aria-label]="t().paletteTitle">
        <div class="flex items-center gap-3 border-b border-line px-4">
          <span class="material-symbols-outlined text-[20px] text-fg-3">search</span>
          <input #queryInput type="text" class="h-12 w-full bg-transparent text-[15px] text-fg placeholder:text-fg-3 focus:outline-none"
                 [placeholder]="t().palettePlaceholder" [value]="query()" (input)="onQuery($any($event.target).value)"
                 (keydown)="onKeydown($event)" role="combobox" aria-expanded="true" aria-controls="palette-list"
                 [attr.aria-activedescendant]="results()[active()] ? 'cmd-' + results()[active()].id : null" autocomplete="off" spellcheck="false" />
          <span class="kbd hidden sm:inline">Esc</span>
        </div>

        <ul id="palette-list" role="listbox" class="max-h-[min(60vh,420px)] overflow-y-auto p-2">
          @for (cmd of results(); track cmd.id; let i = $index) {
            @if (i === 0 || results()[i - 1].group !== cmd.group) {
              <li role="presentation" class="px-2.5 pb-1 pt-2.5 text-2xs font-semibold uppercase tracking-[0.08em] text-fg-3 first:pt-1">{{ cmd.group }}</li>
            }
            <li [id]="'cmd-' + cmd.id" role="option" [attr.aria-selected]="i === active()"
                class="flex cursor-pointer items-center gap-3 rounded-lg px-2.5 py-2 text-sm"
                [class.bg-surface-2]="i === active()" [class.text-fg]="i === active()" [class.text-fg-2]="i !== active()"
                (mousemove)="active.set(i)" (click)="run(cmd)">
              <span class="material-symbols-outlined text-[18px]" [class.text-brand-text]="i === active()">{{ cmd.icon }}</span>
              <span class="flex-1 truncate">{{ cmd.label }}</span>
              @if (cmd.hint) { <span class="truncate text-xs text-fg-3">{{ cmd.hint }}</span> }
              @if (i === active()) { <span class="material-symbols-outlined text-[16px] text-fg-3">keyboard_return</span> }
            </li>
          } @empty {
            <li class="px-3 py-10 text-center text-sm text-fg-3">{{ t().paletteEmpty }}</li>
          }
        </ul>

        <div class="flex items-center gap-4 border-t border-line bg-surface-2/50 px-4 py-2 text-xs text-fg-3">
          <span class="inline-flex items-center gap-1.5"><span class="kbd">↑</span><span class="kbd">↓</span>{{ t().paletteNavigate }}</span>
          <span class="inline-flex items-center gap-1.5"><span class="kbd">↵</span>{{ t().paletteOpen }}</span>
        </div>
      </div>
    }
  `
})
export class CommandPaletteComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly theme = inject(ThemeService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  groups = input<PaletteNavGroup[]>([]);

  readonly isOpen = signal(false);
  readonly query = signal('');
  readonly active = signal(0);
  private readonly queryInput = viewChild<ElementRef<HTMLInputElement>>('queryInput');
  private returnFocus: HTMLElement | null = null;

  private readonly commands = computed<Command[]>(() => {
    const t = this.t();
    const pages: Command[] = this.groups().flatMap(g =>
      g.items.map(item => ({
        id: 'nav' + item.path.replace(/\W/g, '-'),
        group: t.palettePages,
        icon: item.icon,
        label: item.label,
        hint: g.label,
        keywords: `${item.label} ${g.label} ${item.path}`.toLowerCase(),
        run: () => this.router.navigateByUrl(item.path)
      }))
    );
    const dark = this.theme.theme() === 'dark';
    const actions: Command[] = [
      { id: 'act-account', group: t.paletteActions, icon: 'person', label: t.navAccount, keywords: 'account profile', run: () => this.router.navigateByUrl('/account') },
      { id: 'act-mfa', group: t.paletteActions, icon: 'phonelink_lock', label: t.paletteSetupMfa, keywords: 'mfa 2fa totp authenticator security', run: () => this.router.navigate(['/account'], { queryParams: { tab: 'security' } }) },
      { id: 'act-sessions', group: t.paletteActions, icon: 'devices', label: t.paletteMySessions, keywords: 'sessions devices sign out', run: () => this.router.navigate(['/account'], { queryParams: { tab: 'sessions' } }) },
      { id: 'act-theme', group: t.paletteActions, icon: dark ? 'light_mode' : 'dark_mode', label: dark ? t.themeLight : t.themeDark, keywords: 'theme dark light mode appearance', run: () => this.theme.toggleTheme() },
      { id: 'act-signout', group: t.paletteActions, icon: 'logout', label: t.signOut, keywords: 'sign out logout', run: () => this.auth.logout() }
    ];
    return [...pages, ...actions];
  });

  readonly results = computed(() => {
    const terms = this.query().trim().toLowerCase().split(/\s+/).filter(Boolean);
    if (!terms.length) return this.commands();
    return this.commands().filter(c => terms.every(term => c.keywords.includes(term) || c.label.toLowerCase().includes(term)));
  });

  @HostListener('document:keydown', ['$event'])
  onGlobalKeydown(event: KeyboardEvent) {
    if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.isOpen() ? this.close() : this.open();
    }
  }

  open() {
    this.returnFocus = document.activeElement as HTMLElement | null;
    this.query.set('');
    this.active.set(0);
    this.isOpen.set(true);
    setTimeout(() => this.queryInput()?.nativeElement.focus());
  }

  close() {
    this.isOpen.set(false);
    this.returnFocus?.focus?.();
  }

  onQuery(value: string) {
    this.query.set(value);
    this.active.set(0);
  }

  onKeydown(event: KeyboardEvent) {
    const count = this.results().length;
    if (event.key === 'ArrowDown' && count) {
      event.preventDefault();
      this.active.update(i => (i + 1) % count);
      this.scrollActiveIntoView();
    } else if (event.key === 'ArrowUp' && count) {
      event.preventDefault();
      this.active.update(i => (i - 1 + count) % count);
      this.scrollActiveIntoView();
    } else if (event.key === 'Enter') {
      event.preventDefault();
      const cmd = this.results()[this.active()];
      if (cmd) this.run(cmd);
    } else if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      this.close();
    } else if (event.key === 'Tab') {
      event.preventDefault(); // keep focus inside the dialog
    }
  }

  run(cmd: Command) {
    this.isOpen.set(false);
    this.returnFocus = null;
    cmd.run();
  }

  private scrollActiveIntoView() {
    setTimeout(() => {
      const cmd = this.results()[this.active()];
      if (cmd) document.getElementById('cmd-' + cmd.id)?.scrollIntoView({ block: 'nearest' });
    });
  }
}
