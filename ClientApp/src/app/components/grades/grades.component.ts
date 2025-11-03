import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GradesService, GradesApiResponse } from '../../services/grades.service';
import { StudentsService } from '../../services/students.service';
import { CoursesService } from '../../services/courses.service';
import { ClassesService } from '../../services/classes.service';
import { AuthService } from '../../services/auth.service';
import { Grade, Student, Course, Class } from '../../models/models';

@Component({
  selector: 'app-grades',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss']
})
export class GradesComponent implements OnInit {
  grades: Grade[] = [];
  students: Student[] = [];
  courses: Course[] = [];
  classes: Class[] = [];
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 15;
  totalPages: number = 0;
  totalCount: number = 0;

  // Filters
  selectedClassId: string = '';
  selectedCourseId: string = '';

  // Loading & Messages
  isLoading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  // Modal state
  showAddModal: boolean = false;
  showEditModal: boolean = false;
  showDeleteModal: boolean = false;
  isEditMode: boolean = false;

  // Current grade for add/edit/delete
  currentGrade: Grade = this.getEmptyGrade();
  gradeToDelete: Grade | null = null;

  // Validation errors
  validationErrors: any = {};

  // Preview classification
  previewClassification: string = '';

  constructor(
    private gradesService: GradesService,
    private studentsService: StudentsService,
    private coursesService: CoursesService,
    private classesService: ClassesService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadGrades();
    this.loadClasses();
    this.loadStudents();
    this.loadCourses();
  }

  getEmptyGrade(): Grade {
    return {
      studentId: '',
      courseId: '',
      score: 0,
      classification: ''
    };
  }

  loadGrades(): void {
    this.isLoading = true;
    this.gradesService.getGrades(
      this.currentPage,
      this.pageSize,
      this.selectedClassId,
      this.selectedCourseId
    ).subscribe({
      next: (response: GradesApiResponse) => {
        this.grades = response.data;
        this.currentPage = response.pageNumber;
        this.pageSize = response.pageSize;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading grades:', error);
        this.errorMessage = 'Lỗi khi tải danh sách điểm';
        this.isLoading = false;
        this.hideMessageAfterDelay();
      }
    });
  }

  loadClasses(): void {
    this.classesService.getClasses(1, 100).subscribe({
      next: (response) => {
        this.classes = response.data;
      },
      error: (error) => {
        console.error('Error loading classes:', error);
      }
    });
  }

  loadStudents(): void {
    this.studentsService.getStudents({ pageNumber: 1, pageSize: 500 }).subscribe({
      next: (response) => {
        this.students = response.data;
      },
      error: (error) => {
        console.error('Error loading students:', error);
      }
    });
  }

  loadCourses(): void {
    this.coursesService.getCourses(1, 100).subscribe({
      next: (response) => {
        this.courses = response.data;
      },
      error: (error) => {
        console.error('Error loading courses:', error);
      }
    });
  }

  onClassFilterChange(): void {
    this.currentPage = 1;
    this.loadGrades();
  }

