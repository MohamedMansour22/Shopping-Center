import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult, Product, ProductVisibilityFilter } from './models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/products`;

  // Public storefront read — visible-only, one server-paginated page at a time.
  // An optional search term filters by name/category across the whole catalogue.
  getPage(page: number, pageSize: number, search?: string): Observable<PagedResult<Product>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const term = search?.trim();
    if (term) {
      params = params.set('search', term);
    }
    return this.http.get<PagedResult<Product>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  // Admin reads — require an admin JWT, one server-paginated page at a time. The visibility filter
  // selects all / visible-only / hidden-only products (defaults to all); name and category narrow the
  // list by those fields (ANDed) when supplied.
  getPageForAdmin(
    page: number,
    pageSize: number,
    visibility: ProductVisibilityFilter = 'All',
    filters: { name?: string; category?: string } = {}
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('visibility', visibility);
    const name = filters.name?.trim();
    if (name) {
      params = params.set('name', name);
    }
    const category = filters.category?.trim();
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<PagedResult<Product>>(`${this.baseUrl}/admin`, { params });
  }

  getByIdForAdmin(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/admin/${id}`);
  }

  // Sends multipart/form-data so the selected image file is uploaded with the fields.
  create(formData: FormData): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, formData);
  }

  update(id: string, formData: FormData): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  setVisibility(id: string, isHidden: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/visibility`, { isHidden });
  }
}
