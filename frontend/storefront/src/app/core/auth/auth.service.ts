import { Injectable, signal, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { shareReplay, finalize, map, Observable } from 'rxjs';

// fn sinh từ OpenAPI (đúng đường dẫn như ở cây thư mục phía trên)
import { apiAuthLoginPost } from '../api-clients/identity/fn/auth/api-auth-login-post';
import { apiAuthRegisterPost } from '../api-clients/identity/fn/auth/api-auth-register-post';

// nếu bạn muốn dùng health: import { healthGet } from '../api-clients/identity/fn/identity-api/health-get';

export interface AuthResp {
  accessToken: string;
  refreshToken: string;
  role: string;
  email: string;
}
export interface LoginReq {
  email: string;
  password: string;
}
export interface RefreshReq {
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private _auth = signal<AuthResp | null>(null);
  auth = this._auth.asReadonly();

  private refresh$: Observable<AuthResp> | null = null;

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem('auth');
      if (saved) this._auth.set(JSON.parse(saved));
    }
  }

  login(body: LoginReq): Observable<AuthResp> {
    return this.http.post<AuthResp>(
      `${environment.apiBaseUrl}/api/auth/login`,
      body
    );
  }

  setSession(auth: AuthResp) {
    this._auth.set(auth);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('auth', JSON.stringify(auth));
    }
  }

  logout() {
    const rt = this._auth()?.refreshToken;
    if (rt)
      this.http
        .post(`${environment.apiBaseUrl}/api/auth/logout`, { refreshToken: rt })
        .subscribe();
    this._auth.set(null);
    if (isPlatformBrowser(this.platformId)) localStorage.removeItem('auth');
  }

  get accessToken() {
    return this._auth()?.accessToken ?? null;
  }

  get refreshToken() {
    return this._auth()?.refreshToken ?? null;
  }

  get role() {
    return this._auth()?.role ?? null;
  }

  /** gọi refresh, có chia sẻ observable để tránh gọi trùng */
  refresh(): Observable<AuthResp> | null {
    if (!this.refreshToken) return null;

    if (!this.refresh$) {
      this.refresh$ = this.http
        .post<AuthResp>(`${environment.apiBaseUrl}/api/auth/refresh`, {
          refreshToken: this.refreshToken,
        })
        .pipe(
          map((res) => {
            this.setSession(res); // Cập nhật lại session với accessToken và refreshToken mới
            return res; // Trả về cả accessToken và refreshToken
          }),
          shareReplay(1),
          finalize(() => (this.refresh$ = null))
        );
    }
    return this.refresh$;
  }
}
