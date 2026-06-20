import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/product.service';
import { Product } from '../../core/models';
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

  constructor() {
    this.load();
  }

  goToPage(page: number): void {
    this.page.set(page);
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
      next: () =>
        this.products.update((list) =>
          list.map((p) => (p.id === product.id ? { ...p, isHidden: next } : p))
        ),
      error: () => this.error.set('plist.errVisibility'),
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.productService.getPageForAdmin(this.page(), this.pageSize).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('plist.errLoad');
        this.loading.set(false);
      },
    });
  }
}
