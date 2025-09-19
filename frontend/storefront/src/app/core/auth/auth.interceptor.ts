import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

const AUTH_URLS = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/auth/seed-admin'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // gắn bearer trước
  const token = auth.accessToken;
  if (token) req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // nếu 401 và không phải chính các endpoint auth → thử refresh
      const isAuthUrl = AUTH_URLS.some(u => req.url.includes(u));
      if (error.status === 401 && !isAuthUrl) {
        const refresh$ = auth.refresh();
        if (!refresh$) { auth.logout(); return throwError(() => error); }

        return refresh$.pipe(
          switchMap((newAccess) => {
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${newAccess}` } });
            return next(retried);
          }),
          catchError(err2 => { auth.logout(); return throwError(() => err2); })
        );
      }
      return throwError(() => error);
    })
  );
};
