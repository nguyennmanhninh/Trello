import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DepartmentsService, DepartmentDto } from '../../services/departments.service';
import { Department } from '../../models/models';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.scss'
})
export class DepartmentsComponent implements OnInit {
  departments: Department[] = [];
  isLoading: boolean = false;
  searchString: string = '';
  
  // Modal states
  showModal: boolean = false;
  showDeleteConfirm: boolean = false;
  isEditMode: boolean = false;
  
  // Current department being edited/deleted
  currentDepartment: DepartmentDto = {
    departmentId: '',
    departmentCode: '',
    departmentName: ''
  };
  
  departmentToDelete: Department | null = null;
  
  // Messages
  successMessage: string = '';
  errorMessage: string = '';
  
  // Validation errors
  validationErrors: any = {};

  constructor(
    private departmentsService: DepartmentsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('🏫 Departments Component - Initializing...');
    this.loadDepartments();
  }

  loadDepartments(): void {
    console.log('🏫 Departments Component - Loading departments...');
    this.isLoading = true;
    this.departmentsService.getDepartments().subscribe({
      next: (data) => {
        console.log('🏫 Departments Component - Data loaded:', data.length, 'departments');
        this.departments = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Departments Component - Error loading departments:', error);
        this.showError('Lỗi khi tải danh sách khoa');
        this.isLoading = false;
      }
    });
  }

  search(): void {
    if (!this.searchString.trim()) {
      this.loadDepartments();
      return;
    }
    
    this.isLoading = true;
    this.departments = this.departments.filter(d => 
      d.departmentName.toLowerCase().includes(this.searchString.toLowerCase()) ||
      d.departmentId.toLowerCase().includes(this.searchString.toLowerCase())
    );
    this.isLoading = false;
  }

  clearSearch(): void {
    this.searchString = '';
    this.loadDepartments();
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.currentDepartment = {
      departmentId: '',
      departmentCode: '',
      departmentName: ''
    };
    this.validationErrors = {};
    this.showModal = true;
  }

  openEditModal(dept: Department): void {
    this.isEditMode = true;
    this.currentDepartment = {
      departmentId: dept.departmentId,
      departmentCode: dept.departmentCode,
      departmentName: dept.departmentName
    };
    this.validationErrors = {};
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.currentDepartment = {
      departmentId: '',
      departmentCode: '',
      departmentName: ''
    };
    this.validationErrors = {};
  }

  validateForm(): boolean {
    this.validationErrors = {};
    let isValid = true;

    if (!this.currentDepartment.departmentId || this.currentDepartment.departmentId.trim() === '') {
      this.validationErrors.departmentId = 'Mã khoa là bắt buộc';
      isValid = false;
    }

    if (!this.currentDepartment.departmentCode || this.currentDepartment.departmentCode.trim() === '') {
      this.validationErrors.departmentCode = 'Mã khoa (code) là bắt buộc';
      isValid = false;
    }

    if (!this.currentDepartment.departmentName || this.currentDepartment.departmentName.trim() === '') {
      this.validationErrors.departmentName = 'Tên khoa là bắt buộc';
      isValid = false;
    }

    return isValid;
  }

  saveDepartment(): void {
    if (!this.validateForm()) {
      return;
    }

    if (this.isEditMode) {
      this.updateDepartment();
    } else {
      this.createDepartment();
    }
  }

  createDepartment(): void {
    this.departmentsService.createDepartment(this.currentDepartment).subscribe({
      next: (response) => {
        this.showSuccess('Thêm khoa thành công');
        this.closeModal();
        this.loadDepartments();
      },
      error: (error) => {
        console.error('Error creating department:', error);
        const errorMsg = error.error?.message || 'Lỗi khi thêm khoa';
        this.showError(errorMsg);
      }
    });
  }

  updateDepartment(): void {
    this.departmentsService.updateDepartment(this.currentDepartment.departmentId, this.currentDepartment).subscribe({
      next: (response) => {
        this.showSuccess('Cập nhật khoa thành công');
        this.closeModal();
        this.loadDepartments();
      },
      error: (error) => {
        console.error('Error updating department:', error);
        const errorMsg = error.error?.message || 'Lỗi khi cập nhật khoa';
        this.showError(errorMsg);
      }
    });
  }

  openDeleteConfirm(dept: Department): void {
    this.departmentToDelete = dept;
    this.showDeleteConfirm = true;
  }

  closeDeleteConfirm(): void {
    this.showDeleteConfirm = false;
    this.departmentToDelete = null;
  }

  confirmDelete(): void {
    if (!this.departmentToDelete) return;

    this.departmentsService.deleteDepartment(this.departmentToDelete.departmentId).subscribe({
      next: (response) => {
        this.showSuccess('Xóa khoa thành công');
        this.closeDeleteConfirm();
        this.loadDepartments();
      },
      error: (error) => {
        console.error('Error deleting department:', error);
        const errorMsg = error.error?.message || 'Lỗi khi xóa khoa';
        this.showError(errorMsg);
        this.closeDeleteConfirm();
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
