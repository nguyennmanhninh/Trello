import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { TeachersService } from '../../services/teachers.service';
import { DepartmentsService } from '../../services/departments.service';
import { AuthService } from '../../services/auth.service';
import { ModalService } from '../../services/modal.service';
import { Teacher, Department } from '../../models/models';

@Component({
  selector: 'app-teachers',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teachers.component.html',
  styleUrls: ['./teachers.component.scss']
})
export class TeachersComponent implements OnInit, OnDestroy {
  teachers: Teacher[] = [];
  departments: Department[] = [];
  currentTeacher: Teacher = this.getEmptyTeacher();
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  totalCount: number = 0;
  
  // Search & Filter
  searchString: string = '';
  
  // UI State
  isModalOpen: boolean = false;
  isEditMode: boolean = false;
  showDeleteConfirm: boolean = false;
  teacherToDelete: string = '';
  
  // Loading & Alerts
  isLoading: boolean = false;
  showAlert: boolean = false;
  alertMessage: string = '';
  alertType: 'success' | 'error' = 'success';
  
  // Validation
  validationErrors: any = {};
  
  // User role
  userRole: string = '';

  // Subscription for modal service
  private modalSubscription?: Subscription;

  constructor(
    private teachersService: TeachersService,
    private departmentsService: DepartmentsService,
    private authService: AuthService,
    private modalService: ModalService
  ) {}

  ngOnInit(): void {
    const user = this.authService.currentUserValue;
    this.userRole = user?.role || '';
    
    this.loadDepartments();
    this.loadTeachers();

    // Subscribe to modal service to open add modal when triggered from dashboard
    this.modalSubscription = this.modalService.openTeacherModal$.subscribe(() => {
      console.log('📢 Received teacher modal trigger from dashboard');
      this.openAddModal();
    });
  }

  ngOnDestroy(): void {
    // Unsubscribe to prevent memory leaks
    if (this.modalSubscription) {
      this.modalSubscription.unsubscribe();
    }
  }

  loadTeachers(): void {
    this.isLoading = true;
    this.teachersService.getTeachers(this.currentPage, this.pageSize, this.searchString)
      .subscribe({
        next: (response) => {
          console.log('✅ Teachers loaded:', response);
          this.teachers = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.currentPage = response.pageNumber;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Error loading teachers:', error);
          console.error('❌ Error details:', {
            message: error.message,
            status: error.status,
            statusText: error.statusText,
            url: error.url,
            error: error.error
          });
          this.showError(error.error?.message || 'Lỗi khi tải danh sách giáo viên');
          this.isLoading = false;
        }
      });
  }

