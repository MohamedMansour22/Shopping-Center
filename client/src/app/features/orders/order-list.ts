import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/order.service';
import { Order } from '../../core/models';
import { TranslatePipe } from '../../core/translate.pipe';
import { Pager } from '../../shared/pager';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-order-list',
  imports: [RouterLink, CurrencyPipe, DatePipe, TranslatePipe, Pager],
  templateUrl: './order-list.html',
  styleUrl: './orders.scss',
})
export class OrderList {
  private orderService = inject(OrderService);

  readonly orders = signal<Order[]>([]);
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

  // Translation key for a status name, e.g. "Placed" -> "ostatus.placed".
  statusKey(name: string): string {
    return `ostatus.${name.toLowerCase()}`;
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.orderService.getPage(this.page(), this.pageSize).subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('olist.errLoad');
        this.loading.set(false);
      },
    });
  }
}
