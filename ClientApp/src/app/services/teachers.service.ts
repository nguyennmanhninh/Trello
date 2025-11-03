import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Teacher } from '../models/models';

interface TeachersApiResponse {
  items: any[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class TeachersService {
  private readonly apiUrl = '/api/teachers';

  constructor(private http: HttpClient) { }

  getTeachers(pageNumber: number = 1, pageSize: number = 10, searchTerm: string = ''): Observable<TeachersApiResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<any>(this.apiUrl, { params }).pipe(
      map(response => {
        console.log('📡 TeachersService - Raw response:', response);
        
        // Map PascalCase to camelCase
        const mappedItems = response.items.map((t: any) => ({
          teacherId: t.TeacherId || t.teacherId,
          fullName: t.FullName || t.fullName,
          dateOfBirth: t.DateOfBirth || t.dateOfBirth,
          gender: t.Gender !== undefined ? t.Gender : t.gender,
          phone: t.Phone || t.phone,
          address: t.Address || t.address,
          username: t.Username || t.username,
          password: t.Password || t.password || '',
          departmentId: t.DepartmentId || t.departmentId,
          departmentName: t.DepartmentName || t.departmentName
        }));

        console.log('📡 TeachersService - Mapped teachers:', mappedItems);

        return {
          items: mappedItems,
          totalCount: response.totalCount,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalPages: response.totalPages
        };
      })
    );
  }

  getTeacher(id: string): Observable<Teacher> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(t => ({
        teacherId: t.TeacherId || t.teacherId,
        fullName: t.FullName || t.fullName,
        dateOfBirth: t.DateOfBirth || t.dateOfBirth,
        gender: t.Gender !== undefined ? t.Gender : t.gender,
        phone: t.Phone || t.phone,
        address: t.Address || t.address,
        username: t.Username || t.username,
        password: t.Password || t.password || '',
        departmentId: t.DepartmentId || t.departmentId,
        departmentName: t.DepartmentName || t.departmentName
      }))
    );
  }

  createTeacher(teacher: Teacher): Observable<Teacher> {
    // Convert camelCase to PascalCase for backend
    const payload = {
      TeacherId: teacher.teacherId,
      FullName: teacher.fullName,
      DateOfBirth: teacher.dateOfBirth,
      Gender: teacher.gender,
      Phone: teacher.phone,
      Address: teacher.address,
      Username: teacher.username,
      Password: teacher.password,
      DepartmentId: teacher.departmentId
    };

    return this.http.post<Teacher>(this.apiUrl, payload);
  }

  updateTeacher(id: string, teacher: Teacher): Observable<Teacher> {
    // Convert camelCase to PascalCase for backend
    const payload = {
      TeacherId: teacher.teacherId,
      FullName: teacher.fullName,
      DateOfBirth: teacher.dateOfBirth,
      Gender: teacher.gender,
      Phone: teacher.phone,
      Address: teacher.address,
      Username: teacher.username,
      Password: teacher.password,
      DepartmentId: teacher.departmentId
    };

    return this.http.put<Teacher>(`${this.apiUrl}/${id}`, payload);
  }

  deleteTeacher(id: string): Observable<void> {
    console.log('📡 TeachersService - Deleting teacher:', id);
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  exportToExcel(searchTerm: string = ''): Observable<Blob> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get(`${this.apiUrl}/export/excel`, {
      params,
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies
    });
  }

  exportToPdf(searchTerm: string = ''): Observable<Blob> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get(`${this.apiUrl}/export/pdf`, {
      params,
      responseType: 'blob',
      withCredentials: true  // ✅ Send session cookies
    });
  }
}
