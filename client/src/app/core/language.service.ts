import { Injectable, computed, effect, signal } from '@angular/core';
import { Lang, interpolate, translations } from './i18n';

const LANG_KEY = 'sc_lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private langSignal = signal<Lang>(this.load());

  readonly lang = this.langSignal.asReadonly();
  readonly isRtl = computed(() => this.langSignal() === 'ar');

  constructor() {
    // Reflect the active language onto <html> (lang + dir) and persist it.
    effect(() => {
      const lang = this.langSignal();
      localStorage.setItem(LANG_KEY, lang);
      const root = document.documentElement;
      root.lang = lang;
      root.dir = lang === 'ar' ? 'rtl' : 'ltr';
    });
  }

  setLang(lang: Lang): void {
    this.langSignal.set(lang);
  }

  toggle(): void {
    this.langSignal.update((l) => (l === 'en' ? 'ar' : 'en'));
  }

  // Translate a key for the current language, with optional {token} params.
  t(key: string, params?: Record<string, string | number>): string {
    const dict = translations[this.langSignal()] as Record<string, string>;
    const fallback = translations.en as Record<string, string>;
    return interpolate(dict[key] ?? fallback[key] ?? key, params);
  }

  private load(): Lang {
    const stored = localStorage.getItem(LANG_KEY);
    return stored === 'ar' || stored === 'en' ? stored : 'en';
  }
}
