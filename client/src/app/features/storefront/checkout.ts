import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/cart.service';
import { OrderService } from '../../core/order.service';
import { TranslatePipe } from '../../core/translate.pipe';

@Component({
  selector: 'app-checkout',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, TranslatePipe],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout {
  private fb = inject(FormBuilder);
  private cart = inject(CartService);
  private orders = inject(OrderService);

  readonly items = this.cart.items;
  readonly total = this.cart.totalPrice;

  readonly submitted = signal(false);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  // Set once an order is placed; keeps the confirmation visible after the cart clears.
  readonly placedOrder = signal<{ name: string; email: string } | null>(null);

  // Empty cart matters only before placing — after success the cart is intentionally cleared.
  readonly cartEmpty = computed(() => this.items().length === 0 && !this.placedOrder());

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^[+\d][\d\s()-]{6,}$/)]],
    address: ['', [Validators.required, Validators.minLength(5)]],
  });

  // True when a control is invalid and the user has interacted (or tried to submit).
  invalid(control: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || this.submitted());
  }

  submit(): void {
    this.submitted.set(true);
    this.error.set(null);
    if (this.form.invalid || this.cartEmpty() || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const { fullName, email, phone, address } = this.form.getRawValue();
    this.submitting.set(true);
    this.orders
      .create({
        customerName: fullName,
        customerEmail: email,
        customerPhone: phone,
        shippingAddress: address,
        items: this.items().map((i) => ({ productId: i.productId, quantity: i.quantity })),
      })
      .subscribe({
        next: () => {
          this.placedOrder.set({ name: fullName, email });
          this.cart.clear();
          this.submitting.set(false);
        },
        error: (err) => {
          // Surface the server's reason (e.g. out of stock) when present; translate() passes
          // an unknown string through unchanged, so a raw message renders fine. Fall back otherwise.
          const serverMessage = err?.error?.message;
          this.error.set(typeof serverMessage === 'string' && serverMessage ? serverMessage : 'checkout.errSubmit');
          this.submitting.set(false);
        },
      });
  }
}
