// ============================================
// TypeScript Models - 100% synced with Backend
// ============================================

export interface User {
  userId?: number;
  username: string;
  password?: string;
  role: 'Admin' | 'Teacher' | 'Student';
  entityId?: string;
  fullName?: string;
  token?: string;
  email?: string;
  emailVerified?: boolean;
}

// Registration & Verification Models
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: 'Student' | 'Teacher';
  fullName?: string;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
  verificationCode?: string;
  email?: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface VerifyEmailResponse {
  success: boolean;
  message: string;
}

export interface ResendCodeRequest {
  email: string;
}

export interface ResendCodeResponse {
  success: boolean;
  message: string;
  verificationCode?: string;
}

export interface Student {
  studentId: string;
  fullName: string;
  dateOfBirth: string | Date;
  gender: boolean; // true = Male, false = Female
  email?: string; // Optional email field
  phone: string;
  address: string;
  classId: string;
  className?: string; // For display in lists
  departmentId?: string; // For filtering
  departmentName?: string; // For display in lists
  username: string;
  password?: string;
  class?: Class;
  grades?: Grade[];
  gradeCount?: number; // Số lượng điểm
  averageScore?: number;
}

export interface Teacher {
  teacherId: string;
  fullName: string;
  dateOfBirth: string | Date;
  gender: boolean; // true = Male, false = Female
  phone: string;
  address: string;
  username: string;
  password?: string;
  departmentId: string;
  departmentName?: string; // For display in lists
  department?: Department;
  classes?: Class[];
  courses?: Course[];
}

export interface Class {
  classId: string;
  className: string;
  departmentId: string;
  teacherId: string;
  department?: Department;
  teacher?: Teacher;
  departmentName?: string;
  teacherName?: string;
  students?: Student[];
  studentCount?: number;
}

export interface Course {
  courseId: string;
  courseName: string;
  credits: number;
  departmentId: string;
  teacherId: string;
  department?: Department;
  teacher?: Teacher;
  departmentName?: string;
  teacherName?: string;
  departmentCode?: string;
  grades?: Grade[];
}

export interface Grade {
  studentId: string;
  courseId: string;
  score: number;
  classification: string;
  student?: Student;
  course?: Course;
  studentName?: string;
  courseName?: string;
  className?: string;
}

export interface Department {
  departmentId: string;
  departmentCode: string;
  departmentName: string;
  teachers?: Teacher[];
  classes?: Class[];
  courses?: Course[];
  studentCount?: number;
  teacherCount?: number;
}

// ============================================
// API Response Models
// ============================================

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageIndex: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ============================================
// Dashboard Statistics
// ============================================

export interface DashboardStats {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalCourses: number;
  totalDepartments?: number;
  averageGrade?: number;
  excellentStudents?: number;
  goodStudents?: number;
  averageStudents?: number;
  gradeDistribution?: GradeDistribution[];
  studentsByDepartment?: DepartmentStats[];
  recentGrades?: Grade[];
  topStudents?: StudentRanking[];
}

export interface GradeDistribution {
  classification: string;
  count: number;
  percentage: number;
}

export interface DepartmentStats {
  departmentName: string;
  studentCount: number;
  teacherCount: number;
  classCount: number;
}

export interface StudentRanking {
  studentId: string;
  fullName: string;
  averageScore: number;
  className: string;
  rank: number;
}

// ============================================
// Filter & Search Models
// ============================================

export interface StudentFilter {
  searchString?: string;
  classId?: string;
  departmentId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface GradeFilter {
  studentId?: string;
  courseId?: string;
  classId?: string;
  semester?: string;
  pageNumber?: number;
  pageSize?: number;
}

// ============================================
// UI Models
// ============================================

export interface MenuItem {
  icon: string;
  label: string;
  route: string;
  roles: string[];
  badge?: number;
}

export interface Notification {
  id: number;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
  read: boolean;
}

export interface ThemeConfig {
  mode: 'light' | 'dark';
  primaryColor: string;
  accentColor: string;
  fontSize: 'small' | 'medium' | 'large';
}

// ============================================
// Chart Data Models
// ============================================

export interface ChartData {
  labels: string[];
  datasets: ChartDataset[];
}

export interface ChartDataset {
  label: string;
  data: number[];
  backgroundColor?: string | string[];
  borderColor?: string | string[];
  borderWidth?: number;
  tension?: number;
}

// ============================================
// Form Validation
// ============================================

export interface ValidationErrors {
  [key: string]: string;
}

export interface FormState<T> {
  data: T;
  errors: ValidationErrors;
  isSubmitting: boolean;
  isDirty: boolean;
}

// ============================================
// ATTENDANCE MANAGEMENT
// ============================================

export interface AttendanceSession {
  sessionId?: number;
  courseId: string;
  courseName?: string;
  teacherId: string;
  teacherName?: string;
  sessionDate: string | Date;
  sessionTime: string; // "08:00"
  sessionTitle: string;
  sessionType?: string; // "Lý thuyết", "Thực hành", "Kiểm tra"
  location?: string;
  duration?: number; // minutes
  notes?: string;
  status?: string; // "Scheduled", "Completed", "Cancelled"
  createdAt?: string | Date;
  updatedAt?: string | Date;
  
  // Navigation properties
  course?: Course;
  teacher?: Teacher;
  attendances?: Attendance[];
  
  // Computed properties
  totalStudents?: number;
  presentCount?: number;
  absentCount?: number;
  lateCount?: number;
  attendanceRate?: number;
}

export interface Attendance {
  attendanceId?: number;
  sessionId: number;
  studentId: string;
  studentName?: string;
  status: 'Present' | 'Absent' | 'Late' | 'Excused';
  checkInTime?: string; // "08:15"
  notes?: string;
  markedByTeacherId?: string;
  markedAt?: string | Date;
  
  // Navigation properties
  session?: AttendanceSession;
  student?: Student;
}

export interface MarkAttendanceRequest {
  sessionId: number;
  teacherId: string;
  attendances: AttendanceRecord[];
}

export interface AttendanceRecord {
  studentId: string;
  status: 'Present' | 'Absent' | 'Late' | 'Excused';
  checkInTime?: string;
  notes?: string;
}

export interface AttendanceStatistics {
  courseId: string;
  courseName: string;
  totalSessions: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  excusedCount: number;
  attendanceRate: number;
}

export interface AttendanceWarning {
  studentId: string;
  fullName: string;
  email?: string;
  phone?: string;
  totalSessions: number;
  absentCount: number;
  absentRate: number;
}

export interface CreateSessionRequest {
  courseId: string;
  teacherId: string;
  sessionDate: string;
  sessionTime: string;
  sessionTitle: string;
  sessionType?: string;
  location?: string;
  duration?: number;
}
