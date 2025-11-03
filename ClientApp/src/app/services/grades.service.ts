import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Grade } from '../models/models';

export interface GradesApiResponse {
  data: Grade[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class GradesService {
  private readonly API_URL = '/api/grades';

  constructor(private http: HttpClient) { }

  getGrades(
    pageNumber: number = 1,
    pageSize: number = 15,
    classId?: string,
    courseId?: string
  ): Observable<GradesApiResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (classId) {
      params = params.set('classId', classId);
    }
    if (courseId) {
      params = params.set('courseId', courseId);
    }

    return this.http.get<any>(this.API_URL, { params }).pipe(
      map(response => ({
        data: response.data.map((grade: any) => ({
          studentId: grade.studentId || grade.StudentId,
          studentName: grade.studentName || grade.StudentName,
          className: grade.className || grade.ClassName,
          courseId: grade.courseId || grade.CourseId,
          courseName: grade.courseName || grade.CourseName,
          score: grade.score || grade.Score,
          classification: grade.classification || grade.Classification
        })),
        pageNumber: response.pageNumber,
        pageSize: response.pageSize,
        totalCount: response.totalCount,
        totalPages: response.totalPages
      }))
    );
  }

  getGrade(studentId: string, courseId: string): Observable<Grade> {
    return this.http.get<any>(`${this.API_URL}/${studentId}/${courseId}`).pipe(
      map(grade => ({
        studentId: grade.studentId || grade.StudentId,
        studentName: grade.studentName || grade.StudentName,
        courseId: grade.courseId || grade.CourseId,
        courseName: grade.courseName || grade.CourseName,
        score: grade.score || grade.Score,
        classification: grade.classification || grade.Classification
      }))
    );
  }

  createGrade(grade: Grade): Observable<any> {
    console.log('Creating grade:', grade);
    const payload = {
      StudentId: grade.studentId,
      CourseId: grade.courseId,
      Score: grade.score
    };
    return this.http.post(this.API_URL, payload);
  }

  updateGrade(studentId: string, courseId: string, grade: Grade): Observable<any> {
    console.log('Updating grade:', studentId, courseId, grade);
    const payload = {
      StudentId: grade.studentId,
      CourseId: grade.courseId,
      Score: grade.score
    };
    return this.http.put(`${this.API_URL}/${studentId}/${courseId}`, payload);
  }

  deleteGrade(studentId: string, courseId: string): Observable<any> {
    console.log('Deleting grade:', studentId, courseId);
    return this.http.delete(`${this.API_URL}/${studentId}/${courseId}`);
  }

  getStudentGrades(studentId: string): Observable<Grade[]> {
    return this.http.get<any>(`${this.API_URL}/student/${studentId}`).pipe(
      map(grades => grades.map((grade: any) => ({
        studentId: grade.studentId || grade.StudentId,
        courseId: grade.courseId || grade.CourseId,
        courseName: grade.courseName || grade.CourseName,
        score: grade.score || grade.Score,
        classification: grade.classification || grade.Classification
      })))
    );
  }

  getCourseGrades(courseId: string): Observable<Grade[]> {
    return this.http.get<any>(`${this.API_URL}/course/${courseId}`).pipe(
      map(grades => grades.map((grade: any) => ({
        studentId: grade.studentId || grade.StudentId,
        studentName: grade.studentName || grade.StudentName,
        courseId: grade.courseId || grade.CourseId,
        score: grade.score || grade.Score,
        classification: grade.classification || grade.Classification
      })))
    );
  }

  exportToExcel(classId?: string, courseId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (classId) params = params.set('classId', classId);
    if (courseId) params = params.set('courseId', courseId);
    
    return this.http.get(`${this.API_URL}/export/excel`, { 
      params, 
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }

  exportToPdf(classId?: string, courseId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (classId) params = params.set('classId', classId);
    if (courseId) params = params.set('courseId', courseId);
    
    return this.http.get(`${this.API_URL}/export/pdf`, { 
      params, 
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }

  // Calculate classification on frontend (same logic as backend)
  calculateClassification(score: number): string {
    if (score >= 9.0) return 'Xuất sắc';
    if (score >= 8.0) return 'Giỏi';
    if (score >= 7.0) return 'Khá';
    if (score >= 5.5) return 'Trung bình';
    if (score >= 4.0) return 'Yếu';
    return 'Kém';
  }

  // Get classification color for badges
  getClassificationColor(classification: string): string {
    switch (classification) {
      case 'Xuất sắc': return 'excellent';
      case 'Giỏi': return 'good';
      case 'Khá': return 'average';
      case 'Trung bình': return 'below-average';
      case 'Yếu': return 'weak';
      case 'Kém': return 'poor';
      default: return 'default';
    }
  }
}
