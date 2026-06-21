import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/product.service';
import { Product, ProductVisibilityFilter } from '../../core/models';
import { LanguageService } from '../../core/language.service';
import { TranslatePipe } from '../../core/translate.pipe';
import { Pager } from '../../shared/pager';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, CurrencyPipe, TranslatePipe, Pager],
  templateUrl: './product-list.html',
  styleUrl: './products.scss',
})
export class ProductList {
  private productService = inject(ProductService);
  private i18n = inject(LanguageService);

  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly page = signal(1);
  readonly pageSize = PAGE_SIZE;
  readonly totalCount = signal(0);

  // --- Filters (staged by the inputs, applied when the user clicks Search) ---
  readonly visibility = signal<ProductVisibilityFilter>('All');
  readonly name = signal('');
  readonly category = signal('');

  readonly hasActiveFilters = computed(
    () => !!this.name().trim() || !!this.category().trim() || this.visibility() !== 'All'
  );

  // Monotonic token so a slow response can't overwrite a newer one.
  private requestSeq = 0;

  constructor() {
    this.load();
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  // Filter controls only stage their value; the query runs when the user clicks Search (or Enter).
  onNameInput(value: string): void {
    this.name.set(value);
  }

  onCategoryInput(value: string): void {
    this.category.set(value);
  }

  onVisibilityChange(value: ProductVisibilityFilter): void {
    this.visibility.set(value);
  }

  // Run the search with the staged filters, from page 1.
  search(): void {
    this.page.set(1);
    this.load();
  }

  clearFilters(): void {
    this.name.set('');
    this.category.set('');
    this.visibility.set('All');
    this.page.set(1);
    this.load();
  }

  remove(id: string, name: string): void {
    if (!confirm(this.i18n.t('plist.confirmDelete', { name }))) {
      return;
    }
    this.productService.delete(id).subscribe({
      // Reload so totals/pages stay correct; step back if the last item on a page was removed.
      next: () => {
        if (this.products().length === 1 && this.page() > 1) {
          this.page.update((p) => p - 1);
        }
        this.load();
      },
      error: () => this.error.set('plist.errDelete'),
    });
  }

  toggleVisibility(product: Product): void {
    const next = !product.isHidden;
    this.productService.setVisibility(product.id, next).subscribe({
      next: () => {
        // Under "All" the row just updates in place. Under a visible/hidden-only filter the toggled
        // product no longer matches, so reload to drop it (stepping back if it was the last on a page).
        if (this.visibility() === 'All') {
          this.products.update((list) =>
            list.map((p) => (p.id === product.id ? { ...p, isHidden: next } : p))
          );
        } else {
          if (this.products().length === 1 && this.page() > 1) {
            this.page.update((p) => p - 1);
          }
          this.load();
        }
      },
      error: () => this.error.set('plist.errVisibility'),
    });
  }

  load(): void {
    const seq = ++this.requestSeq;
    this.loading.set(true);
    this.error.set(null);
    this.productService
      .getPageForAdmin(this.page(), this.pageSize, this.visibility(), {
        name: this.name().trim() || undefined,
        category: this.category().trim() || undefined,
      })
      .subscribe({
        next: (res) => {
          if (seq !== this.requestSeq) return; // superseded by a newer request
          this.products.set(res.items);
          this.totalCount.set(res.totalCount);
          this.loading.set(false);
        },
        error: () => {
          if (seq !== this.requestSeq) return;
          this.error.set('plist.errLoad');
          this.loading.set(false);
        },
      });
  }
}
