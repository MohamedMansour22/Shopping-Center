import { ChangeDetectorRef, Pipe, PipeTransform, effect, inject } from '@angular/core';
import { LanguageService } from './language.service';

// Impure so it re-evaluates on every change-detection pass, picking up the
// current language signal. Usage: {{ 'home.title' | translate }}
// With params: {{ 'home.noMatch' | translate: { term: search() } }}
//
// This is a zoneless app, so we don't rely on global CD from events. The effect
// below marks the host view dirty whenever the language changes, guaranteeing
// every component that uses the pipe re-renders (the same approach ngx-translate
// uses for its impure pipe). Cleaned up automatically when the view is destroyed.
@Pipe({ name: 'translate', pure: false })
export class TranslatePipe implements PipeTransform {
  private i18n = inject(LanguageService);
  private cdr = inject(ChangeDetectorRef);

  constructor() {
    effect(() => {
      this.i18n.lang();
      this.cdr.markForCheck();
    });
  }

  transform(key: string, params?: Record<string, string | number>): string {
    return this.i18n.t(key, params);
  }
}
