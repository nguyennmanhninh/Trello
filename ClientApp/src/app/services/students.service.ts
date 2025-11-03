import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Student, PaginatedResponse, ApiResponse } from '../models/models';

export interface StudentDto {
  studentId: string;
  fullName: string;
  dateOfBirth: string | Date;
  gender: boolean;
  email?: string; // Add email field
  phone?: string;
  address?: string;
  classId: string;
  username: string;
  password?: string;
}

export interface StudentFilterParams {
  searchString?: string;
  classId?: string;
  departmentId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface StudentsApiResponse {
  data: any[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class StudentsService {
  private readonly API_URL = 'http://localhost:5298/api/students';

  constructor(private http: HttpClient) { }

  getStudents(params: StudentFilterParams): Observable<StudentsApiResponse> {
    console.log('📡 StudentsService - Calling API with params:', params);
    
    let httpParams = new HttpParams();
    
    if (params.searchString) httpParams = httpParams.set('searchString', params.searchString);
    if (params.classId) httpParams = httpParams.set('classId', params.classId);
    if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<StudentsApiResponse>(this.API_URL, { params: httpParams }).pipe(
      map(response => {
        console.log('📡 StudentsService - Raw response:', response);
        // Map PascalCase to camelCase
        const students: Student[] = (response.data || []).map((s: any) => ({
          studentId: s.StudentId || s.studentId,
          fullName: s.FullName || s.fullName,
          dateOfBirth: s.DateOfBirth || s.dateOfBirth,
          gender: s.Gender !== undefined ? s.Gender : s.gender,
          phone: s.Phone || s.phone || '',
          address: s.Address || s.address || '',
          classId: s.ClassId || s.classId,
          className: s.ClassName || s.className,
          departmentId: s.DepartmentId || s.departmentId,
          departmentName: s.DepartmentName || s.departmentName,
          username: s.Username || s.username || '',
          grades: []
        }));
        
        console.log('📡 StudentsService - Mapped students:', students);
        
        return {
          data: students,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages
        };
      })
    );
  }

  getStudent(id: string): Observable<Student> {
    console.log('📡 StudentsService - Getting student:', id);
    return this.http.get<any>(`${this.API_URL}/${id}`).pipe(
      map((s: any) => ({
        studentId: s.StudentId || s.studentId,
        fullName: s.FullName || s.fullName,
        dateOfBirth: s.DateOfBirth || s.dateOfBirth,
        gender: s.Gender !== undefined ? s.Gender : s.gender,
        phone: s.Phone || s.phone || '',
        address: s.Address || s.address || '',
        classId: s.ClassId || s.classId,
        className: s.ClassName || s.className,
        departmentName: s.DepartmentName || s.departmentName,
        username: s.Username || s.username || '',
        grades: s.Grades || s.grades || [],
        gradeCount: s.GradeCount || s.gradeCount || 0
      }))
    );
  }

  createStudent(student: StudentDto): Observable<ApiResponse<Student>> {
    console.log('📡 StudentsService - Creating student:', student);
    // Backend expects PascalCase
    const payload = {
      StudentId: student.studentId,
      FullName: student.fullName,
      DateOfBirth: student.dateOfBirth,
      Gender: student.gender,
      Phone: student.phone || '',
      Address: student.address || '',
      ClassId: student.classId,
      Username: student.username,
      Password: student.password
    };
    return this.http.post<ApiResponse<Student>>(this.API_URL, payload);
  }

  updateStudent(id: string, student: StudentDto): Observable<ApiResponse<any>> {
    console.log('📡 StudentsService - Updating student:', id, student);
    // Backend expects PascalCase
    const payload = {
      StudentId: student.studentId,
      FullName: student.fullName,
      DateOfBirth: student.dateOfBirth,
      Gender: student.gender,
      Email: student.email || null, // Add Email to payload
      Phone: student.phone || '',
      Address: student.address || '',
      ClassId: student.classId,
      Username: student.username,
      Password: student.password || undefined
    };
    return this.http.put<ApiResponse<any>>(`${this.API_URL}/${id}`, payload);
  }

  deleteStudent(id: string): Observable<ApiResponse<any>> {
    console.log('📡 StudentsService - Deleting student:', id);
    return this.http.delete<ApiResponse<any>>(`${this.API_URL}/${id}`);
  }

  deleteAllGrades(studentId: string): Observable<ApiResponse<any>> {
    console.log('📡 StudentsService - Deleting all grades for student:', studentId);
    return this.http.delete<ApiResponse<any>>(`http://localhost:5298/api/grades/DeleteAllByStudent/${studentId}`);
  }

  exportToExcel(params: StudentFilterParams): Observable<Blob> {
    let httpParams = new HttpParams();
    
    if (params.searchString) httpParams = httpParams.set('searchString', params.searchString);
    if (params.classId) httpParams = httpParams.set('classId', params.classId);
    if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId);

    return this.http.get(`${this.API_URL}/export/excel`, { 
      params: httpParams,
      responseType: 'blob',
      withCredentials: true  // ✅ CRITICAL: Send session cookies for Teacher role filtering
    });
  }

  exportToPdf(params: StudentFilterParams): Observable<Blob> {
    let httpParams = new HttpParams();
    
    if (params.searchString) httpParams = httpParams.set('searchString', params.searchString);
    if (params.classId) httpParams = httpParams.set('classId', params.classId);
    if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId);

    return this.http.get(`${this.API_URL}/export/pdf`, { 
      params: httpParams,
      responseType: 'blob',
      withCredentials: true  // ✅ CRITICAL: Send session cookies for Teacher role filtering
    });
  }
}
