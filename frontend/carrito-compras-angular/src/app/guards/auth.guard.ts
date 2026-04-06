import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const raw = localStorage.getItem('userLogged');
  if (!raw || raw === '{}') {
    router.navigate(['/login']);
    return false;
  }
  return true;
};
