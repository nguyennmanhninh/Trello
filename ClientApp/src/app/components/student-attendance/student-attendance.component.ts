import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AttendanceService } from '../../services/attendance.service';
import { AuthService } from '../../services/auth.service';
import { AttendanceStatistics, AttendanceWarning } from '../../models/models';

@Component({
  selector: 'app-student-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-attendance.component.html',
  styleUrls: ['./student-attendance.component.scss']
})
export class StudentAttendanceComponent implements OnInit {
  studentId = '';
  attendanceRecords: any[] = [];
  statistics: AttendanceStatistics[] = [];
  warnings: AttendanceWarning[] = [];
  loading = false;
  errorMessage = '';
  
  // Filter
  selectedCourseId = 'All';
  courses: any[] = [];
  
  // Tab state
  activeTab: 'records' | 'statistics' | 'warnings' = 'records';

  constructor(
    private attendanceService: AttendanceService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('📋 StudentAttendanceComponent - Initializing...');
    
    // Get current user from AuthService
    const currentUser = this.authService.currentUserValue;
    console.log('👤 Current user:', currentUser);
    
    if (currentUser && currentUser.role === 'Student' && currentUser.entityId) {
      this.studentId = currentUser.entityId; // Use entityId (e.g., "SV002") for database queries
      console.log('✅ Student ID:', this.studentId);
      this.loadAttendanceData();
    } else {
      console.error('❌ Invalid user data:', { role: currentUser?.role, entityId: currentUser?.entityId });
      this.errorMessage = 'Không xác định được sinh viên';
    }
  }

  loadAttendanceData(): void {
    this.loading = true;
    this.errorMessage = '';
    
    console.log('🔄 Loading attendance data for student:', this.studentId);
    
    // Load attendance records
    this.attendanceService.getStudentRecords(this.studentId).subscribe({
      next: (response: any) => {
        console.log('📦 Raw response from getStudentRecords:', response);
        
        // API returns { success: true, data: attendances }
        if (response && response.data) {
          this.attendanceRecords = this.mapAttendanceRecords(response.data);
          console.log('✅ Mapped attendance records:', this.attendanceRecords.length);
        } else if (Array.isArray(response)) {
          // Fallback: direct array response
          this.attendanceRecords = this.mapAttendanceRecords(response);
          console.log('✅ Direct array attendance records:', this.attendanceRecords.length);
        } else {
          this.attendanceRecords = [];
          console.warn('⚠️ Unexpected response format');
        }
        
        this.extractCourses();
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error loading attendance:', error);
        this.errorMessage = 'Không thể tải dữ liệu điểm danh';
        this.loading = false;
      }
    });
    
    // Load statistics
    this.attendanceService.getStudentStats(this.studentId).subscribe({
      next: (response: any) => {
        console.log('📊 Stats response:', response);
        // API returns { success: true, data: stats }
        this.statistics = response?.data || response || [];
      },
      error: (error) => {
        console.error('❌ Error loading stats:', error);
      }
    });
    
    // Note: Warnings are not shown to students, so we don't load them
    // Only Admin/Teacher can see warnings
    console.log('ℹ️ Warnings not loaded for student view');
  }

  mapAttendanceRecords(rawRecords: any[]): any[] {
    console.log('🔄 Mapping attendance records, count:', rawRecords.length);
    if (rawRecords.length > 0) {
      console.log('📋 First raw record:', JSON.stringify(rawRecords[0], null, 2));
    }
    
    return rawRecords.map((record: any) => {
      // Extract session data - try multiple paths
      const session = record.Session || record.session || {};
      
      // Try to get course from multiple locations
      let course = session.Course || session.course || {};
      
      // If course is still empty, try getting it directly from record
      if (!course.CourseId && !course.courseId) {
        course = {
          CourseId: record.CourseId || record.courseId,
          CourseName: record.CourseName || record.courseName
        };
      }
      
      // Build the mapped object
      const mapped = {
        attendanceId: record.AttendanceId || record.attendanceId,
        sessionId: record.SessionId || record.sessionId,
        studentId: record.StudentId || record.studentId,
        status: record.Status || record.status || 'Absent',
        checkInTime: record.CheckInTime || record.checkInTime,
        notes: record.Notes || record.notes,
        markedByTeacherId: record.MarkedByTeacherId || record.markedByTeacherId,
        markedAt: record.MarkedAt || record.markedAt,
        
        // Session info - check both session object and direct record properties
        sessionTitle: session.SessionTitle || session.sessionTitle || record.SessionTitle || record.sessionTitle || 'Buổi học',
        sessionDate: session.SessionDate || session.sessionDate || record.SessionDate || record.sessionDate,
        sessionTime: session.SessionTime || session.sessionTime || record.SessionTime || record.sessionTime,
        location: session.Location || session.location || record.Location || record.location || 'Chưa xác định',
        
        // Course info - try all possible paths
        courseId: course.CourseId || course.courseId || session.CourseId || session.courseId || record.CourseId || record.courseId || 'N/A',
        courseName: course.CourseName || course.courseName || session.CourseName || session.courseName || record.CourseName || record.courseName || 'Không xác định'
      };
      
      console.log('✅ Mapped record:', {
        attendanceId: mapped.attendanceId,
        sessionId: mapped.sessionId,
        courseId: mapped.courseId,
        courseName: mapped.courseName,
        sessionDate: mapped.sessionDate,
        sessionTitle: mapped.sessionTitle,
        status: mapped.status,
        rawSessionHasCourse: !!(session.Course || session.course),
        rawCourseId: course.CourseId || course.courseId,
        rawCourseName: course.CourseName || course.courseName
      });
      
      return mapped;
    });
  }

  extractCourses(): void {
    const courseMap = new Map();
    this.attendanceRecords.forEach((record: any) => {
      if (!courseMap.has(record.courseId)) {
        courseMap.set(record.courseId, {
          courseId: record.courseId,
          courseName: record.courseName
        });
      }
    });
    this.courses = Array.from(courseMap.values());
  }

  get filteredRecords(): any[] {
    if (this.selectedCourseId === 'All') {
      return this.attendanceRecords;
    }
    return this.attendanceRecords.filter(r => r.courseId === this.selectedCourseId);
  }

  get overallStats() {
    const total = this.attendanceRecords.length;
    if (total === 0) {
      return { total: 0, present: 0, absent: 0, late: 0, excused: 0, rate: 0 };
    }
    
    const present = this.attendanceRecords.filter(r => r.status === 'Present').length;
    const absent = this.attendanceRecords.filter(r => r.status === 'Absent').length;
    const late = this.attendanceRecords.filter(r => r.status === 'Late').length;
    const excused = this.attendanceRecords.filter(r => r.status === 'Excused').length;
    const rate = Math.round((present / total) * 100);
    
    return { total, present, absent, late, excused, rate };
  }

  getStatusClass(status: string): string {
    return this.attendanceService.getStatusClass(status);
  }

  getStatusLabel(status: string): string {
    return this.attendanceService.getStatusLabel(status);
  }

  getAttendanceRateColor(rate: number): string {
    return this.attendanceService.getAttendanceRateColor(rate);
  }

  formatDate(date: any): string {
    return this.attendanceService.formatDate(date);
  }

  formatTime(time: any): string {
    return this.attendanceService.formatTime(time);
  }

  switchTab(tab: 'records' | 'statistics' | 'warnings'): void {
    this.activeTab = tab;
  }
}
