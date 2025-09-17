import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { Router } from '@angular/router';
import { NgIf } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [ReactiveFormsModule, NgIf],
  template: ` <div class="max-w-sm mx-auto p-6">
    <h1 class="text-2xl font-semibold mb-4">Đăng nhập</h1>
    <form [formGroup]="f" (ngSubmit)="onSubmit()" class="space-y-3">
      <input
        class="border rounded w-full p-2"
        placeholder="Email"
        formControlName="email"
      />
      <input
        class="border rounded w-full p-2"
        placeholder="Mật khẩu"
        type="password"
        formControlName="password"
      />
      <button
        class="bg-black text-white rounded px-4 py-2"
        [disabled]="f.invalid"
      >
        Đăng nhập
      </button>
      <div class="text-red-600 text-sm" *ngIf="error">{{ error }}</div>
    </form>
  </div>`,
})
export class LoginPage {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  error = '';

  f = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  onSubmit() {
    if (this.f.invalid) return;
    this.auth.login(this.f.value as any).subscribe({
      next: (res: any) => {
        // hoặc (res: AuthResp) nếu backend đã .Produces<AuthResponse>
        this.auth.setSession(res);
        this.router.navigateByUrl('/admin');
      },
      error: () => (this.error = 'Sai email hoặc mật khẩu'),
    });
  }
}
