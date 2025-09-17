import { Injectable, signal, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

// fn sinh từ OpenAPI (đúng đường dẫn như ở cây thư mục phía trên)
import { apiAuthLoginPost } from '../api-clients/identity/fn/auth/api-auth-login-post';
import { apiAuthRegisterPost } from '../api-clients/identity/fn/auth/api-auth-register-post';

// nếu bạn muốn dùng health: import { healthGet } from '../api-clients/identity/fn/identity-api/health-get';

export interface AuthResp { token: string; role: string; email: string; } // FE định nghĩa tạm
export interface LoginReq { email: string; password: string; }
export interface RegisterReq { email: string; password: string; fullName?: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private _auth = signal<AuthResp | null>(null);
  auth = this._auth.asReadonly();

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem('auth');
      if (saved) this._auth.set(JSON.parse(saved));
    }
  }

  login(body: LoginReq) {
    // fn signature: (http, rootUrl, { body })
    return apiAuthLoginPost(this.http, environment.apiBaseUrl, { body });
  }

  register(body: RegisterReq) {
    return apiAuthRegisterPost(this.http, environment.apiBaseUrl, { body });
  }

  setSession(auth: AuthResp) {
    this._auth.set(auth);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('auth', JSON.stringify(auth));
    }
  }

  logout() {
    this._auth.set(null);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('auth');
    }
  }

  get token() { return this._auth()?.token ?? null; }
  get role()  { return this._auth()?.role  ?? null; }
}
