import { Component, computed, inject } from '@angular/core';
import { LANGUAGES } from '../core/i18n';
import { LanguageService } from '../core/language.service';
import { ThemeService } from '../core/theme.service';

// Theme + language switchers, shared by the storefront and admin layouts.
// Buttons use currentColor so they adapt to whatever bar they sit in.
@Component({
  selector: 'app-settings-controls',
  template: `
    <div class="topbar-actions">
      <button
        type="button"
        class="icon-btn"
        (click)="theme.toggle()"
        [attr.aria-label]="theme.isDark() ? i18n.t('common.lightMode') : i18n.t('common.darkMode')"
        [title]="theme.isDark() ? i18n.t('common.lightMode') : i18n.t('common.darkMode')"
      >
        {{ theme.isDark() ? '☀' : '☾' }}
      </button>
      <button
        type="button"
        class="lang-btn"
        (click)="i18n.toggle()"
        [attr.aria-label]="'Switch language to ' + targetLabel()"
      >
        {{ targetLabel() }}
      </button>
    </div>
  `,
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
      }
      .topbar-actions {
        display: inline-flex;
        align-items: center;
        gap: 0.75rem;
      }
      .icon-btn,
      .lang-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: transparent;
        color: inherit;
        border: 1px solid currentColor;
        border-radius: var(--radius);
        cursor: pointer;
        font-family: var(--font-sans);
        opacity: 0.85;
        transition: opacity 0.15s ease;
      }
      .icon-btn:hover,
      .lang-btn:hover {
        opacity: 1;
      }
      .icon-btn {
        width: 2rem;
        height: 2rem;
        font-size: 0.95rem;
        line-height: 1;
        padding: 0;
      }
      .lang-btn {
        height: 2rem;
        padding: 0 0.7rem;
        font-size: 0.75rem;
        letter-spacing: 0.04em;
      }
    `,
  ],
})
export class SettingsControls {
  protected theme = inject(ThemeService);
  protected i18n = inject(LanguageService);

  // Label of the language the button will switch TO.
  protected targetLabel = computed(
    () => LANGUAGES.find((l) => l.code !== this.i18n.lang())?.label ?? ''
  );
}
