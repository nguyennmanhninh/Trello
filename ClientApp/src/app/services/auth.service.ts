import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { 
  LoginRequest, 
  LoginResponse, 
  User, 
  RegisterRequest, 
  RegisterResponse, 
  VerifyEmailRequest, 
  VerifyEmailResponse,
  ResendCodeRequest,
  ResendCodeResponse
} from '../models/models';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5298/api';
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<User | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  public get userRole(): string | null {
    return this.currentUserSubject.value?.role || null;
  }

  public get token(): string | null {
    return localStorage.getItem('token');
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/auth/login`, credentials, {
      withCredentials: true  // ✅ Ensure cookies are sent/received
    })
      .pipe(
        tap(response => {
          console.log('🔐 Login response received:', response);
          if (response.success && response.token && response.user) {
            // Store user details and jwt token
            localStorage.setItem('token', response.token);
            localStorage.setItem('currentUser', JSON.stringify(response.user));
            this.currentUserSubject.next(response.user);
            console.log('✅ User data stored in localStorage');
          }
        })
      );
  }

  logout(): void {
    // Remove user from local storage
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  changePassword(oldPassword: string, newPassword: string): Observable<any> {
    const body = {
      userId: this.currentUserValue?.userId,
      oldPassword,
      newPassword
    };
    return this.http.post(`${this.API_URL}/auth/change-password`, body);
  }

  /**
   * Update current user data in localStorage and BehaviorSubject
   * Call this after profile update to sync local data with backend
   */
  updateCurrentUser(updatedData: Partial<User>): void {
    const currentUser = this.currentUserValue;
    if (currentUser) {
      const updatedUser = { ...currentUser, ...updatedData };
      localStorage.setItem('currentUser', JSON.stringify(updatedUser));
      this.currentUserSubject.next(updatedUser);
      console.log('✅ Updated currentUser in localStorage:', updatedUser);
    }
  }

  hasRole(roles: string[]): boolean {
    const userRole = this.userRole;
    return userRole ? roles.includes(userRole) : false;
  }

  isAdmin(): boolean {
    return this.userRole === 'Admin';
  }

  isTeacher(): boolean {
    return this.userRole === 'Teacher';
  }

  isStudent(): boolean {
    return this.userRole === 'Student';
  }

  // ============================================
  // Registration & Email Verification
  // ============================================

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.API_URL}/auth/register`, data);
  }

  verifyEmail(email: string, code: string): Observable<VerifyEmailResponse> {
    const data: VerifyEmailRequest = { email, code };
    return this.http.post<VerifyEmailResponse>(`${this.API_URL}/auth/verify-email`, data);
  }

  resendVerificationCode(email: string): Observable<ResendCodeResponse> {
    const data: ResendCodeRequest = { email };
    return this.http.post<ResendCodeResponse>(`${this.API_URL}/auth/resend-code`, data);
  }
}
