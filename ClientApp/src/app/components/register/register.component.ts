import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface RegisterForm {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: 'Student' | 'Teacher';
  fullName?: string;
}

interface ValidationErrors {
  username?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
  general?: string;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: RegisterForm = {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'Student',
    fullName: ''
  };

  validationErrors: ValidationErrors = {};
  isLoading: boolean = false;
  showPassword: boolean = false;
  showConfirmPassword: boolean = false;
  successMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Redirect if already logged in
    if (this.authService.isLoggedIn) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    // Reset errors
    this.validationErrors = {};
    this.successMessage = '';

    // Validate form
    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;

    this.authService.register(this.registerForm).subscribe({
      next: (response) => {
        console.log('Registration response:', response);
        if (response.success) {
          // Navigate to verify email page with email and code
          this.router.navigate(['/verify-email'], {
            state: {
              email: this.registerForm.email,
              verificationCode: response.verificationCode,
              message: response.message
            }
          });
        } else {
          this.validationErrors.general = response.message || 'Đăng ký thất bại';
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Registration error:', error);
        
        // Handle validation errors from backend
        if (error.error && error.error.errors) {
          const backendErrors = error.error.errors;
          if (backendErrors.Username) {
            this.validationErrors.username = backendErrors.Username[0];
          }
          if (backendErrors.Email) {
            this.validationErrors.email = backendErrors.Email[0];
          }
          if (backendErrors.Password) {
            this.validationErrors.password = backendErrors.Password[0];
          }
        } else if (error.error && error.error.message) {
          this.validationErrors.general = error.error.message;
        } else {
          this.validationErrors.general = 'Đã xảy ra lỗi. Vui lòng thử lại.';
        }
        
        this.isLoading = false;
      }
    });
  }

  validateForm(): boolean {
    let isValid = true;

    // Username validation
    if (!this.registerForm.username) {
      this.validationErrors.username = 'Tên đăng nhập là bắt buộc';
      isValid = false;
    } else if (this.registerForm.username.length < 3 || this.registerForm.username.length > 50) {
      this.validationErrors.username = 'Tên đăng nhập phải từ 3-50 ký tự';
      isValid = false;
    } else if (!/^[a-zA-Z0-9_]+$/.test(this.registerForm.username)) {
      this.validationErrors.username = 'Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới';
      isValid = false;
    }

    // Email validation
    if (!this.registerForm.email) {
      this.validationErrors.email = 'Email là bắt buộc';
      isValid = false;
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.registerForm.email)) {
      this.validationErrors.email = 'Email không hợp lệ';
      isValid = false;
    }

    // Password validation
    if (!this.registerForm.password) {
      this.validationErrors.password = 'Mật khẩu là bắt buộc';
      isValid = false;
    } else if (this.registerForm.password.length < 6) {
      this.validationErrors.password = 'Mật khẩu phải có ít nhất 6 ký tự';
      isValid = false;
    }

    // Confirm password validation
    if (!this.registerForm.confirmPassword) {
      this.validationErrors.confirmPassword = 'Vui lòng xác nhận mật khẩu';
      isValid = false;
    } else if (this.registerForm.password !== this.registerForm.confirmPassword) {
      this.validationErrors.confirmPassword = 'Mật khẩu xác nhận không khớp';
      isValid = false;
    }

    return isValid;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  clearError(field: keyof ValidationErrors): void {
    delete this.validationErrors[field];
  }
}
