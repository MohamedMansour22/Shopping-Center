import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateOrderRequest, Order, OrderStatus, PagedResult } from './models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  // Public: place an order from the storefront checkout.
  create(payload: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, payload);
  }

  // Admin: list placed orders, one server-paginated page at a time (requires an admin JWT).
  getPage(page: number, pageSize: number): Observable<PagedResult<Order>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
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
