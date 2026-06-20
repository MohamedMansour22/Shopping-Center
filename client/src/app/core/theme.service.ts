import { Injectable, computed, effect, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

const THEME_KEY = 'sc_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private themeSignal = signal<Theme>(this.load());

  readonly theme = this.themeSignal.asReadonly();
  readonly isDark = computed(() => this.themeSignal() === 'dark');

  constructor() {
    // Toggle the `dark` class on <html> and persist whenever the theme changes.
    effect(() => {
      const theme = this.themeSignal();
      localStorage.setItem(THEME_KEY, theme);
      document.documentElement.classList.toggle('dark', theme === 'dark');
    });
  }

  setTheme(theme: Theme): void {
    this.themeSignal.set(theme);
  }

  toggle(): void {
    this.themeSignal.update((t) => (t === 'light' ? 'dark' : 'light'));
  }

  // Stored preference wins; otherwise fall back to the OS color-scheme.
  private load(): Theme {
    const stored = localStorage.getItem(THEME_KEY);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
