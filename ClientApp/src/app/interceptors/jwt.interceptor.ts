import { HttpInterceptorFn } from '@angular/common/http';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  // Get token from localStorage
  const token = localStorage.getItem('token');
  
  // Clone request and add authorization header if token exists
  // IMPORTANT: Also add withCredentials: true to send session cookies
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      },
      withCredentials: true  // ✅ Send session cookies
    });
  } else {
    // Even without JWT token, still send session cookies for session-based auth
    req = req.clone({
      withCredentials: true  // ✅ Send session cookies
    });
  }

  return next(req);
};
