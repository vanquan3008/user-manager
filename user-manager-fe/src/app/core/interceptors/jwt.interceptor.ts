import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

/**
 * Attaches the access token and, for API calls, the X-Requested-With header that the API requires on
 * cookie-authenticated endpoints (refresh/logout) as a CSRF defence.
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getAccessToken();
  const isApi = !!environment.apiUrl && req.url.startsWith(environment.apiUrl);
  const headers: Record<string, string> = {};
  if (isApi) headers['X-Requested-With'] = 'XMLHttpRequest';
  if (token && isApi && !req.headers.has('Authorization')) headers['Authorization'] = `Bearer ${token}`;

  return next(req.clone({ withCredentials: isApi, setHeaders: headers }));
};
