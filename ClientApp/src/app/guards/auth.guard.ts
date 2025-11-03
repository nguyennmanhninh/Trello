import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log('🔐 Auth Guard - Checking route:', state.url);
  console.log('🔐 Auth Guard - User logged in:', authService.isLoggedIn);
  console.log('🔐 Auth Guard - User role:', authService.userRole);

  if (authService.isLoggedIn) {
    // Check if route has role requirements
    const requiredRoles = route.data['roles'] as string[];
    
    console.log('🔐 Auth Guard - Required roles:', requiredRoles);
    console.log('🔐 Auth Guard - Route data:', route.data);
    
    if (requiredRoles && requiredRoles.length > 0) {
      // Check if user has required role
      const hasPermission = authService.hasRole(requiredRoles);
      console.log('🔐 Auth Guard - Has permission:', hasPermission);
      
      if (hasPermission) {
        return true;
      } else {
        // User doesn't have permission - redirect to appropriate dashboard
        console.warn('⛔ Auth Guard - Access DENIED - Redirecting to dashboard');
        
        const userRole = authService.userRole;
        let dashboardRoute = '/login';
        
        if (userRole === 'Admin') {
          dashboardRoute = '/dashboard-admin';
        } else if (userRole === 'Teacher') {
          dashboardRoute = '/dashboard-teacher';
        } else if (userRole === 'Student') {
          dashboardRoute = '/dashboard-student';
        }
        
        router.navigate([dashboardRoute]);
        return false;
      }
    }
    
    console.log('✅ Auth Guard - No role check needed - Access GRANTED');
    return true;
  }

  // Not logged in, redirect to login page
  console.warn('⛔ Auth Guard - Not logged in - Redirecting to login');
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
