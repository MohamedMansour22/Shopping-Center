import { Component, computed, input, output } from '@angular/core';
import { TranslatePipe } from '../core/translate.pipe';

// Reusable Prev / "Page X of Y" / Next control for server-paginated admin tables.
// Stateless: the parent owns the current page and reloads on (pageChange).
@Component({
  selector: 'app-pager',
  imports: [TranslatePipe],
  template: `
    @if (totalPages() > 1) {
      <nav class="pager" aria-label="Pagination">
        <button type="button" class="pager-btn" (click)="go(page() - 1)" [disabled]="page() <= 1">
          ‹ {{ 'pager.prev' | translate }}
        </button>
        <span class="pager-info">{{ 'pager.pageOf' | translate: { page: page(), pages: totalPages() } }}</span>
        <button type="button" class="pager-btn" (click)="go(page() + 1)" [disabled]="page() >= totalPages()">
          {{ 'pager.next' | translate }} ›
        </button>
      </nav>
    }
  `,
  styleUrl: './pager.scss',
})
export class Pager {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly pageChange = output<number>();

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize()))
  );

  go(target: number): void {
    if (target >= 1 && target <= this.totalPages() && target !== this.page()) {
      this.pageChange.emit(target);
    }
  }
}
