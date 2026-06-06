import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, shareReplay } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

let refreshInProgress: ReturnType<typeof refreshAndRetry> | null = null;

function refreshAndRetry(
  auth: AuthService,
  req: Parameters<HttpInterceptorFn>[0],
  next: Parameters<HttpInterceptorFn>[1]
) {
  return auth.refreshToken().pipe(
    switchMap(res => {
      refreshInProgress = null;
      if (res.success && res.data) {
        auth.setSession(res.data);
        const cloned = req.clone({
          setHeaders: { Authorization: `Bearer ${res.data.accessToken}` }
        });
        return next(cloned);
      }
      auth.logout();
      return throwError(() => new Error('Refresh failed'));
    }),
    catchError(e => {
      refreshInProgress = null;
      auth.logout();
      return throwError(() => e);
    }),
    shareReplay({ bufferSize: 1, refCount: true })
  );
}

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const authApi = `${environment.apiUrl}/api/auth`;
  const oauthApi = `${environment.apiUrl}/api/oauth`;
  const isRefreshRequest = req.url.includes(authApi) && req.url.includes('refresh');
  const isPublicAuthRequest =
    req.url.includes(authApi) &&
    (
      req.url.includes('/login') ||
      req.url.includes('/sso/login') ||
      req.url.includes('/register') ||
      req.url.includes('/passwordless/request') ||
      req.url.includes('/password/reset') ||
      req.url.includes('/magic-link/verify') ||
      req.url.includes('/verify-email')
    );
  const isOAuthTokenRequest = req.url.includes(`${oauthApi}/token`);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401) return throwError(() => err);
      if (isRefreshRequest) {
        auth.logout();
        return throwError(() => err);
      }
      if (isPublicAuthRequest || isOAuthTokenRequest) {
        return throwError(() => err);
      }
      if (!auth.user()) {
        auth.logout();
        return throwError(() => err);
      }

      if (refreshInProgress) {
        return refreshInProgress.pipe(
          switchMap(() => next(req.clone({
            setHeaders: { Authorization: `Bearer ${auth.getAccessToken()}` }
          })))
        );
      }
      refreshInProgress = refreshAndRetry(auth, req, next);
      return refreshInProgress;
    })
  );
};
