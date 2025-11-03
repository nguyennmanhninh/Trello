import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface TeacherClass {
  classId: string;
  className: string;
  departmentName: string;
  studentCount: number;
}

interface TeacherCourse {
  courseId: string;
  courseName: string;
  credits: number;
  departmentName: string;
  studentCount: number;
}

interface TeacherDashboardData {
  teacherClasses: TeacherClass[];
  teacherCourses: TeacherCourse[];
  totalClasses: number;
  totalCourses: number;
  totalStudents: number;
}

@Component({
  selector: 'app-dashboard-teacher',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-teacher.component.html',
  styleUrl: './dashboard-teacher.component.scss'
})
export class DashboardTeacherComponent implements OnInit {
  teacherClasses: TeacherClass[] = [];
  teacherCourses: TeacherCourse[] = [];
  totalClasses: number = 0;
  totalCourses: number = 0;
  totalStudents: number = 0;
  
  loading: boolean = true;
  error: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = '';

    this.http.get<any>('/api/dashboard/teacher-stats').subscribe({
      next: (data) => {
        console.log('📊 Teacher dashboard raw data:', data);
        
        // Map PascalCase to camelCase
        this.teacherClasses = (data.teacherClasses || data.TeacherClasses || []).map((c: any) => ({
          classId: c.classId || c.ClassId || '',
          className: c.className || c.ClassName || '',
          departmentName: c.departmentName || c.DepartmentName || c.Department?.departmentName || c.Department?.DepartmentName || '',
          studentCount: c.studentCount || c.StudentCount || 0
        }));

        this.teacherCourses = (data.teacherCourses || data.TeacherCourses || []).map((c: any) => ({
          courseId: c.courseId || c.CourseId || '',
          courseName: c.courseName || c.CourseName || '',
          credits: c.credits || c.Credits || 0,
          departmentName: c.departmentName || c.DepartmentName || c.Department?.departmentName || c.Department?.DepartmentName || '',
          studentCount: c.studentCount || c.StudentCount || 0
        }));

        this.totalClasses = this.teacherClasses.length;
        this.totalCourses = this.teacherCourses.length;
        
        // Calculate total unique students from classes
        this.totalStudents = this.teacherClasses.reduce((sum, c) => sum + c.studentCount, 0);

        console.log('✅ Mapped data:', {
          classes: this.teacherClasses,
          courses: this.teacherCourses,
          totalClasses: this.totalClasses,
          totalCourses: this.totalCourses,
          totalStudents: this.totalStudents
        });

        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading teacher dashboard:', err);
        this.error = 'Không thể tải dữ liệu dashboard';
        this.loading = false;
      }
    });
  }
}
