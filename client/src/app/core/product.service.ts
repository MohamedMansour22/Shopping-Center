import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult, Product } from './models';

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

  // Admin reads — include hidden products (require an admin JWT), one server-paginated page at a time.
  getPageForAdmin(page: number, pageSize: number): Observable<PagedResult<Product>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
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