  loadDepartments(): void {
    this.departmentsService.getDepartments().subscribe({
      next: (departments) => {
        this.departments = departments;
      },
      error: (error) => {
        console.error('❌ Error loading departments:', error);
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadTeachers();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTeachers();
    }
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.currentTeacher = this.getEmptyTeacher();
    this.validationErrors = {};
    this.isModalOpen = true;
  }

  openEditModal(teacher: Teacher): void {
    this.isEditMode = true;
    this.currentTeacher = { 
      ...teacher,
      dateOfBirth: this.formatDateForInput(teacher.dateOfBirth),
      password: '' // Don't pre-fill password in edit mode
    };
    this.validationErrors = {};
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.currentTeacher = this.getEmptyTeacher();
    this.validationErrors = {};
  }

  saveTeacher(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;

    if (this.isEditMode) {
      this.teachersService.updateTeacher(this.currentTeacher.teacherId, this.currentTeacher)
        .subscribe({
          next: () => {
            this.showSuccess('Cập nhật giáo viên thành công');
            this.closeModal();
            this.loadTeachers();
          },
          error: (error) => {
            console.error('❌ Error updating teacher:', error);
            this.showError(error.error?.message || 'Lỗi khi cập nhật giáo viên');
            this.isLoading = false;
          }
        });
    } else {
      this.teachersService.createTeacher(this.currentTeacher)
        .subscribe({
          next: () => {
            this.showSuccess('Thêm giáo viên thành công');
            this.closeModal();
            this.loadTeachers();
          },
          error: (error) => {
            console.error('❌ Error creating teacher:', error);
            this.showError(error.error?.message || 'Lỗi khi thêm giáo viên');
            this.isLoading = false;
          }
        });
    }
  }

  confirmDelete(teacherId: string): void {
    this.teacherToDelete = teacherId;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.teacherToDelete = '';
  }

  deleteTeacher(): void {
    if (!this.teacherToDelete) return;

    this.isLoading = true;
    this.teachersService.deleteTeacher(this.teacherToDelete)
      .subscribe({
        next: () => {
          this.showSuccess('Xóa giáo viên thành công');
          this.cancelDelete();
          this.loadTeachers();
        },
        error: (error) => {
          console.error('❌ Error deleting teacher:', error);
          this.showError(error.error?.message || 'Lỗi khi xóa giáo viên');
          this.isLoading = false;
          this.cancelDelete();
        }
      });
  }

  exportToExcel(): void {
    this.teachersService.exportToExcel(this.searchString)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `DanhSachGiaoVien_${new Date().getTime()}.xlsx`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.showSuccess('Xuất Excel thành công');
        },
        error: (error) => {
          console.error('❌ Error exporting to Excel:', error);
          this.showError('Lỗi khi xuất file Excel');
        }
      });
  }

  exportToPdf(): void {
    this.teachersService.exportToPdf(this.searchString)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `DanhSachGiaoVien_${new Date().getTime()}.pdf`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.showSuccess('Xuất PDF thành công');
        },
        error: (error) => {
          console.error('❌ Error exporting to PDF:', error);
          this.showError('Lỗi khi xuất file PDF');
        }
      });
  }

  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    // Teacher ID validation
    if (!this.currentTeacher.teacherId || this.currentTeacher.teacherId.trim() === '') {
      this.validationErrors.teacherId = 'Mã giáo viên là bắt buộc';
      isValid = false;
    } else if (this.currentTeacher.teacherId.length > 10) {
      this.validationErrors.teacherId = 'Mã giáo viên không quá 10 ký tự';
      isValid = false;
    }

    // Full name validation
    if (!this.currentTeacher.fullName || this.currentTeacher.fullName.trim() === '') {
      this.validationErrors.fullName = 'Họ tên là bắt buộc';
      isValid = false;
    } else if (this.currentTeacher.fullName.length > 100) {
      this.validationErrors.fullName = 'Họ tên không quá 100 ký tự';
      isValid = false;
    }

    // Date of birth validation
    if (!this.currentTeacher.dateOfBirth) {
      this.validationErrors.dateOfBirth = 'Ngày sinh là bắt buộc';
      isValid = false;
    }

    // Phone validation
    if (!this.currentTeacher.phone || this.currentTeacher.phone.trim() === '') {
      this.validationErrors.phone = 'Số điện thoại là bắt buộc';
      isValid = false;
    } else if (!/^[0-9]{10,15}$/.test(this.currentTeacher.phone)) {
      this.validationErrors.phone = 'Số điện thoại phải từ 10-15 chữ số';
      isValid = false;
    }

    // Address validation
    if (!this.currentTeacher.address || this.currentTeacher.address.trim() === '') {
      this.validationErrors.address = 'Địa chỉ là bắt buộc';
      isValid = false;
    } else if (this.currentTeacher.address.length > 200) {
      this.validationErrors.address = 'Địa chỉ không quá 200 ký tự';
      isValid = false;
    }

    // Username validation
    if (!this.currentTeacher.username || this.currentTeacher.username.trim() === '') {
      this.validationErrors.username = 'Tên đăng nhập là bắt buộc';
      isValid = false;
    } else if (this.currentTeacher.username.length > 50) {
      this.validationErrors.username = 'Tên đăng nhập không quá 50 ký tự';
      isValid = false;
    }

    // Password validation (only required when adding new teacher)
    if (!this.isEditMode) {
      if (!this.currentTeacher.password || this.currentTeacher.password.trim() === '') {
        this.validationErrors.password = 'Mật khẩu là bắt buộc';
        isValid = false;
      } else if (this.currentTeacher.password.length < 6) {
        this.validationErrors.password = 'Mật khẩu phải có ít nhất 6 ký tự';
        isValid = false;
      }
    }

    // Department validation
    if (!this.currentTeacher.departmentId || this.currentTeacher.departmentId === '') {
      this.validationErrors.departmentId = 'Khoa là bắt buộc';
      isValid = false;
    }

    return isValid;
  }

  getEmptyTeacher(): Teacher {
    return {
      teacherId: '',
      fullName: '',
      dateOfBirth: '',
      gender: true,
      phone: '',
      address: '',
      username: '',
      password: '',
      departmentId: ''
    };
  }

  formatDateForInput(date: string | Date): string {
    if (!date) return '';
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatDateForDisplay(date: string | Date): string {
    if (!date) return '';
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  showSuccess(message: string): void {
    this.alertMessage = message;
    this.alertType = 'success';
    this.showAlert = true;
    setTimeout(() => {
      this.showAlert = false;
    }, 3000);
  }

  showError(message: string): void {
    this.alertMessage = message;
    this.alertType = 'error';
    this.showAlert = true;
    setTimeout(() => {
      this.showAlert = false;
    }, 5000);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  canEdit(): boolean {
    return this.userRole === 'Admin';
  }

  canDelete(): boolean {
    return this.userRole === 'Admin';
  }

  canExport(): boolean {
    return this.userRole === 'Admin';
  }
}
