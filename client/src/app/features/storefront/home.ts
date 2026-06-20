import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/cart.service';
import { Product } from '../../core/models';
import { ProductService } from '../../core/product.service';
import { TranslatePipe } from '../../core/translate.pipe';

const PAGE_SIZE = 12;
const SEARCH_DEBOUNCE_MS = 300;

@Component({
  selector: 'app-home',
  imports: [RouterLink, CurrencyPipe, TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private productService = inject(ProductService);
  private cart = inject(CartService);

  // Accumulated products across all pages loaded so far.
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true); // initial load / search reload (replaces the grid)
  readonly loadingMore = signal(false); // appending the next page
  readonly error = signal<string | null>(null);
  readonly hasMore = signal(false);
  readonly search = signal('');

  // Last page number successfully loaded.
  private page = 0;
  // Monotonic request token: a response is applied only if its token still matches the
  // latest request, so a slow page can't overwrite results for a newer search term.
  private requestSeq = 0;
  private searchDebounce?: ReturnType<typeof setTimeout>;

  // Per-product quantity selection (productId → quantity); defaults to 1.
  readonly quantities = signal<Record<string, number>>({});
  // Id of the product most recently added, for transient "✓ added" feedback.
  readonly addedId = signal<string | null>(null);

  constructor() {
    this.loadFirstPage();
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
    // Debounce so we issue one server request after the user pauses typing.
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => this.loadFirstPage(), SEARCH_DEBOUNCE_MS);
  }

  // Load (or reload) from page 1 — used on first render and whenever the search term changes.
  private loadFirstPage(): void {
    const seq = ++this.requestSeq;
    this.page = 1;
    this.loading.set(true);
    this.error.set(null);
    this.productService.getPage(1, PAGE_SIZE, this.search()).subscribe({
      next: (res) => {
        if (seq !== this.requestSeq) return; // a newer request superseded this one
        this.products.set(res.items);
        this.hasMore.set(res.hasMore);
        this.loading.set(false);
      },
      error: () => {
        if (seq !== this.requestSeq) return;
        this.error.set('home.loadError');
        this.loading.set(false);
      },
    });
  }

  loadMore(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    const seq = this.requestSeq; // tie this fetch to the current search context
    const next = this.page + 1;
    this.loadingMore.set(true);
    this.productService.getPage(next, PAGE_SIZE, this.search()).subscribe({
      next: (res) => {
        if (seq !== this.requestSeq) return; // search changed mid-load → discard this page
        this.page = next;
        this.products.update((curr) => [...curr, ...res.items]);
        this.hasMore.set(res.hasMore);
        this.loadingMore.set(false);
      },
      error: () => {
        if (seq !== this.requestSeq) return;
        this.loadingMore.set(false);
      },
    });
  }

  // Currently selected quantity for a product (defaults to 1).
  qty(id: string): number {
    return this.quantities()[id] ?? 1;
  }

  // Parse/clamp the entered quantity to [1, stockQuantity] and store it.
  setQty(product: Product, value: string): void {
    const n = Math.floor(Number(value));
    const safe = Number.isFinite(n) && n >= 1 ? n : 1;
    const clamped = Math.min(safe, product.stockQuantity);
    this.quantities.update((q) => ({ ...q, [product.id]: clamped }));
  }

  addToCart(product: Product): void {
    if (product.stockQuantity <= 0) {
      return;
    }
    this.cart.add(product, this.qty(product.id));
    this.addedId.set(product.id);
    setTimeout(() => {
      if (this.addedId() === product.id) {
        this.addedId.set(null);
      }
    }, 2000);
    // Reset this product's selector back to 1.
    this.quantities.update((q) => ({ ...q, [product.id]: 1 }));
  }
}
