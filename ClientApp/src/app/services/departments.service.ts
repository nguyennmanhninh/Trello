import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Department, ApiResponse } from '../models/models';

export interface DepartmentDto {
  departmentId: string;
  departmentCode: string;
  departmentName: string;
}

export interface DepartmentsListResponse {
  data: Department[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class DepartmentsService {
  private readonly API_URL = 'http://localhost:5298/api/departments';

  constructor(private http: HttpClient) { }

  getDepartments(): Observable<Department[]> {
    console.log('📡 DepartmentsService - Calling API:', this.API_URL);
    return this.http.get<DepartmentsListResponse>(this.API_URL).pipe(
      map(response => {
        console.log('📡 DepartmentsService - Raw response:', response);
        // Backend returns { data: [...], pageNumber, pageSize, totalCount, totalPages }
        // Extract the data array and map PascalCase to camelCase
        const departments = (response.data || []).map((d: any) => ({
          departmentId: d.DepartmentId || d.departmentId,
          departmentCode: d.DepartmentCode || d.departmentCode,
          departmentName: d.DepartmentName || d.departmentName
        }));
        console.log('📡 DepartmentsService - Mapped departments:', departments);
        return departments;
      })
    );
  }

  getDepartment(id: string): Observable<Department> {
    console.log('📡 DepartmentsService - Getting department:', id);
    return this.http.get<Department>(`${this.API_URL}/${id}`).pipe(
      map((d: any) => ({
        departmentId: d.DepartmentId || d.departmentId,
        departmentCode: d.DepartmentCode || d.departmentCode,
        departmentName: d.DepartmentName || d.departmentName
      }))
    );
  }

  createDepartment(dept: DepartmentDto): Observable<ApiResponse<Department>> {
    console.log('📡 DepartmentsService - Creating department:', dept);
    // Backend expects PascalCase
    const payload = {
      DepartmentId: dept.departmentId,
      DepartmentCode: dept.departmentCode,
      DepartmentName: dept.departmentName
    };
    return this.http.post<ApiResponse<Department>>(this.API_URL, payload);
  }

  updateDepartment(id: string, dept: DepartmentDto): Observable<ApiResponse<any>> {
    console.log('📡 DepartmentsService - Updating department:', id, dept);
    // Backend expects PascalCase
    const payload = {
      DepartmentId: dept.departmentId,
      DepartmentCode: dept.departmentCode,
      DepartmentName: dept.departmentName
    };
    return this.http.put<ApiResponse<any>>(`${this.API_URL}/${id}`, payload);
  }

  deleteDepartment(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.API_URL}/${id}`);
  }
}
