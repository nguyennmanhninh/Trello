import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Class } from '../models/models';

interface ClassesApiResponse {
  data: any[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ClassesService {
  private readonly apiUrl = '/api/classes';

  constructor(private http: HttpClient) { }

  getClasses(pageNumber: number = 1, pageSize: number = 10, searchString: string = '', departmentId: string = ''): Observable<ClassesApiResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchString) {
      params = params.set('searchString', searchString);
    }

    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }

    return this.http.get<any>(this.apiUrl, { params }).pipe(
      map(response => {
        console.log('📡 ClassesService - Raw response:', response);
        
        // Map PascalCase to camelCase
        const mappedClasses = response.data.map((c: any) => ({
          classId: c.ClassId || c.classId,
          className: c.ClassName || c.className,
          departmentId: c.DepartmentId || c.departmentId,
          departmentName: c.DepartmentName || c.departmentName,
          departmentCode: c.DepartmentCode || c.departmentCode,
          teacherId: c.TeacherId || c.teacherId,
          teacherName: c.TeacherName || c.teacherName,
          studentCount: c.StudentCount !== undefined ? c.StudentCount : c.studentCount
        }));

        console.log('📡 ClassesService - Mapped classes:', mappedClasses);

        return {
          data: mappedClasses,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages
        };
      })
    );
  }

  getClass(id: string): Observable<Class> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(c => ({
        classId: c.ClassId || c.classId,
        className: c.ClassName || c.className,
        departmentId: c.DepartmentId || c.departmentId,
        teacherId: c.TeacherId || c.teacherId,
        departmentName: c.DepartmentName || c.departmentName,
        teacherName: c.TeacherName || c.teacherName,
        studentCount: c.StudentCount !== undefined ? c.StudentCount : c.studentCount
      }))
    );
  }

  createClass(classData: Class): Observable<Class> {
    // Convert camelCase to PascalCase for backend
    const payload = {
      ClassId: classData.classId,
      ClassName: classData.className,
      DepartmentId: classData.departmentId,
      TeacherId: classData.teacherId
    };

    console.log('📡 ClassesService - Creating class:', payload);
    return this.http.post<Class>(this.apiUrl, payload);
  }

  updateClass(id: string, classData: Class): Observable<Class> {
    // Convert camelCase to PascalCase for backend
    const payload = {
      ClassId: classData.classId,
      ClassName: classData.className,
      DepartmentId: classData.departmentId,
      TeacherId: classData.teacherId
    };

    console.log('📡 ClassesService - Updating class:', payload);
    return this.http.put<Class>(`${this.apiUrl}/${id}`, payload);
  }

  deleteClass(id: string): Observable<void> {
    console.log('📡 ClassesService - Deleting class:', id);
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  exportToExcel(searchString: string = '', departmentId: string = ''): Observable<Blob> {
    let params = new HttpParams();
    if (searchString) {
      params = params.set('searchString', searchString);
    }
    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }

    return this.http.get(`${this.apiUrl}/export/excel`, {
      params,
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }

  exportToPdf(searchString: string = '', departmentId: string = ''): Observable<Blob> {
    let params = new HttpParams();
    if (searchString) {
      params = params.set('searchString', searchString);
    }
    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }

    return this.http.get(`${this.apiUrl}/export/pdf`, {
      params,
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies for Teacher role filtering
    });
  }
}
