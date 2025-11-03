import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  credentials: LoginRequest = {
    username: '',
    password: ''
  };

  errorMessage: string = '';
  isLoading: boolean = false;
  showPassword: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Redirect if already logged in
    if (this.authService.isLoggedIn) {
      this.redirectToDashboard();
    }
  }

  redirectToDashboard(): void {
    const role = this.authService.userRole;
    switch (role) {
      case 'Admin':
        this.router.navigate(['/dashboard-admin']);
        break;
      case 'Teacher':
        this.router.navigate(['/dashboard-teacher']);
        break;
      case 'Student':
        this.router.navigate(['/dashboard-student']);
        break;
      default:
        this.router.navigate(['/students']);
    }
  }

  onSubmit(): void {
    if (!this.credentials.username || !this.credentials.password) {
      this.errorMessage = 'Vui lòng nhập tên đăng nhập và mật khẩu';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        console.log('Login response:', response);
        if (response.success) {
          // Navigate to role-based dashboard
          this.redirectToDashboard();
        } else {
          this.errorMessage = response.message || 'Đăng nhập thất bại';
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Login error:', error);
        this.errorMessage = 'Tên đăng nhập hoặc mật khẩu không đúng';
        this.isLoading = false;
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // Demo login
  loginDemo(role: 'admin' | 'teacher' | 'student'): void {
    const demoAccounts = {
      admin: { username: 'admin', password: 'admin123' },
      teacher: { username: 'nvanh', password: 'teacher123' },
      student: { username: 'ttbinh', password: 'student123' }  // Changed from 'nvan' to 'ttbinh' (SV002)
    };

    this.credentials = demoAccounts[role];
    this.onSubmit();
  }
}
