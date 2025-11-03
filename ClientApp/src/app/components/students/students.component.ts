import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { StudentsService } from '../../services/students.service';
import { DepartmentsService } from '../../services/departments.service';
import { ClassesService } from '../../services/classes.service';
import { ModalService } from '../../services/modal.service';
import { Student, Department, Class } from '../../models/models';
import { AuthService } from '../../services/auth.service';

export interface StudentDto {
  studentId: string;
  fullName: string;
  dateOfBirth: string;
  gender: boolean; // true = Male, false = Female
  phone: string;
  address: string;
  classId: string;
  username: string;
  password?: string;
}

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss'
})
export class StudentsComponent implements OnInit, OnDestroy {
  students: Student[] = [];
  departments: Department[] = [];
  classes: Class[] = [];
  filteredClasses: Class[] = [];
  
  isLoading: boolean = false;
  searchString: string = '';
  selectedDepartmentId: string = '';
  selectedClassId: string = '';
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;
  
  // Modal states
  showModal: boolean = false;
  showDeleteConfirm: boolean = false;
  isEditMode: boolean = false;
  
  // Current student being edited/deleted
  currentStudent: StudentDto = this.getEmptyStudent();
  studentToDelete: Student | null = null;
  
  // Messages
  successMessage: string = '';
  errorMessage: string = '';
  
  // Validation errors
  validationErrors: any = {};

  // Subscription for modal service
  private modalSubscription?: Subscription;
  
  // User info
  userRole: string | null = null;

  constructor(
    private studentsService: StudentsService,
    private departmentsService: DepartmentsService,
    private classesService: ClassesService,
    private authService: AuthService,
    private router: Router,
    private modalService: ModalService
  ) {
    this.userRole = this.authService.userRole;
  }

  ngOnInit(): void {
    console.log('👨‍🎓 Students Component - Initializing...');
    this.loadDepartments();
    this.loadClasses();
    this.loadStudents();

    // Subscribe to modal service to open add modal when triggered from dashboard
    console.log('🎧 Students: Setting up modal subscription...');
    this.modalSubscription = this.modalService.openStudentModal$.subscribe(() => {
      console.log('📢 Students: Received modal trigger from dashboard!!!');
      console.log('📢 Students: Current showModal value:', this.showModal);
      console.log('📢 Students: Calling openAddModal()...');
      this.openAddModal();
      console.log('📢 Students: After openAddModal(), showModal =', this.showModal);
    });
    console.log('✅ Students: Modal subscription setup complete');
  }

  ngOnDestroy(): void {
    // Unsubscribe to prevent memory leaks
    if (this.modalSubscription) {
      this.modalSubscription.unsubscribe();
    }
  }

  getEmptyStudent(): StudentDto {
    return {
      studentId: '',
      fullName: '',
      dateOfBirth: '',
      gender: true,
      phone: '',
      address: '',
      classId: '',
      username: '',
      password: ''
    };
  }

  loadDepartments(): void {
    this.departmentsService.getDepartments().subscribe({
      next: (data) => {
        this.departments = data;
        console.log('👨‍🎓 Departments loaded:', data.length);
      },
      error: (error) => {
        console.error('❌ Error loading departments:', error);
      }
    });
  }

  loadClasses(): void {
    this.classesService.getClasses(1, 100).subscribe({
      next: (response) => {
        this.classes = response.data;
        this.filteredClasses = response.data;
        console.log('👨‍🎓 Classes loaded:', response.data.length);
      },
      error: (error) => {
        console.error('❌ Error loading classes:', error);
      }
    });
  }

