import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminCatalogFacade {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  // Tạo sản phẩm mới
  createProduct(body: { name: string; brand: string; price: number; description?: string }) {
    return this.http.post<{ id: number }>(`${this.base}/api/catalog/admin/products`, body);
  }

  // Upload ảnh cho sản phẩm
  uploadImage(id: number, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.base}/api/catalog/admin/products/${id}/image`, form);
  }

  // Xóa sản phẩm
  deleteProduct(id: number) {
    return this.http.delete(`${this.base}/api/catalog/admin/products/${id}`);
  }
}
