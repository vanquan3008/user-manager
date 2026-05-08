import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { hasAdminRole } from '../constants/roles';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.hasValidSession()) {
    router.navigate(['/login']);
    return false;
  }

  const hasRole = hasAdminRole(auth.user()?.roles);

  if (!hasRole) {
    auth.logout({ queryParams: { forbidden: '1' } });
    return false;
  }

  return true;
};
