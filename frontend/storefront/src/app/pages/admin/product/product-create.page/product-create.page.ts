import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AdminCatalogFacade } from './../../../../core/api/admin.catalog.facade';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-product-create',
  imports: [ReactiveFormsModule, NgIf],
  template: ` <div class="max-w-lg mx-auto p-6">
    <h1 class="text-2xl font-semibold mb-4">Tạo sản phẩm</h1>
    <form [formGroup]="f" (ngSubmit)="onSubmit()" class="space-y-3">
      <input
        class="border rounded w-full p-2"
        placeholder="Name"
        formControlName="name"
      />
      <input
        class="border rounded w-full p-2"
        placeholder="Brand"
        formControlName="brand"
      />
      <input
        class="border rounded w-full p-2"
        type="number"
        placeholder="Price"
        formControlName="price"
      />
      <textarea
        class="border rounded w-full p-2"
        placeholder="Description"
        formControlName="description"
      ></textarea>
      <input type="file" (change)="onFile($event)" />
      <button
        class="bg-black text-white rounded px-4 py-2"
        [disabled]="f.invalid || uploading"
      >
        Tạo
      </button>
      <div *ngIf="imageUrl" class="text-sm mt-2">Ảnh: {{ imageUrl }}</div>
      <div *ngIf="err" class="text-red-600 text-sm">{{ err }}</div>
    </form>
  </div>`,
})
export class ProductCreatePage {
  private fb = inject(FormBuilder);
  private api = inject(AdminCatalogFacade);
  private authService = inject(AuthService);
  file: File | null = null;
  uploading = false;
  imageUrl = '';
  err = '';

  f = this.fb.group({
    name: ['', Validators.required],
    brand: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    description: [''],
  });

  onFile(e: any) {
    this.file = e.target.files?.[0] ?? null;
  }

  onSubmit() {
    if (this.f.invalid) return;
    const token = this.authService.accessToken;
    if (!token) {
      this.err = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
      return;
    }
    this.api.createProduct(this.f.value as any).subscribe({
      next: ({ id }) => {
        if (this.file) {
          this.uploading = true;
          this.api.uploadImage(id, this.file).subscribe({
            next: (r) => {
              this.imageUrl = r.imageUrl;
              this.uploading = false;
            },
            error: () => {
              this.err = 'Upload ảnh thất bại';
              this.uploading = false;
            },
          });
        }
      },
      error: () => (this.err = 'Tạo sản phẩm thất bại'),
    });
  }
}
