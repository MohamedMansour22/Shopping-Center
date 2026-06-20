import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../core/order.service';
import { Order, OrderStatus } from '../../core/models';
import { TranslatePipe } from '../../core/translate.pipe';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe, TranslatePipe],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);

  readonly order = signal<Order | null>(null);
  readonly statuses = signal<OrderStatus[]>([]);
  // The status currently selected in the dropdown (staged until Save).
  readonly selectedStatusId = signal<number | null>(null);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly saving = signal(false);
  readonly saved = signal(false);

  // Save is enabled only when the selection differs from the saved status.
  readonly dirty = computed(() => {
    const o = this.order();
    return o != null && this.selectedStatusId() !== o.statusId;
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.load(id);
    }
    this.orderService.getStatuses().subscribe({
      next: (s) => this.statuses.set(s),
      error: () => {},
    });
  }

  statusKey(name: string): string {
    return `ostatus.${name.toLowerCase()}`;
  }

  onStatusChange(value: string): void {
    this.selectedStatusId.set(Number(value));
    this.saved.set(false);
  }

  save(): void {
    const o = this.order();
    const statusId = this.selectedStatusId();
    if (!o || statusId == null || !this.dirty() || this.saving()) {
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    this.orderService.updateStatus(o.id, statusId).subscribe({
      next: (updated) => {
        this.order.set({ ...o, statusId: updated.statusId, status: updated.status });
        this.saving.set(false);
        this.saved.set(true);
      },
      error: () => {
        this.error.set('odetail.errSave');
        this.saving.set(false);
      },
    });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.orderService.getById(id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.selectedStatusId.set(o.statusId);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('odetail.errLoad');
        this.loading.set(false);
      },
    });
  }
}
