import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStatsResponse {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalCourses: number;
  totalDepartments: number;
  gradeDistribution: GradeDistributionItem[];
  studentsByDepartment: DepartmentStudentCount[];
}

export interface GradeDistributionItem {
  gradeRange: string;
  count: number;
}

export interface DepartmentStudentCount {
  departmentName: string;
  studentCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly API_URL = 'http://localhost:5298/api';

  constructor(private http: HttpClient) { }

  getStatistics(): Observable<DashboardStatsResponse> {
    return this.http.get<DashboardStatsResponse>(`${this.API_URL}/dashboard/stats`);
  }
}
