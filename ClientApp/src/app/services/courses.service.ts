import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Course } from '../models/models';

export interface CoursesApiResponse {
  data: Course[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class CoursesService {
  private readonly API_URL = '/api/courses';

  constructor(private http: HttpClient) { }

  getCourses(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchString?: string,
    departmentId?: string,
    teacherId?: string
  ): Observable<CoursesApiResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchString) {
      params = params.set('searchString', searchString);
    }
    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }
    if (teacherId) {
      params = params.set('teacherId', teacherId);
    }

    return this.http.get<any>(this.API_URL, { params }).pipe(
      map(response => ({
        data: response.data.map((course: any) => ({
          courseId: course.courseId || course.CourseId,
          courseName: course.courseName || course.CourseName,
          credits: course.credits || course.Credits,
          departmentId: course.departmentId || course.DepartmentId,
          departmentName: course.departmentName || course.DepartmentName,
          departmentCode: course.departmentCode || course.DepartmentCode,
          teacherId: course.teacherId || course.TeacherId,
          teacherName: course.teacherName || course.TeacherName
        })),
        pageNumber: response.pageNumber,
        pageSize: response.pageSize,
        totalCount: response.totalCount,
        totalPages: response.totalPages
      }))
    );
  }

  getCourse(id: string): Observable<Course> {
    return this.http.get<any>(`${this.API_URL}/${id}`).pipe(
      map(course => ({
        courseId: course.courseId || course.CourseId,
        courseName: course.courseName || course.CourseName,
        credits: course.credits || course.Credits,
        departmentId: course.departmentId || course.DepartmentId,
        departmentName: course.departmentName || course.DepartmentName,
        departmentCode: course.departmentCode || course.DepartmentCode,
        teacherId: course.teacherId || course.TeacherId,
        teacherName: course.teacherName || course.TeacherName
      }))
    );
  }

  createCourse(course: Course): Observable<any> {
    console.log('Creating course:', course);
    const payload = {
      CourseId: course.courseId,
      CourseName: course.courseName,
      Credits: course.credits,
      DepartmentId: course.departmentId,
      TeacherId: course.teacherId || null
    };
    return this.http.post(this.API_URL, payload);
  }

  updateCourse(id: string, course: Course): Observable<any> {
    console.log('Updating course:', id, course);
    const payload = {
      CourseId: course.courseId,
      CourseName: course.courseName,
      Credits: course.credits,
      DepartmentId: course.departmentId,
      TeacherId: course.teacherId || null
    };
    return this.http.put(`${this.API_URL}/${id}`, payload);
  }

  deleteCourse(id: string): Observable<any> {
    console.log('Deleting course:', id);
    return this.http.delete(`${this.API_URL}/${id}`);
  }

  exportToExcel(searchString?: string, departmentId?: string, teacherId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (searchString) params = params.set('searchString', searchString);
    if (departmentId) params = params.set('departmentId', departmentId);
    if (teacherId) params = params.set('teacherId', teacherId);
    
    return this.http.get(`${this.API_URL}/export/excel`, { 
      params, 
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }

  exportToPdf(searchString?: string, departmentId?: string, teacherId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (searchString) params = params.set('searchString', searchString);
    if (departmentId) params = params.set('departmentId', departmentId);
    if (teacherId) params = params.set('teacherId', teacherId);
    
    return this.http.get(`${this.API_URL}/export/pdf`, { 
      params, 
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }

  getCoursesDropdown(departmentId?: string): Observable<Course[]> {
    let params = new HttpParams();
    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }
    
    return this.http.get<any>(`${this.API_URL}/dropdown`, { params }).pipe(
      map(courses => courses.map((course: any) => ({
        courseId: course.courseId || course.CourseId,
        courseName: course.courseName || course.CourseName,
        credits: course.credits || course.Credits
      })))
    );
  }
}
