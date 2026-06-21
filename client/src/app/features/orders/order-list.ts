import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderService, orderStatusKey } from '../../core/order.service';
import { Order, OrderStatus } from '../../core/models';
import { TranslatePipe } from '../../core/translate.pipe';
import { Pager } from '../../shared/pager';
import { LtrDirective } from '../../shared/ltr.directive';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-order-list',
  imports: [RouterLink, CurrencyPipe, DatePipe, TranslatePipe, Pager, LtrDirective],
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

  // --- Filters ---
  readonly statuses = signal<OrderStatus[]>([]); // for the status dropdown
  readonly dateFrom = signal('');
  readonly dateTo = signal('');
  readonly customerName = signal('');
  readonly statusId = signal<number | null>(null);

  readonly hasActiveFilters = computed(
    () =>
      !!this.dateFrom() ||
      !!this.dateTo() ||
      !!this.customerName().trim() ||
      this.statusId() !== null
  );

  // Monotonic token so a slow response can't overwrite a newer one.
  private requestSeq = 0;

  constructor() {
    this.orderService.getStatuses().subscribe({
      next: (s) => this.statuses.set(s),
      error: () => {}, // dropdown just falls back to "All statuses" if the lookup fails
    });
    this.load();
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  // Filter controls only stage their value; the query runs when the user clicks Search (or Enter).
  onCustomerNameInput(value: string): void {
    this.customerName.set(value);
  }

  onDateFromChange(value: string): void {
    this.dateFrom.set(value);
  }

  onDateToChange(value: string): void {
    this.dateTo.set(value);
  }

  onStatusChange(value: string): void {
    this.statusId.set(value ? Number(value) : null);
  }

  // Run the search with the staged filters, from page 1.
  search(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.dateFrom.set('');
    this.dateTo.set('');
    this.customerName.set('');
    this.statusId.set(null);
    this.applyFilters();
  }

  // Restart from page 1 and reload with the current filters.
  private applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  readonly statusKey = orderStatusKey;

  load(): void {
    const seq = ++this.requestSeq;
    this.loading.set(true);
    this.error.set(null);
    this.orderService
      .getPage(this.page(), this.pageSize, {
        dateFrom: this.dateFrom() || undefined,
        dateTo: this.dateTo() || undefined,
        customerName: this.customerName().trim() || undefined,
        statusId: this.statusId(),
      })
      .subscribe({
        next: (res) => {
          if (seq !== this.requestSeq) return; // superseded by a newer request
          this.orders.set(res.items);
          this.totalCount.set(res.totalCount);
          this.loading.set(false);
        },
        error: () => {
          if (seq !== this.requestSeq) return;
          this.error.set('olist.errLoad');
          this.loading.set(false);
        },
      });
  }
}
