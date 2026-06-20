import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/cart.service';
import { Product } from '../../core/models';
import { ProductService } from '../../core/product.service';
import { TranslatePipe } from '../../core/translate.pipe';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, CurrencyPipe, TranslatePipe],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private cart = inject(CartService);

  // Bound from the route param via withComponentInputBinding().
  readonly id = input<string>('');

  readonly product = signal<Product | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly quantity = signal(1);
  readonly added = signal(false);
  // Index of the gallery image currently shown as the main image.
  readonly selectedImage = signal(0);

  ngOnInit(): void {
    const id = this.id();
    if (!id) {
      this.error.set('Product not found.');
      this.loading.set(false);
      return;
    }
    this.productService.getById(id).subscribe({
      next: (p) => {
        this.product.set(p);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('detail.notFound');
        this.loading.set(false);
      },
    });
  }

  selectImage(index: number): void {
    this.selectedImage.set(index);
  }

  setQuantity(value: string): void {
    const n = Math.floor(Number(value));
    this.quantity.set(Number.isFinite(n) && n >= 1 ? n : 1);
  }

  addToCart(): void {
    const p = this.product();
    if (!p || p.stockQuantity <= 0) {
      return;
    }
    this.cart.add(p, this.quantity());
    this.added.set(true);
    setTimeout(() => this.added.set(false), 2000);
  }
}
