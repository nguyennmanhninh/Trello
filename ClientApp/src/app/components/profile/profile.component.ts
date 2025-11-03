import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ProfileService, ProfileResponse, AdminProfile, ChangePasswordRequest, UpdateAdminProfileRequest } from '../../services/profile.service';
import { DepartmentsService } from '../../services/departments.service';
import { Student, Teacher, Department } from '../../models/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  userRole: string = '';
  profileData: Student | Teacher | AdminProfile | null = null;
  departments: Department[] = [];
  
  isEditMode: boolean = false;
  isChangePasswordMode: boolean = false;
  loading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';
  validationErrors: any = {};

  // Editable copies
  editedStudent: Student | null = null;
  editedTeacher: Teacher | null = null;
  editedAdmin: UpdateAdminProfileRequest | null = null;
  
  // Password change
  passwordData: ChangePasswordRequest = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  constructor(
    private authService: AuthService,
    private profileService: ProfileService,
    private departmentsService: DepartmentsService
  ) {}

  ngOnInit(): void {
    console.log('👤 ProfileComponent - Initializing...');
    this.userRole = this.authService.userRole || '';
    this.loadDepartments();
    this.loadProfile();
  }

  loadDepartments(): void {
    this.departmentsService.getDepartments().subscribe({
      next: (response: any) => {
        this.departments = response.data || response;
      },
      error: (error: any) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  loadProfile(): void {
    this.loading = true;
    console.log('👤 Loading profile...');
    this.profileService.getProfile().subscribe({
      next: (response: any) => {
        console.log('✅ Profile loaded (raw):', response);
        console.log('📊 Role from response:', response.role);
        console.log('📊 Data from response (raw):', response.data);
        
        this.userRole = response.role;
        
        // Map PascalCase to camelCase
        const rawData = response.data;
        if (this.userRole === 'Student') {
          this.profileData = {
            studentId: rawData.StudentId || rawData.studentId,
            fullName: rawData.FullName || rawData.fullName,
            dateOfBirth: rawData.DateOfBirth || rawData.dateOfBirth,
            gender: rawData.Gender !== undefined ? rawData.Gender : rawData.gender,
            email: rawData.Email || rawData.email,
            phone: rawData.Phone || rawData.phone,
            address: rawData.Address || rawData.address,
            classId: rawData.ClassId || rawData.classId,
            className: rawData.ClassName || rawData.className,
            departmentId: rawData.DepartmentId || rawData.departmentId,
            departmentName: rawData.DepartmentName || rawData.departmentName,
            username: rawData.Username || rawData.username
          } as Student;
        } else if (this.userRole === 'Teacher') {
          this.profileData = {
            teacherId: rawData.TeacherId || rawData.teacherId,
            fullName: rawData.FullName || rawData.fullName,
            dateOfBirth: rawData.DateOfBirth || rawData.dateOfBirth,
            gender: rawData.Gender !== undefined ? rawData.Gender : rawData.gender,
            phone: rawData.Phone || rawData.phone,
            address: rawData.Address || rawData.address,
            username: rawData.Username || rawData.username,
            departmentId: rawData.DepartmentId || rawData.departmentId,
            departmentName: rawData.DepartmentName || rawData.departmentName
          } as Teacher;
        } else {
          // Admin profile is already simple
          this.profileData = rawData;
        }
        
        console.log('📊 Mapped profileData:', this.profileData);
        console.log('📊 isAdmin():', this.isAdmin());
        console.log('📊 isTeacher():', this.isTeacher());
        console.log('📊 isStudent():', this.isStudent());
        this.loading = false;
      },
      error: (error: any) => {
        console.error('❌ Error loading profile:', error);
        console.error('❌ Error details:', error.error);
        this.errorMessage = error.error?.message || 'Lỗi khi tải thông tin cá nhân';
        this.loading = false;
      }
    });
  }

  enableEditMode(): void {
    this.isEditMode = true;
    this.validationErrors = {};
    
    if (this.userRole === 'Student' && this.profileData) {
      this.editedStudent = { ...(this.profileData as Student) };
    } else if (this.userRole === 'Teacher' && this.profileData) {
      this.editedTeacher = { ...(this.profileData as Teacher) };
    } else if (this.userRole === 'Admin' && this.profileData) {
      const adminData = this.profileData as AdminProfile;
      this.editedAdmin = { username: adminData.username };
    }
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editedStudent = null;
    this.editedTeacher = null;
    this.editedAdmin = null;
    this.validationErrors = {};
  }

  saveProfile(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;

    if (this.userRole === 'Student' && this.editedStudent) {
      this.profileService.updateStudentProfile(this.editedStudent).subscribe({
        next: (response: any) => {
          console.log('✅ Update student response:', response);
          this.successMessage = response.message || 'Cập nhật thông tin thành công!';
          this.isEditMode = false;
          
          // ✅ Update localStorage with new data
          if (this.editedStudent) {
            this.authService.updateCurrentUser({
              fullName: this.editedStudent.fullName,
              // Username doesn't change for students, but update if needed
            });
          }
          
          this.editedStudent = null;
          this.loading = false;
          // Reload profile from server to get latest data
          this.loadProfile();
          this.hideMessageAfterDelay();
        },
        error: (error: any) => {
          console.error('❌ Error updating profile:', error);
          this.errorMessage = error.error?.message || 'Lỗi khi cập nhật thông tin';
          this.loading = false;
          this.hideMessageAfterDelay();
        }
      });
    } else if (this.userRole === 'Teacher' && this.editedTeacher) {
      this.profileService.updateTeacherProfile(this.editedTeacher).subscribe({
        next: (response: any) => {
          console.log('✅ Update teacher response:', response);
          this.successMessage = response.message || 'Cập nhật thông tin thành công!';
          this.isEditMode = false;
          
          // ✅ Update localStorage with new data
          if (this.editedTeacher) {
            this.authService.updateCurrentUser({
              fullName: this.editedTeacher.fullName,
              username: this.editedTeacher.username
            });
          }
          
          this.editedTeacher = null;
          this.loading = false;
          // Reload profile from server to get latest data
          this.loadProfile();
          this.hideMessageAfterDelay();
        },
        error: (error: any) => {
          console.error('❌ Error updating profile:', error);
          this.errorMessage = error.error?.message || 'Lỗi khi cập nhật thông tin';
          this.loading = false;
          this.hideMessageAfterDelay();
        }
      });
    } else if (this.userRole === 'Admin' && this.editedAdmin) {
      this.profileService.updateAdminProfile(this.editedAdmin).subscribe({
        next: (response: any) => {
          console.log('✅ Update admin response:', response);
          this.successMessage = response.message || 'Cập nhật thông tin thành công!';
          this.isEditMode = false;
          
          // ✅ Update localStorage with new username
          if (this.editedAdmin) {
            this.authService.updateCurrentUser({
              username: this.editedAdmin.username
            });
          }
          
          this.editedAdmin = null;
          this.loading = false;
          // Reload profile from server to get latest data
          this.loadProfile();
          this.hideMessageAfterDelay();
        },
        error: (error: any) => {
          console.error('❌ Error updating profile:', error);
          this.errorMessage = error.error?.message || 'Lỗi khi cập nhật thông tin';
          this.loading = false;
          this.hideMessageAfterDelay();
        }
      });
    }
  }

  openChangePassword(): void {
    this.isChangePasswordMode = true;
    this.passwordData = {
      oldPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.validationErrors = {};
  }

  cancelChangePassword(): void {
    this.isChangePasswordMode = false;
    this.passwordData = {
      oldPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.validationErrors = {};
  }

  changePassword(): void {
    if (!this.validatePasswordForm()) {
      return;
    }

    this.loading = true;
    this.profileService.changePassword(this.passwordData).subscribe({
      next: (response: any) => {
        this.successMessage = response.message || 'Đổi mật khẩu thành công!';
        this.isChangePasswordMode = false;
        this.passwordData = {
          oldPassword: '',
          newPassword: '',
          confirmPassword: ''
        };
        this.loading = false;
        this.hideMessageAfterDelay();
      },
      error: (error: any) => {
        console.error('Error changing password:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi đổi mật khẩu';
        this.loading = false;
        this.hideMessageAfterDelay();
      }
    });
  }

  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    if (this.userRole === 'Student' && this.editedStudent) {
      // Email validation (optional)
      if (this.editedStudent.email && this.editedStudent.email.trim() !== '') {
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailPattern.test(this.editedStudent.email)) {
          this.validationErrors.email = 'Email không hợp lệ';
          isValid = false;
        }
      }

      // Phone validation (optional, 10-11 digits)
      if (this.editedStudent.phone && this.editedStudent.phone.trim() !== '') {
        const phonePattern = /^[0-9]{10,11}$/;
        if (!phonePattern.test(this.editedStudent.phone)) {
          this.validationErrors.phone = 'Số điện thoại phải có 10-11 chữ số';
          isValid = false;
        }
      }
    } else if (this.userRole === 'Teacher' && this.editedTeacher) {
      // FullName validation
      if (!this.editedTeacher.fullName || this.editedTeacher.fullName.trim() === '') {
        this.validationErrors.fullName = 'Họ tên là bắt buộc';
        isValid = false;
      }

      // Email validation
      // Note: Teacher model doesn't have email field - removed validation
      
      // Phone validation
      if (this.editedTeacher.phone && this.editedTeacher.phone.trim() !== '') {
        const phonePattern = /^[0-9]{10,11}$/;
        if (!phonePattern.test(this.editedTeacher.phone)) {
          this.validationErrors.phone = 'Số điện thoại phải có 10-11 chữ số';
          isValid = false;
        }
      }
    } else if (this.userRole === 'Admin' && this.editedAdmin) {
      // Username validation
      if (!this.editedAdmin.username || this.editedAdmin.username.trim() === '') {
        this.validationErrors.username = 'Tên đăng nhập là bắt buộc';
        isValid = false;
      } else if (this.editedAdmin.username.length < 3) {
        this.validationErrors.username = 'Tên đăng nhập phải có ít nhất 3 ký tự';
        isValid = false;
      }
    }

    return isValid;
  }

  validatePasswordForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    if (!this.passwordData.oldPassword || this.passwordData.oldPassword.trim() === '') {
      this.validationErrors.oldPassword = 'Vui lòng nhập mật khẩu cũ';
      isValid = false;
    }

    if (!this.passwordData.newPassword || this.passwordData.newPassword.length < 6) {
      this.validationErrors.newPassword = 'Mật khẩu mới phải có ít nhất 6 ký tự';
      isValid = false;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.validationErrors.confirmPassword = 'Mật khẩu xác nhận không khớp';
      isValid = false;
    }

    return isValid;
  }

  hideMessageAfterDelay(): void {
    setTimeout(() => {
      this.successMessage = '';
      this.errorMessage = '';
    }, 5000);
  }

  getGenderText(gender: boolean | undefined): string {
    return gender ? 'Nam' : 'Nữ';
  }

  formatDate(dateString: string | Date | undefined): string {
    if (!dateString) return 'N/A';
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    return date.toLocaleDateString('vi-VN');
  }

  isStudent(): boolean {
    const result = this.userRole === 'Student';
    console.log('🔍 isStudent():', result, 'userRole:', this.userRole);
    return result;
  }

  isTeacher(): boolean {
    const result = this.userRole === 'Teacher';
    console.log('🔍 isTeacher():', result, 'userRole:', this.userRole);
    return result;
  }

  isAdmin(): boolean {
    const result = this.userRole === 'Admin';
    console.log('🔍 isAdmin():', result, 'userRole:', this.userRole);
    return result;
  }

  getStudentData(): Student | null {
    return this.isStudent() ? (this.profileData as Student) : null;
  }

  getTeacherData(): Teacher | null {
    return this.isTeacher() ? (this.profileData as Teacher) : null;
  }

  getAdminData(): AdminProfile | null {
    return this.isAdmin() ? (this.profileData as AdminProfile) : null;
  }
}
