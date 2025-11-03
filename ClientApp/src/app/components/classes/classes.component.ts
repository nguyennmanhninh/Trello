import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { ClassesService } from '../../services/classes.service';
import { DepartmentsService } from '../../services/departments.service';
import { TeachersService } from '../../services/teachers.service';
import { AuthService } from '../../services/auth.service';
import { ModalService } from '../../services/modal.service';
import { Class, Department, Teacher } from '../../models/models';

@Component({
  selector: 'app-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './classes.component.html',
  styleUrls: ['./classes.component.scss']
})
export class ClassesComponent implements OnInit, OnDestroy {
  classes: Class[] = [];
  departments: Department[] = [];
  teachers: Teacher[] = [];
  filteredTeachers: Teacher[] = [];
  currentClass: Class = this.getEmptyClass();
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  totalCount: number = 0;
  
  // Search & Filter
  searchString: string = '';
  selectedDepartmentId: string = '';
  
  // UI State
  isModalOpen: boolean = false;
  isEditMode: boolean = false;
  showDeleteConfirm: boolean = false;
  classToDelete: string = '';
  
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
    private classesService: ClassesService,
    private departmentsService: DepartmentsService,
    private teachersService: TeachersService,
    private authService: AuthService,
    private modalService: ModalService
  ) {}

  ngOnInit(): void {
    const user = this.authService.currentUserValue;
    this.userRole = user?.role || '';
    
    this.loadDepartments();
    this.loadTeachers();
    this.loadClasses();

    // Subscribe to modal service to open add modal when triggered from dashboard
    this.modalSubscription = this.modalService.openClassModal$.subscribe(() => {
      console.log('📢 Received class modal trigger from dashboard');
      this.openAddModal();
    });
  }

  ngOnDestroy(): void {
    // Unsubscribe to prevent memory leaks
    if (this.modalSubscription) {
      this.modalSubscription.unsubscribe();
    }
  }

  loadClasses(): void {
    this.isLoading = true;
    this.classesService.getClasses(this.currentPage, this.pageSize, this.searchString, this.selectedDepartmentId)
      .subscribe({
        next: (response) => {
          console.log('✅ Classes loaded:', response);
          this.classes = response.data;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.currentPage = response.pageNumber;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Error loading classes:', error);
          this.showError('Lỗi khi tải danh sách lớp học');
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

  loadTeachers(): void {
    this.teachersService.getTeachers(1, 100).subscribe({
      next: (response) => {
        this.teachers = response.items;
        this.filteredTeachers = response.items;
      },
      error: (error) => {
        console.error('❌ Error loading teachers:', error);
      }
    });
  }

  onDepartmentChange(): void {
    this.currentPage = 1;
    this.loadClasses();
    
    // Filter teachers by selected department
    if (this.currentClass.departmentId) {
      this.filteredTeachers = this.teachers.filter(t => t.departmentId === this.currentClass.departmentId);
    } else {
      this.filteredTeachers = this.teachers;
    }
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadClasses();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadClasses();
    }
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.currentClass = this.getEmptyClass();
    this.filteredTeachers = this.teachers;
    this.validationErrors = {};
    this.isModalOpen = true;
  }

  openEditModal(classData: Class): void {
    this.isEditMode = true;
    this.currentClass = { ...classData };
    
    // Filter teachers by class department
    if (this.currentClass.departmentId) {
      this.filteredTeachers = this.teachers.filter(t => t.departmentId === this.currentClass.departmentId);
    } else {
      this.filteredTeachers = this.teachers;
    }
    
    this.validationErrors = {};
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.currentClass = this.getEmptyClass();
    this.validationErrors = {};
  }

  saveClass(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;

    if (this.isEditMode) {
      this.classesService.updateClass(this.currentClass.classId, this.currentClass)
        .subscribe({
          next: () => {
            this.showSuccess('Cập nhật lớp học thành công');
            this.closeModal();
            this.loadClasses();
          },
          error: (error) => {
            console.error('❌ Error updating class:', error);
            this.showError(error.error?.message || 'Lỗi khi cập nhật lớp học');
            this.isLoading = false;
          }
        });
    } else {
      this.classesService.createClass(this.currentClass)
        .subscribe({
          next: () => {
            this.showSuccess('Thêm lớp học thành công');
            this.closeModal();
            this.loadClasses();
          },
          error: (error) => {
            console.error('❌ Error creating class:', error);
            this.showError(error.error?.message || 'Lỗi khi thêm lớp học');
            this.isLoading = false;
          }
        });
    }
  }

  confirmDelete(classId: string): void {
    this.classToDelete = classId;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.classToDelete = '';
  }

  deleteClass(): void {
    if (!this.classToDelete) return;

    this.isLoading = true;
    this.classesService.deleteClass(this.classToDelete)
      .subscribe({
        next: () => {
          this.showSuccess('Xóa lớp học thành công');
          this.cancelDelete();
          this.loadClasses();
        },
        error: (error) => {
          console.error('❌ Error deleting class:', error);
          this.showError(error.error?.message || 'Lỗi khi xóa lớp học');
          this.isLoading = false;
          this.cancelDelete();
        }
      });
  }

  exportToExcel(): void {
    this.classesService.exportToExcel(this.searchString, this.selectedDepartmentId)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `DanhSachLopHoc_${new Date().getTime()}.xlsx`;
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
    this.classesService.exportToPdf(this.searchString, this.selectedDepartmentId)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `DanhSachLopHoc_${new Date().getTime()}.pdf`;
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

    // Class ID validation
    if (!this.currentClass.classId || this.currentClass.classId.trim() === '') {
      this.validationErrors.classId = 'Mã lớp là bắt buộc';
      isValid = false;
    } else if (this.currentClass.classId.length > 10) {
      this.validationErrors.classId = 'Mã lớp không quá 10 ký tự';
      isValid = false;
    }

    // Class name validation
    if (!this.currentClass.className || this.currentClass.className.trim() === '') {
      this.validationErrors.className = 'Tên lớp là bắt buộc';
      isValid = false;
    } else if (this.currentClass.className.length > 100) {
      this.validationErrors.className = 'Tên lớp không quá 100 ký tự';
      isValid = false;
    }

    // Department validation
    if (!this.currentClass.departmentId || this.currentClass.departmentId === '') {
      this.validationErrors.departmentId = 'Khoa là bắt buộc';
      isValid = false;
    }

    // Teacher validation
    if (!this.currentClass.teacherId || this.currentClass.teacherId === '') {
      this.validationErrors.teacherId = 'Giáo viên chủ nhiệm là bắt buộc';
      isValid = false;
    }

    return isValid;
  }

  getEmptyClass(): Class {
    return {
      classId: '',
      className: '',
      departmentId: '',
      teacherId: ''
    };
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
    return this.userRole === 'Admin' || this.userRole === 'Teacher';
  }

  canDelete(): boolean {
    return this.userRole === 'Admin';
  }

  canExport(): boolean {
    return this.userRole === 'Admin' || this.userRole === 'Teacher';
  }
}