  loadStudents(): void {
    console.log('👨‍🎓 Loading students - Page:', this.currentPage, 'Search:', this.searchString);
    this.isLoading = true;
    
    this.studentsService.getStudents({
      searchString: this.searchString || undefined,
      classId: this.selectedClassId || undefined,
      departmentId: this.selectedDepartmentId || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        console.log('👨‍🎓 Students loaded:', response);
        this.students = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.currentPage = response.pageNumber;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading students:', error);
        this.showError('Lỗi khi tải danh sách sinh viên');
        this.isLoading = false;
      }
    });
  }

  search(): void {
    this.currentPage = 1;
    this.loadStudents();
  }

  clearSearch(): void {
    this.searchString = '';
    this.selectedDepartmentId = '';
    this.selectedClassId = '';
    this.currentPage = 1;
    this.loadStudents();
  }

  onDepartmentChange(): void {
    this.selectedClassId = '';
    if (this.selectedDepartmentId) {
      this.filteredClasses = this.classes.filter(c => c.departmentId === this.selectedDepartmentId);
    } else {
      this.filteredClasses = this.classes;
    }
    this.search();
  }

  onClassChange(): void {
    this.search();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadStudents();
    }
  }

  getGenderText(gender: boolean): string {
    return gender ? 'Nam' : 'Nữ';
  }

  openAddModal(): void {
    console.log('🎬 Students: openAddModal() called');
    this.isEditMode = false;
    this.currentStudent = this.getEmptyStudent();
    this.validationErrors = {};
    this.showModal = true;
    console.log('✅ Students: Modal should be open now, showModal =', this.showModal);
  }

  openEditModal(student: Student): void {
    this.isEditMode = true;
    this.currentStudent = {
      studentId: student.studentId,
      fullName: student.fullName,
      dateOfBirth: student.dateOfBirth.toString().split('T')[0], // Format for input[type=date]
      gender: student.gender,
      phone: student.phone,
      address: student.address,
      classId: student.classId || '',
      username: student.username,
      password: '' // Don't send password on edit
    };
    this.validationErrors = {};
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.currentStudent = this.getEmptyStudent();
    this.validationErrors = {};
  }

  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    if (!this.currentStudent.studentId || this.currentStudent.studentId.trim() === '') {
      this.validationErrors.studentId = 'Mã sinh viên là bắt buộc';
      isValid = false;
    }

    if (!this.currentStudent.fullName || this.currentStudent.fullName.trim() === '') {
      this.validationErrors.fullName = 'Họ tên là bắt buộc';
      isValid = false;
    }

    if (!this.currentStudent.dateOfBirth) {
      this.validationErrors.dateOfBirth = 'Ngày sinh là bắt buộc';
      isValid = false;
    }

    if (!this.currentStudent.classId) {
      this.validationErrors.classId = 'Lớp học là bắt buộc';
      isValid = false;
    }

    if (!this.isEditMode) {
      if (!this.currentStudent.username || this.currentStudent.username.trim() === '') {
        this.validationErrors.username = 'Tên đăng nhập là bắt buộc';
        isValid = false;
      }

      if (!this.currentStudent.password || this.currentStudent.password.trim() === '') {
        this.validationErrors.password = 'Mật khẩu là bắt buộc';
        isValid = false;
      }
    }

    return isValid;
  }

  saveStudent(): void {
    if (!this.validateForm()) {
      return;
    }

    if (this.isEditMode) {
      this.studentsService.updateStudent(this.currentStudent.studentId, this.currentStudent).subscribe({
        next: () => {
          this.showSuccess('Cập nhật sinh viên thành công');
          this.closeModal();
          this.loadStudents();
        },
        error: (error) => {
          console.error('❌ Error updating student:', error);
          this.showError(error.error?.message || 'Lỗi khi cập nhật sinh viên');
        }
      });
    } else {
      this.studentsService.createStudent(this.currentStudent).subscribe({
        next: () => {
          this.showSuccess('Thêm sinh viên thành công');
          this.closeModal();
          this.loadStudents();
        },
        error: (error) => {
          console.error('❌ Error creating student:', error);
          this.showError(error.error?.message || 'Lỗi khi thêm sinh viên');
        }
      });
    }
  }

  openDeleteConfirm(student: Student): void {
    // Load student details để lấy gradeCount chính xác
    this.studentsService.getStudent(student.studentId).subscribe({
      next: (studentData) => {
        this.studentToDelete = studentData;
        this.showDeleteConfirm = true;
      },
      error: (error) => {
        console.error('❌ Error loading student details:', error);
        this.showError('Không thể tải thông tin sinh viên');
      }
    });
  }

  closeDeleteConfirm(): void {
    this.showDeleteConfirm = false;
    this.studentToDelete = null;
  }

  deleteAllGrades(): void {
    if (!this.studentToDelete) return;
    
    if (!confirm(`Xác nhận xóa tất cả ${this.studentToDelete.gradeCount} điểm của sinh viên ${this.studentToDelete.fullName}?`)) {
      return;
    }

    this.studentsService.deleteAllGrades(this.studentToDelete.studentId).subscribe({
      next: (response: any) => {
        this.showSuccess(response.message || 'Đã xóa tất cả điểm số');
        // Reload student data to update gradeCount
        this.studentsService.getStudent(this.studentToDelete!.studentId).subscribe({
          next: (updatedStudent) => {
            this.studentToDelete = updatedStudent;
          }
        });
      },
      error: (error) => {
        console.error('❌ Error deleting grades:', error);
        this.showError(error.error?.message || 'Lỗi khi xóa điểm số');
      }
    });
  }

  confirmDelete(): void {
    if (this.studentToDelete) {
      this.studentsService.deleteStudent(this.studentToDelete.studentId).subscribe({
        next: () => {
          this.showSuccess('Xóa sinh viên thành công');
          this.closeDeleteConfirm();
          this.loadStudents();
        },
        error: (error) => {
          console.error('❌ Error deleting student:', error);
          this.showError(error.error?.message || 'Lỗi khi xóa sinh viên');
        }
      });
    }
  }

  exportToExcel(): void {
    console.log('📊 Exporting to Excel...');
    this.studentsService.exportToExcel({
      searchString: this.searchString || undefined,
      classId: this.selectedClassId || undefined,
      departmentId: this.selectedDepartmentId || undefined
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `DanhSachSinhVien_${new Date().getTime()}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.showSuccess('Xuất Excel thành công');
      },
      error: (error) => {
        console.error('❌ Error exporting to Excel:', error);
        this.showError('Lỗi khi xuất Excel');
      }
    });
  }

  exportToPdf(): void {
    console.log('📄 Exporting to PDF...');
    this.studentsService.exportToPdf({
      searchString: this.searchString || undefined,
      classId: this.selectedClassId || undefined,
      departmentId: this.selectedDepartmentId || undefined
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `DanhSachSinhVien_${new Date().getTime()}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.showSuccess('Xuất PDF thành công');
      },
      error: (error) => {
        console.error('❌ Error exporting to PDF:', error);
        this.showError('Lỗi khi xuất PDF');
      }
    });
  }

  showSuccess(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
    setTimeout(() => {
      this.successMessage = '';
    }, 3000);
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
    setTimeout(() => {
      this.errorMessage = '';
    }, 5000);
  }
}
