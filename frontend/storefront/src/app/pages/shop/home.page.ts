import { Component, inject, signal } from '@angular/core';
import { NgFor, CurrencyPipe } from '@angular/common';
import { CatalogFacade } from '../../core/api/catalog.facade';
import { ProductListItemDto } from '../../core/api-clients/catalog/models/product-list-item-dto';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [NgFor, CurrencyPipe],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-semibold mb-4">Sản phẩm mới</h1>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div *ngFor="let p of products()" class="rounded-xl border p-4">
          <div class="font-medium">{{ p.name }}</div>
          <div class="text-sm text-gray-500">{{ p.brand }}</div>
          <div class="mt-2 font-semibold">{{ p.price | currency:'VND':'symbol':'1.0-0' }}</div>
        </div>
      </div>
    </div>
  `
})
export class HomePage {
  private api = inject(CatalogFacade);
  products = signal<ProductListItemDto[]>([]);

  ngOnInit() {
    this.api.list().subscribe(list => this.products.set(list));
  }
}