  onCourseFilterChange(): void {
    this.currentPage = 1;
    this.loadGrades();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadGrades();
    }
  }

  // Modal methods
  openAddModal(): void {
    this.isEditMode = false;
    this.currentGrade = this.getEmptyGrade();
    this.previewClassification = '';
    this.validationErrors = {};
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.currentGrade = this.getEmptyGrade();
    this.previewClassification = '';
    this.validationErrors = {};
  }

  openEditModal(grade: Grade): void {
    this.isEditMode = true;
    this.currentGrade = { ...grade };
    this.updatePreviewClassification();
    this.validationErrors = {};
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.currentGrade = this.getEmptyGrade();
    this.previewClassification = '';
    this.validationErrors = {};
  }

  openDeleteModal(grade: Grade): void {
    this.gradeToDelete = grade;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.gradeToDelete = null;
  }

  // Score change handler - update classification preview
  onScoreChange(): void {
    this.updatePreviewClassification();
  }

  updatePreviewClassification(): void {
    if (this.currentGrade.score >= 0 && this.currentGrade.score <= 10) {
      this.previewClassification = this.gradesService.calculateClassification(this.currentGrade.score);
    } else {
      this.previewClassification = '';
    }
  }

  // CRUD Operations
  saveGrade(): void {
    if (!this.validateForm()) {
      return;
    }

    // Auto-calculate classification before saving
    this.currentGrade.classification = this.gradesService.calculateClassification(this.currentGrade.score);

    if (this.isEditMode) {
      this.updateGrade();
    } else {
      this.createGrade();
    }
  }

  createGrade(): void {
    this.gradesService.createGrade(this.currentGrade).subscribe({
      next: () => {
        this.successMessage = 'Thêm điểm thành công!';
        this.closeAddModal();
        this.loadGrades();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error creating grade:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi thêm điểm';
        this.hideMessageAfterDelay();
      }
    });
  }

  updateGrade(): void {
    this.gradesService.updateGrade(
      this.currentGrade.studentId,
      this.currentGrade.courseId,
      this.currentGrade
    ).subscribe({
      next: () => {
        this.successMessage = 'Cập nhật điểm thành công!';
        this.closeEditModal();
        this.loadGrades();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error updating grade:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi cập nhật điểm';
        this.hideMessageAfterDelay();
      }
    });
  }

  deleteGrade(): void {
    if (!this.gradeToDelete) return;

    this.gradesService.deleteGrade(
      this.gradeToDelete.studentId,
      this.gradeToDelete.courseId
    ).subscribe({
      next: () => {
        this.successMessage = 'Xóa điểm thành công!';
        this.closeDeleteModal();
        this.loadGrades();
        this.hideMessageAfterDelay();
      },
      error: (error) => {
        console.error('Error deleting grade:', error);
        this.errorMessage = error.error?.message || 'Lỗi khi xóa điểm';
        this.closeDeleteModal();
        this.hideMessageAfterDelay();
      }
    });
  }

  // Validation
  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    // Validate studentId (required)
    if (!this.currentGrade.studentId || this.currentGrade.studentId === '') {
      this.validationErrors.studentId = 'Sinh viên là bắt buộc';
      isValid = false;
    }

    // Validate courseId (required)
    if (!this.currentGrade.courseId || this.currentGrade.courseId === '') {
      this.validationErrors.courseId = 'Môn học là bắt buộc';
      isValid = false;
    }

    // Validate score (required, 0-10, max 2 decimals)
    if (this.currentGrade.score === null || this.currentGrade.score === undefined) {
      this.validationErrors.score = 'Điểm là bắt buộc';
      isValid = false;
    } else if (this.currentGrade.score < 0 || this.currentGrade.score > 10) {
      this.validationErrors.score = 'Điểm phải từ 0 đến 10';
      isValid = false;
    } else {
      // Check decimal places
      const scoreStr = this.currentGrade.score.toString();
      const decimalIndex = scoreStr.indexOf('.');
      if (decimalIndex !== -1 && scoreStr.length - decimalIndex - 1 > 2) {
        this.validationErrors.score = 'Điểm chỉ được có tối đa 2 chữ số thập phân';
        isValid = false;
      }
    }

    return isValid;
  }

  // Export methods
  exportToExcel(): void {
    this.gradesService.exportToExcel(
      this.selectedClassId,
      this.selectedCourseId
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `BangDiem_${new Date().getTime()}.xlsx`;
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
    this.gradesService.exportToPdf(
      this.selectedClassId,
      this.selectedCourseId
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `BangDiem_${new Date().getTime()}.pdf`;
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

  // Get classification badge class
  getClassificationClass(classification: string): string {
    return `badge-${this.gradesService.getClassificationColor(classification)}`;
  }

  // Role-based access
  isStudent(): boolean {
    const role = this.authService.userRole;
    return role === 'Student';
  }

  canEdit(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin' || role === 'Teacher';
  }

  canDelete(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin' || role === 'Teacher';
  }

  canExport(): boolean {
    const role = this.authService.userRole;
    return role === 'Admin' || role === 'Teacher';
  }
}
