import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  AttendanceSession,
  Attendance,
  MarkAttendanceRequest,
  AttendanceStatistics,
  AttendanceWarning,
  CreateSessionRequest
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly apiUrl = '/api/attendance';

  constructor(private http: HttpClient) {}

  // ============================================
  // SESSION MANAGEMENT
  // ============================================

  /**
   * Get all attendance sessions (filtered by role)
   */
  getSessions(courseId?: string): Observable<AttendanceSession[]> {
    const url = courseId 
      ? `${this.apiUrl}/sessions?courseId=${courseId}`
      : `${this.apiUrl}/sessions`;
    
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response.success && response.data) {
          return this.mapSessionsFromBackend(response.data);
        }
        return [];
      })
    );
  }

  /**
   * Get session details by ID
   */
  getSessionDetails(sessionId: number): Observable<AttendanceSession | null> {
    return this.http.get<any>(`${this.apiUrl}/sessions/${sessionId}`).pipe(
      map(response => {
        if (response.success && response.data) {
          return this.mapSessionFromBackend(response.data);
        }
        return null;
      })
    );
  }

  /**
   * Create new attendance session
   */
  createSession(request: CreateSessionRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/sessions`, request);
  }

  /**
   * Delete attendance session
   */
  deleteSession(sessionId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/sessions/${sessionId}`);
  }

  // ============================================
  // ATTENDANCE MARKING
  // ============================================

  /**
   * Mark attendance for multiple students
   */
  markAttendance(request: MarkAttendanceRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/mark`, request);
  }

  // ============================================
  // STUDENT ATTENDANCE
  // ============================================

  /**
   * Get student attendance statistics
   */
  getStudentStats(studentId: string, courseId?: string): Observable<AttendanceStatistics[]> {
    const url = courseId
      ? `${this.apiUrl}/student/${studentId}/stats?courseId=${courseId}`
      : `${this.apiUrl}/student/${studentId}/stats`;
    
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response.success && response.data) {
          return this.mapStatsFromBackend(response.data);
        }
        return [];
      })
    );
  }

  /**
   * Get student attendance records
   */
  getStudentRecords(studentId: string): Observable<Attendance[]> {
    return this.http.get<any>(`${this.apiUrl}/student/${studentId}/records`).pipe(
      map(response => {
        if (response.success && response.data) {
          return this.mapAttendancesFromBackend(response.data);
        }
        return [];
      })
    );
  }

  // ============================================
  // WARNINGS & REPORTS
  // ============================================

  /**
   * Get attendance warnings (students with low attendance)
   */
  getWarnings(courseId: string, threshold: number = 20): Observable<AttendanceWarning[]> {
    return this.http.get<any>(`${this.apiUrl}/warnings/${courseId}?threshold=${threshold}`).pipe(
      map(response => {
        if (response.success && response.data) {
          return this.mapWarningsFromBackend(response.data);
        }
        return [];
      })
    );
  }

  // ============================================
  // MAPPING FUNCTIONS (PascalCase → camelCase)
  // ============================================

  private mapSessionFromBackend(data: any): AttendanceSession {
    return {
      sessionId: data.SessionId || data.sessionId,
      courseId: data.CourseId || data.courseId,
      courseName: data.Course?.CourseName || data.course?.courseName || data.courseName,
      teacherId: data.TeacherId || data.teacherId,
      teacherName: data.Teacher?.FullName || data.teacher?.fullName || data.teacherName,
      sessionDate: data.SessionDate || data.sessionDate,
      sessionTime: data.SessionTime || data.sessionTime,
      sessionTitle: data.SessionTitle || data.sessionTitle,
      sessionType: data.SessionType || data.sessionType,
      location: data.Location || data.location,
      duration: data.Duration || data.duration,
      notes: data.Notes || data.notes,
      status: data.Status || data.status,
      createdAt: data.CreatedAt || data.createdAt,
      updatedAt: data.UpdatedAt || data.updatedAt,
      attendances: data.Attendances 
        ? this.mapAttendancesFromBackend(data.Attendances)
        : (data.attendances ? this.mapAttendancesFromBackend(data.attendances) : []),
      totalStudents: data.TotalStudents || data.totalStudents,
      presentCount: data.PresentCount || data.presentCount,
      absentCount: data.AbsentCount || data.absentCount,
      lateCount: data.LateCount || data.lateCount,
      attendanceRate: data.AttendanceRate || data.attendanceRate
    };
  }

  private mapSessionsFromBackend(data: any[]): AttendanceSession[] {
    return data.map(item => this.mapSessionFromBackend(item));
  }

  private mapAttendanceFromBackend(data: any): Attendance | null {
    // Handle null or undefined data
    if (!data) {
      console.warn('⚠️ Null attendance data received');
      return null;
    }
    
    return {
      attendanceId: data.AttendanceId || data.attendanceId,
      sessionId: data.SessionId || data.sessionId,
      studentId: data.StudentId || data.studentId,
      studentName: data.Student?.FullName || data.student?.fullName || data.studentName,
      status: data.Status || data.status,
      checkInTime: data.CheckInTime || data.checkInTime,
      notes: data.Notes || data.notes,
      markedByTeacherId: data.MarkedByTeacherId || data.markedByTeacherId,
      markedAt: data.MarkedAt || data.markedAt,
      session: data.Session ? this.mapSessionFromBackend(data.Session) : undefined,
      student: data.Student ? {
        studentId: data.Student.StudentId || data.Student.studentId,
        fullName: data.Student.FullName || data.Student.fullName,
        email: data.Student.Email || data.Student.email,
        phone: data.Student.Phone || data.Student.phone,
        address: data.Student.Address || data.Student.address,
        classId: data.Student.ClassId || data.Student.classId,
        className: data.Student.Class?.ClassName || data.Student.class?.className || data.Student.className,
        dateOfBirth: data.Student.DateOfBirth || data.Student.dateOfBirth,
        gender: data.Student.Gender !== undefined ? data.Student.Gender : data.Student.gender,
        username: data.Student.Username || data.Student.username
      } : undefined
    };
  }

  private mapAttendancesFromBackend(data: any[]): Attendance[] {
    if (!Array.isArray(data)) {
      console.warn('⚠️ mapAttendancesFromBackend received non-array:', data);
      return [];
    }
    
    // Filter out null/undefined items and map the rest
    return data
      .filter(item => item != null)
      .map(item => this.mapAttendanceFromBackend(item))
      .filter((item): item is Attendance => item !== null);
  }

  private mapStatsFromBackend(data: any[]): AttendanceStatistics[] {
    return data.map(item => ({
      courseId: item.CourseId || item.courseId,
      courseName: item.CourseName || item.courseName,
      totalSessions: item.TotalSessions || item.totalSessions || 0,
      presentCount: item.PresentCount || item.presentCount || 0,
      absentCount: item.AbsentCount || item.absentCount || 0,
      lateCount: item.LateCount || item.lateCount || 0,
      excusedCount: item.ExcusedCount || item.excusedCount || 0,
      attendanceRate: item.AttendanceRate || item.attendanceRate || 0
    }));
  }

  private mapWarningsFromBackend(data: any[]): AttendanceWarning[] {
    return data.map(item => ({
      studentId: item.StudentId || item.studentId,
      fullName: item.FullName || item.fullName,
      email: item.Email || item.email,
      phone: item.Phone || item.phone,
      totalSessions: item.TotalSessions || item.totalSessions || 0,
      absentCount: item.AbsentCount || item.absentCount || 0,
      absentRate: item.AbsentRate || item.absentRate || 0
    }));
  }

  // ============================================
  // UTILITY METHODS
  // ============================================

  /**
   * Get status badge class for UI
   */
  getStatusClass(status: string): string {
    switch (status) {
      case 'Present': return 'status-present';
      case 'Absent': return 'status-absent';
      case 'Late': return 'status-late';
      case 'Excused': return 'status-excused';
      case 'Completed': return 'status-completed';
      case 'Scheduled': return 'status-scheduled';
      case 'Cancelled': return 'status-cancelled';
      default: return '';
    }
  }

  /**
   * Get status label in Vietnamese
   */
  getStatusLabel(status: string): string {
    switch (status) {
      case 'Present': return 'Có mặt';
      case 'Absent': return 'Vắng';
      case 'Late': return 'Đi muộn';
      case 'Excused': return 'Có phép';
      case 'Completed': return 'Đã hoàn thành';
      case 'Scheduled': return 'Đã lên lịch';
      case 'Cancelled': return 'Đã hủy';
      default: return status;
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: string | Date): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('vi-VN');
  }

  /**
   * Format time for display
   */
  formatTime(time: string): string {
    // Input: "08:00:00" or "08:00"
    // Output: "08:00"
    return time.substring(0, 5);
  }

  /**
   * Calculate attendance rate color
   */
  getAttendanceRateColor(rate: number): string {
    if (rate >= 90) return 'green';
    if (rate >= 75) return 'blue';
    if (rate >= 60) return 'orange';
    return 'red';
  }
}
