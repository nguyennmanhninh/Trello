import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AttendanceService } from '../../services/attendance.service';
import { CoursesService } from '../../services/courses.service';
import { AuthService } from '../../services/auth.service';
import { AttendanceSession, Course, CreateSessionRequest } from '../../models/models';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './attendance-list.component.html',
  styleUrls: ['./attendance-list.component.scss']
})
export class AttendanceListComponent implements OnInit {
  sessions: AttendanceSession[] = [];
  courses: Course[] = [];
  loading: boolean = false;
  error: string = '';
  success: string = '';
  
  // Filters
  selectedCourseId: string = '';
  searchTerm: string = '';
  statusFilter: string = 'all'; // all, scheduled, completed, cancelled
  
  // Create session modal
  showCreateModal: boolean = false;
  newSession: CreateSessionRequest = {
    courseId: '',
    teacherId: '',
    sessionDate: '',
    sessionTime: '08:00',
    sessionTitle: '',
    sessionType: 'Lý thuyết',
    location: '',
    duration: 90
  };
  validationErrors: any = {};

  constructor(
    private attendanceService: AttendanceService,
    private coursesService: CoursesService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('📋 AttendanceListComponent - Initializing...');
    
    // Check user role - this page is only for Admin/Teacher
    const currentUser = this.authService.currentUserValue;
    console.log('👤 Current user:', currentUser);
    console.log('🔐 User role:', currentUser?.role);
    
    if (currentUser?.role === 'Student') {
      console.warn('⚠️ Student detected in AttendanceList - redirecting to my-attendance');
      this.router.navigate(['/my-attendance']);
      return;
    }
    
    this.loadCourses();
    this.loadSessions();
  }

  loadCourses(): void {
    this.coursesService.getCourses().subscribe({
      next: (response: any) => {
        this.courses = response.data || response || [];
        console.log('✅ Loaded courses:', this.courses.length);
      },
      error: (error) => {
        console.error('❌ Error loading courses:', error);
      }
    });
  }

  loadSessions(): void {
    this.loading = true;
    this.error = '';
    
    const courseId = this.selectedCourseId || undefined;
    
    this.attendanceService.getSessions(courseId).subscribe({
      next: (sessions) => {
        this.sessions = sessions;
        console.log('✅ Loaded sessions:', sessions.length);
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error loading sessions:', error);
        this.error = 'Lỗi khi tải danh sách buổi điểm danh';
        this.loading = false;
      }
    });
  }

  get filteredSessions(): AttendanceSession[] {
    return this.sessions.filter(session => {
      // Status filter
      if (this.statusFilter !== 'all' && session.status !== this.statusFilter) {
        return false;
      }
      
      // Search filter
      if (this.searchTerm) {
        const search = this.searchTerm.toLowerCase();
        return (
          session.sessionTitle?.toLowerCase().includes(search) ||
          session.courseName?.toLowerCase().includes(search) ||
          session.location?.toLowerCase().includes(search)
        );
      }
      
      return true;
    });
  }

  onCourseFilterChange(): void {
    this.loadSessions();
  }

  openCreateModal(): void {
    this.showCreateModal = true;
    this.validationErrors = {};
    this.newSession = {
      courseId: this.selectedCourseId || '',
      teacherId: '', // Will be set by backend based on session
      sessionDate: new Date().toISOString().split('T')[0],
      sessionTime: '08:00',
      sessionTitle: '',
      sessionType: 'Lý thuyết',
      location: '',
      duration: 90
    };
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.validationErrors = {};
  }

  validateSession(): boolean {
    this.validationErrors = {};
    let isValid = true;

    if (!this.newSession.courseId) {
      this.validationErrors.courseId = 'Vui lòng chọn môn học';
      isValid = false;
    }

    if (!this.newSession.sessionDate) {
      this.validationErrors.sessionDate = 'Vui lòng chọn ngày học';
      isValid = false;
    }

    if (!this.newSession.sessionTime) {
      this.validationErrors.sessionTime = 'Vui lòng chọn giờ học';
      isValid = false;
    }

    if (!this.newSession.sessionTitle || this.newSession.sessionTitle.trim() === '') {
      this.validationErrors.sessionTitle = 'Vui lòng nhập tiêu đề buổi học';
      isValid = false;
    }

    return isValid;
  }

  createSession(): void {
    if (!this.validateSession()) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.attendanceService.createSession(this.newSession).subscribe({
      next: (response) => {
        console.log('✅ Session created:', response);
        this.success = 'Tạo buổi điểm danh thành công!';
        this.closeCreateModal();
        this.loadSessions();
        setTimeout(() => this.success = '', 3000);
      },
      error: (error) => {
        console.error('❌ Error creating session:', error);
        this.error = error.error?.message || 'Lỗi khi tạo buổi điểm danh';
        this.loading = false;
      }
    });
  }

  viewSession(sessionId: number): void {
    this.router.navigate(['/attendance/take', sessionId]);
  }

  deleteSession(session: AttendanceSession): void {
    if (!confirm(`Bạn có chắc muốn xóa buổi điểm danh "${session.sessionTitle}"?`)) {
      return;
    }

    this.loading = true;
    
    this.attendanceService.deleteSession(session.sessionId!).subscribe({
      next: (response) => {
        console.log('✅ Session deleted');
        this.success = 'Xóa buổi điểm danh thành công!';
        this.loadSessions();
        setTimeout(() => this.success = '', 3000);
      },
      error: (error) => {
        console.error('❌ Error deleting session:', error);
        this.error = error.error?.message || 'Lỗi khi xóa buổi điểm danh';
        this.loading = false;
      }
    });
  }

  getStatusClass(status: string): string {
    return this.attendanceService.getStatusClass(status);
  }

  getStatusLabel(status: string): string {
    return this.attendanceService.getStatusLabel(status);
  }

  formatDate(date: string | Date): string {
    return this.attendanceService.formatDate(date);
  }

  formatTime(time: string): string {
    return this.attendanceService.formatTime(time);
  }

  getAttendanceRateColor(rate: number): string {
    return this.attendanceService.getAttendanceRateColor(rate);
  }
}
