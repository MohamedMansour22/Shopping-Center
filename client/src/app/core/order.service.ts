import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateOrderRequest, Order, OrderListFilter, OrderStatus, PagedResult } from './models';

// Translation key for an order status name, e.g. "Placed" -> "ostatus.placed".
export function orderStatusKey(name: string): string {
  return `ostatus.${name.toLowerCase()}`;
}

// An <input type="date"> gives a local calendar day ('YYYY-MM-DD'). Convert its local-midnight
// boundary to a UTC instant so the server filters by the operator's day, not the UTC day.
function localDayStartUtc(day: string): string {
  return new Date(`${day}T00:00:00`).toISOString();
}

// Exclusive end: the UTC instant at the start of the day AFTER the chosen local day.
function localDayEndExclusiveUtc(day: string): string {
  const d = new Date(`${day}T00:00:00`);
  d.setDate(d.getDate() + 1);
  return d.toISOString();
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  // Public: place an order from the storefront checkout.
  create(payload: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, payload);
  }

  // Admin: list placed orders, one server-paginated page at a time (requires an admin JWT).
  // Optional filter narrows by date range, customer name, and/or status.
  getPage(page: number, pageSize: number, filter?: OrderListFilter): Observable<PagedResult<Order>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filter?.dateFrom) {
      params = params.set('dateFrom', localDayStartUtc(filter.dateFrom));
    }
    if (filter?.dateTo) {
      params = params.set('dateTo', localDayEndExclusiveUtc(filter.dateTo));
    }
    if (filter?.customerName?.trim()) {
      params = params.set('customerName', filter.customerName.trim());
    }
    if (filter?.statusId != null) {
      params = params.set('statusId', filter.statusId);
    }
    return this.http.get<PagedResult<Order>>(this.baseUrl, { params });
  }

  // Admin: a single order with items + status (order details screen).
  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  // Admin: the status lookup (Placed, Delivered) for the status dropdown.
  getStatuses(): Observable<OrderStatus[]> {
    return this.http.get<OrderStatus[]>(`${this.baseUrl}/statuses`);
  }

  // Admin: change an order's status.
  updateStatus(id: string, statusId: number): Observable<Order> {
    return this.http.put<Order>(`${this.baseUrl}/${id}/status`, { statusId });
  }
}
