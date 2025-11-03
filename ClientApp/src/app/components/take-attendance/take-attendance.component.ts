import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AttendanceService } from '../../services/attendance.service';
import { AuthService } from '../../services/auth.service';
import { AttendanceSession, AttendanceRecord, MarkAttendanceRequest } from '../../models/models';

@Component({
  selector: 'app-take-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './take-attendance.component.html',
  styleUrls: ['./take-attendance.component.scss']
})
export class TakeAttendanceComponent implements OnInit {
  sessionId: number = 0;
  session: AttendanceSession | null = null;
  attendanceRecords: any[] = []; // Extended AttendanceRecord with studentName
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';
  
  // Filter & Search
  searchTerm = '';
  statusFilter = 'All'; // All, Present, Absent, Late, Excused
  
  // Statistics
  stats = {
    total: 0,
    present: 0,
    absent: 0,
    late: 0,
    excused: 0,
    notMarked: 0
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public attendanceService: AttendanceService, // Make public for template access
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.sessionId = id ? parseInt(id, 10) : 0;
    if (this.sessionId) {
      this.loadSessionDetails();
    } else {
      this.errorMessage = 'Không tìm thấy buổi điểm danh';
    }
  }

  loadSessionDetails(): void {
    this.loading = true;
    this.errorMessage = '';
    
    this.attendanceService.getSessionDetails(this.sessionId).subscribe({
      next: (response: any) => {
        this.session = response;
        this.attendanceRecords = response.attendances || [];
        this.calculateStats();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading session:', error);
        this.errorMessage = 'Không thể tải thông tin buổi điểm danh';
        this.loading = false;
      }
    });
  }

  calculateStats(): void {
    this.stats.total = this.attendanceRecords.length;
    this.stats.present = this.attendanceRecords.filter(r => r.status === 'Present').length;
    this.stats.absent = this.attendanceRecords.filter(r => r.status === 'Absent').length;
    this.stats.late = this.attendanceRecords.filter(r => r.status === 'Late').length;
    this.stats.excused = this.attendanceRecords.filter(r => r.status === 'Excused').length;
    this.stats.notMarked = this.attendanceRecords.filter(r => r.status === 'Absent' && !r.checkInTime).length;
  }

  // Quick Actions
  markAllPresent(): void {
    if (confirm('Bạn có chắc muốn điểm danh TẤT CẢ sinh viên là CÓ MẶT?')) {
      this.attendanceRecords.forEach(record => {
        record.status = 'Present';
        record.checkInTime = this.getCurrentTime();
        record.notes = '';
      });
      this.calculateStats();
    }
  }

  markAllAbsent(): void {
    if (confirm('Bạn có chắc muốn đánh dấu TẤT CẢ sinh viên là VẮNG MẶT?')) {
      this.attendanceRecords.forEach(record => {
        record.status = 'Absent';
        record.checkInTime = undefined;
        record.notes = '';
      });
      this.calculateStats();
    }
  }

  // Individual Status Change
  toggleStatus(record: AttendanceRecord, status: 'Present' | 'Absent' | 'Late' | 'Excused'): void {
    record.status = status;
    
    // Set check-in time for Present/Late
    if (status === 'Present' || status === 'Late') {
      if (!record.checkInTime) {
        record.checkInTime = this.getCurrentTime();
      }
    } else {
      record.checkInTime = undefined;
    }
    
    // Clear notes when marking Present
    if (status === 'Present') {
      record.notes = '';
    }
    
    this.calculateStats();
  }

  getCurrentTime(): string {
    const now = new Date();
    return now.toTimeString().split(' ')[0].substring(0, 5); // HH:MM
  }

  // Filter & Search
  get filteredRecords(): any[] {
    let filtered = [...this.attendanceRecords];
    
    // Status filter
    if (this.statusFilter !== 'All') {
      filtered = filtered.filter(r => r.status === this.statusFilter);
    }
    
    // Search filter
    if (this.searchTerm.trim()) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(r => 
        r.studentId.toLowerCase().includes(search) ||
        (r.studentName || '').toLowerCase().includes(search)
      );
    }
    
    return filtered;
  }

  // Save Attendance
  saveAttendance(): void {
    if (!this.session) return;
    
    // Validate: Check if any student still has default Absent without action
    const notMarkedCount = this.attendanceRecords.filter(r => 
      r.status === 'Absent' && !r.checkInTime && !r.notes
    ).length;
    
    if (notMarkedCount > 0) {
      if (!confirm(`Còn ${notMarkedCount} sinh viên chưa được đánh dấu rõ ràng. Bạn có muốn tiếp tục?`)) {
        return;
      }
    }
    
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    const request: MarkAttendanceRequest = {
      sessionId: this.sessionId,
      teacherId: '', // Backend will set this from session
      attendances: this.attendanceRecords.map(r => ({
        studentId: r.studentId,
        status: r.status,
        checkInTime: r.checkInTime,
        notes: r.notes
      }))
    };
    
    this.attendanceService.markAttendance(request).subscribe({
      next: (response: any) => {
        this.successMessage = 'Điểm danh thành công!';
        this.saving = false;
        
        // Reload session to get updated data
        setTimeout(() => {
          this.loadSessionDetails();
          this.successMessage = '';
        }, 2000);
      },
      error: (error) => {
        console.error('Error saving attendance:', error);
        this.errorMessage = error.error?.message || 'Không thể lưu điểm danh';
        this.saving = false;
      }
    });
  }

  // Complete Session (mark as completed)
  completeSession(): void {
    if (!confirm('Sau khi HOÀN THÀNH, bạn không thể sửa điểm danh này nữa. Bạn có chắc chắn?')) {
      return;
    }
    
    // First save attendance, then mark as completed
    this.saving = true;
    this.errorMessage = '';
    
    const request: MarkAttendanceRequest = {
      sessionId: this.sessionId,
      teacherId: '', // Backend will set this from session
      attendances: this.attendanceRecords.map(r => ({
        studentId: r.studentId,
        status: r.status,
        checkInTime: r.checkInTime,
        notes: r.notes
      }))
    };
    
    this.attendanceService.markAttendance(request).subscribe({
      next: () => {
        this.successMessage = 'Đã hoàn thành điểm danh!';
        this.saving = false;
        
        // Navigate back to list after 1.5 seconds
        setTimeout(() => {
          this.router.navigate(['/attendance']);
        }, 1500);
      },
      error: (error) => {
        console.error('Error completing session:', error);
        this.errorMessage = error.error?.message || 'Không thể hoàn thành điểm danh';
        this.saving = false;
      }
    });
  }

  // Utility Methods
  getStatusClass(status: string): string {
    return this.attendanceService.getStatusClass(status);
  }

  getStatusLabel(status: string): string {
    return this.attendanceService.getStatusLabel(status);
  }

  getAttendanceRate(): number {
    if (this.stats.total === 0) return 0;
    return Math.round((this.stats.present / this.stats.total) * 100);
  }

  getAttendanceRateColor(): string {
    const rate = this.getAttendanceRate();
    return this.attendanceService.getAttendanceRateColor(rate);
  }

  cancel(): void {
    if (confirm('Bạn có muốn hủy và quay lại danh sách?')) {
      this.router.navigate(['/attendance']);
    }
  }
}
