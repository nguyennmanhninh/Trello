import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoursesService, CoursesApiResponse } from '../../services/courses.service';
import { DepartmentsService } from '../../services/departments.service';
import { TeachersService } from '../../services/teachers.service';
import { AuthService } from '../../services/auth.service';
import { Course, Department, Teacher } from '../../models/models';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './courses.component.html',
  styleUrls: ['./courses.component.scss']
})
export class CoursesComponent implements OnInit {
  courses: Course[] = [];
  departments: Department[] = [];
  teachers: Teacher[] = [];
  filteredTeachers: Teacher[] = [];
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 0;
  totalCount: number = 0;

  // Filters
  searchString: string = '';
  selectedDepartmentId: string = '';
  selectedTeacherId: string = '';

  // Loading & Messages
  isLoading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  // Modal state
  showAddModal: boolean = false;
  showEditModal: boolean = false;
  showDeleteModal: boolean = false;
  isEditMode: boolean = false;

  // Current course for add/edit/delete
  currentCourse: Course = this.getEmptyCourse();
  courseToDelete: Course | null = null;

  // Validation errors
  validationErrors: any = {};

  constructor(
    private coursesService: CoursesService,
    private departmentsService: DepartmentsService,
    private teachersService: TeachersService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadCourses();
    this.loadDepartments();
    this.loadTeachers();
  }

  getEmptyCourse(): Course {
    return {
      courseId: '',
      courseName: '',
      credits: 1,
      departmentId: '',
      teacherId: ''
    };
  }

  loadCourses(): void {
    this.isLoading = true;
    this.coursesService.getCourses(
      this.currentPage,
      this.pageSize,
      this.searchString,
      this.selectedDepartmentId,
      this.selectedTeacherId
    ).subscribe({
      next: (response: CoursesApiResponse) => {
        this.courses = response.data;
        this.currentPage = response.pageNumber;
        this.pageSize = response.pageSize;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading courses:', error);
        this.errorMessage = 'Lỗi khi tải danh sách môn học';
        this.isLoading = false;
        this.hideMessageAfterDelay();
      }
    });
  }

