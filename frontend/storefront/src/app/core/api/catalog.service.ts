import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface Product {
  id: number;
  name: string;
  brand: string;
  price: number;
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getProducts() {
    return this.http.get<Product[]>(`${this.base}/api/catalog/products`);
  }
}
