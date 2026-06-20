import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/cart.service';
import { LanguageService } from '../../core/language.service';
import { TranslatePipe } from '../../core/translate.pipe';

@Component({
  selector: 'app-cart',
  imports: [RouterLink, CurrencyPipe, TranslatePipe],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  private cart = inject(CartService);
  private i18n = inject(LanguageService);

  readonly items = this.cart.items;
  readonly total = this.cart.totalPrice;

  onQuantityChange(productId: string, value: string): void {
    this.cart.setQuantity(productId, Number(value));
  }

  increment(productId: string, current: number): void {
    this.cart.setQuantity(productId, current + 1);
  }

  decrement(productId: string, current: number): void {
    this.cart.setQuantity(productId, current - 1);
  }

  remove(productId: string): void {
    this.cart.remove(productId);
  }

  clear(): void {
    if (this.items().length && confirm(this.i18n.t('cart.confirmEmpty'))) {
      this.cart.clear();
    }
  }
}
