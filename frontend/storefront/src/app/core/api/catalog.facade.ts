import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';

// ✅ đúng theo cây thư mục bạn gửi
import { apiCatalogProductsGet } from '../api-clients/catalog/fn/products/api-catalog-products-get';
// Mở file detail để lấy tên export đúng (IdInt hay Id)
import { apiCatalogProductsIdGet as apiCatalogProductGetById } from '../api-clients/catalog/fn/products/api-catalog-products-id-get';
import { ProductListItemDto } from '../api-clients/catalog/models';

@Injectable({ providedIn: 'root' })
export class CatalogFacade {
  private http = inject(HttpClient);
  private root = environment.apiBaseUrl; // http://localhost:7000 (Gateway)

  list() {
    return apiCatalogProductsGet(this.http, this.root)
    .pipe(map(r => r.body as ProductListItemDto[]));
  }

  getById(id: number) {
    return apiCatalogProductGetById(this.http, this.root, { id });
  }
}
