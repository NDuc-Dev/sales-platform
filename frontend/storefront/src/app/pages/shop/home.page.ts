import { Component, inject, signal } from '@angular/core';
import { CatalogService, Product } from '../../core/api/catalog.service';
import { AsyncPipe, CurrencyPipe, NgFor } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [NgFor, AsyncPipe, CurrencyPipe],
  template: `
  <div class="p-6">
    <h1 class="text-2xl font-semibold mb-4">Sản phẩm mới</h1>
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div *ngFor="let p of products()" class="rounded-xl border p-4">
        <div class="font-medium">{{p.name}}</div>
        <div class="text-sm text-gray-500">{{p.brand}}</div>
        <div class="mt-2 font-semibold">{{ p.price | currency:'VND':'symbol':'1.0-0' }}</div>
      </div>
    </div>
  </div>
  `
})
export class HomePage {
  private api = inject(CatalogService);
  products = signal<Product[]>([]);

  ngOnInit() {
    this.api.getProducts().subscribe(list => this.products.set(list));
  }
}
