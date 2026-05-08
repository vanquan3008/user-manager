import { Injectable, signal, effect } from '@angular/core';

const STORAGE_KEY = 'app_theme';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>(this.loadTheme());

  constructor() {
    effect(() => {
      const t = this.theme();
      document.body.classList.remove('theme-light', 'theme-dark');
      document.body.classList.add(t === 'light' ? 'theme-light' : 'theme-dark');
    });
  }

  private loadTheme(): Theme {
    if (typeof localStorage === 'undefined') return 'dark';
    const v = localStorage.getItem(STORAGE_KEY);
    return v === 'light' || v === 'dark' ? v : 'dark';
  }

  setTheme(theme: Theme) {
    this.theme.set(theme);
    localStorage.setItem(STORAGE_KEY, theme);
  }

  toggleTheme() {
    const next = this.theme() === 'light' ? 'dark' : 'light';
    this.setTheme(next);
  }
}
