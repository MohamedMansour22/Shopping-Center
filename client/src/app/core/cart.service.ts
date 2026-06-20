import { Injectable, computed, effect, signal } from '@angular/core';
import { CartItem, Product } from './models';

const CART_KEY = 'sc_cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  private itemsSignal = signal<CartItem[]>(this.load());

  readonly items = this.itemsSignal.asReadonly();
  readonly count = computed(() => this.itemsSignal().reduce((n, i) => n + i.quantity, 0));
  readonly totalPrice = computed(() =>
    this.itemsSignal().reduce((sum, i) => sum + i.price * i.quantity, 0)
  );

  constructor() {
    // Persist the cart to localStorage whenever it changes.
    effect(() => {
      localStorage.setItem(CART_KEY, JSON.stringify(this.itemsSignal()));
    });
  }

  add(product: Product, quantity: number): void {
    const qty = this.normalize(quantity);
    this.itemsSignal.update((items) => {
      const existing = items.find((i) => i.productId === product.id);
      if (existing) {
        return items.map((i) =>
          i.productId === product.id ? { ...i, quantity: i.quantity + qty } : i
        );
      }
      return [
        ...items,
        {
          productId: product.id,
          name: product.name,
          price: product.price,
          imageDataUri: product.imageDataUri,
          quantity: qty,
        },
      ];
    });
  }

  setQuantity(productId: string, quantity: number): void {
    const qty = this.normalize(quantity);
    this.itemsSignal.update((items) =>
      items.map((i) => (i.productId === productId ? { ...i, quantity: qty } : i))
    );
  }

  remove(productId: string): void {
    this.itemsSignal.update((items) => items.filter((i) => i.productId !== productId));
  }

  clear(): void {
    this.itemsSignal.set([]);
  }

  private normalize(quantity: number): number {
    const n = Math.floor(Number(quantity));
    return Number.isFinite(n) && n >= 1 ? n : 1;
  }

  private load(): CartItem[] {
    try {
      const raw = localStorage.getItem(CART_KEY);
      const parsed = raw ? JSON.parse(raw) : [];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }
}