  loadDepartments(): void {
    this.departmentsService.getDepartments().subscribe({
      next: (departments) => {
        this.departments = departments;
      },
      error: (error) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  loadTeachers(): void {
    this.teachersService.getTeachers(1, 100).subscribe({
      next: (response) => {
        this.teachers = response.items;
        this.filteredTeachers = this.teachers;
      },
      error: (error) => {
        console.error('Error loading teachers:', error);
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCourses();
  }

  onDepartmentFilterChange(): void {
    this.currentPage = 1;
    this.loadCourses();
  }

  onTeacherFilterChange(): void {
    this.currentPage = 1;
    this.loadCourses();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCourses();
    }
  }

  // Modal methods
  openAddModal(): void {
    this.isEditMode = false;
    this.currentCourse = this.getEmptyCourse();
    this.validationErrors = {};
    this.showAddModal = true;
    this.filteredTeachers = this.teachers;
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.currentCourse = this.getEmptyCourse();
    this.validationErrors = {};
  }

  openEditModal(course: Course): void {
    this.isEditMode = true;
    this.currentCourse = { ...course };
    this.validationErrors = {};
    this.showEditModal = true;
    
    // Filter teachers by selected department
    if (this.currentCourse.departmentId) {
      this.filteredTeachers = this.teachers.filter(
        t => t.departmentId === this.currentCourse.departmentId
      );
    } else {
      this.filteredTeachers = this.teachers;
    }
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.currentCourse = this.getEmptyCourse();
    this.validationErrors = {};
  }

  openDeleteModal(course: Course): void {
    this.courseToDelete = course;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.courseToDelete = null;
  }

  // Department change handler for cascading dropdown
  onDepartmentChange(): void {
    if (this.currentCourse.departmentId) {
      this.filteredTeachers = this.teachers.filter(
        t => t.departmentId === this.currentCourse.departmentId
      );
      
      // Reset teacherId if current teacher is not in filtered list
      if (this.currentCourse.teacherId) {
        const teacherExists = this.filteredTeachers.some(
          t => t.teacherId === this.currentCourse.teacherId
        );
        if (!teacherExists) {
          this.currentCourse.teacherId = '';
        }
      }
    } else {
      this.filteredTeachers = this.teachers;
      this.currentCourse.teacherId = '';
    }
  }

  // CRUD Operations
  saveCourse(): void {
    if (!this.validateForm()) {
      return;
    }

    if (this.isEditMode) {
      this.updateCourse();
    } else {
      this.createCourse();
    }
  }

  createCourse(): void {
    this.coursesService.createCourse(this.currentCourse).subscribe({
      next: () => {
        this.successMessage = 'Thêm môn học thành công!';
        this.closeAddModal();
        this.loadCourses();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error creating course:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi thêm môn học';
        this.hideMessageAfterDelay();
      }
    });
  }

  updateCourse(): void {
    this.coursesService.updateCourse(this.currentCourse.courseId, this.currentCourse).subscribe({
      next: () => {
        this.successMessage = 'Cập nhật môn học thành công!';
        this.closeEditModal();
        this.loadCourses();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error updating course:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi cập nhật môn học';
        this.hideMessageAfterDelay();
      }
    });
  }

  deleteCourse(): void {
    if (!this.courseToDelete) return;

    this.coursesService.deleteCourse(this.courseToDelete.courseId).subscribe({
      next: () => {
        this.successMessage = 'Xóa môn học thành công!';
        this.closeDeleteModal();
        this.loadCourses();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error deleting course:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi xóa môn học';
        this.closeDeleteModal();
        this.hideMessageAfterDelay();
      }
    });
  }

  // Validation
  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    // Validate courseId (required, max 10 chars)
    if (!this.currentCourse.courseId || this.currentCourse.courseId.trim() === '') {
      this.validationErrors.courseId = 'Mã môn học là bắt buộc';
      isValid = false;
    } else if (this.currentCourse.courseId.length > 10) {
      this.validationErrors.courseId = 'Mã môn học không được quá 10 ký tự';
      isValid = false;
    }

    // Validate courseName (required, max 100 chars)
    if (!this.currentCourse.courseName || this.currentCourse.courseName.trim() === '') {
      this.validationErrors.courseName = 'Tên môn học là bắt buộc';
      isValid = false;
    } else if (this.currentCourse.courseName.length > 100) {
      this.validationErrors.courseName = 'Tên môn học không được quá 100 ký tự';
      isValid = false;
    }

    // Validate credits (required, 1-10)
    if (!this.currentCourse.credits) {
      this.validationErrors.credits = 'Số tín chỉ là bắt buộc';
      isValid = false;
    } else if (this.currentCourse.credits < 1 || this.currentCourse.credits > 10) {
      this.validationErrors.credits = 'Số tín chỉ phải từ 1 đến 10';
      isValid = false;
    }

    // Validate departmentId (required)
    if (!this.currentCourse.departmentId || this.currentCourse.departmentId === '') {
      this.validationErrors.departmentId = 'Khoa là bắt buộc';
      isValid = false;
    }

    // Validate teacherId (optional but must be valid if provided)
    // No validation needed for optional field

    return isValid;
  }

  // Export methods
  exportToExcel(): void {
    this.coursesService.exportToExcel(
      this.searchString,
      this.selectedDepartmentId,
      this.selectedTeacherId
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `DanhSachMonHoc_${new Date().getTime()}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.successMessage = 'Xuất Excel thành công!';
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error exporting to Excel:', error);
        this.errorMessage = 'Lỗi khi xuất Excel';
        this.hideMessageAfterDelay();
      }
    });
  }

  exportToPdf(): void {
    this.coursesService.exportToPdf(
      this.searchString,
      this.selectedDepartmentId,
      this.selectedTeacherId
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `DanhSachMonHoc_${new Date().getTime()}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.successMessage = 'Xuất PDF thành công!';
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error exporting to PDF:', error);
        this.errorMessage = 'Lỗi khi xuất PDF';
        this.hideMessageAfterDelay();
      }
    });
  }

  // Helper methods
  hideMessageAfterDelay(): void {
    setTimeout(() => {
      this.successMessage = '';
      this.errorMessage = '';
    }, 5000);
  }

  // Role-based access
  canEdit(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin' || role === 'Teacher';
  }

  canDelete(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin';
  }

  canExport(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin' || role === 'Teacher';
  }
}
