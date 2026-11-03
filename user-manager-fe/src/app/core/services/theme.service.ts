import { Injectable, effect, signal } from '@angular/core';

const STORAGE_KEY = 'app_theme';

export type Theme = 'light' | 'dark';

/** Light/dark theme. Defaults to the OS preference until the user picks one explicitly. */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>(this.loadTheme());

  constructor() {
    effect(() => {
      const dark = this.theme() === 'dark';
      document.documentElement.classList.toggle('dark', dark);
      document.body.classList.toggle('theme-dark', dark);
      document.body.classList.toggle('theme-light', !dark);
    });
  }

  setTheme(theme: Theme) {
    this.theme.set(theme);
    try { localStorage.setItem(STORAGE_KEY, theme); } catch { /* storage unavailable */ }
  }

  toggleTheme() {
    this.setTheme(this.theme() === 'light' ? 'dark' : 'light');
  }

  private loadTheme(): Theme {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored === 'light' || stored === 'dark') return stored;
    } catch { /* storage unavailable */ }
    return typeof window !== 'undefined' && window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
