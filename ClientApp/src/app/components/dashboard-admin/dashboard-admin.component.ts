import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Chart, registerables } from 'chart.js';
import { ModalService } from '../../services/modal.service';

Chart.register(...registerables);

interface DashboardStats {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalCourses: number;
  totalDepartments: number;
}

@Component({
  selector: 'app-dashboard-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-admin.component.html',
  styleUrl: './dashboard-admin.component.scss'
})
export class DashboardAdminComponent implements OnInit {
  stats: DashboardStats = {
    totalStudents: 0,
    totalTeachers: 0,
    totalClasses: 0,
    totalCourses: 0,
    totalDepartments: 0
  };
  
  loading: boolean = true;
  error: string = '';
  
  private studentsByClassChart: Chart | null = null;
  private studentsByDepartmentChart: Chart | null = null;

  constructor(
    private http: HttpClient,
    private router: Router,
    private modalService: ModalService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = '';

    console.log('Loading dashboard data...');

    // Load statistics from backend
    this.http.get<any>('/api/dashboard/admin-stats').subscribe({
      next: (data) => {
        console.log('Admin stats received:', data);
        this.stats = {
          totalStudents: data.totalStudents || data.TotalStudents || 0,
          totalTeachers: data.totalTeachers || data.TotalTeachers || 0,
          totalClasses: data.totalClasses || data.TotalClasses || 0,
          totalCourses: data.totalCourses || data.TotalCourses || 0,
          totalDepartments: data.totalDepartments || data.TotalDepartments || 0
        };
        this.loading = false;
        
        console.log('Stats loaded, now loading charts...');
        // Load chart data after stats are loaded and view is initialized
        this.loadChartData();
      },
      error: (err) => {
        console.error('Error loading dashboard data:', err);
        this.error = 'Không thể tải dữ liệu dashboard';
        this.loading = false;
      }
    });
  }

  loadChartData(): void {
    // Wait for DOM to be ready
    setTimeout(() => {
      // Load students by class chart
      this.http.get<any>('/api/dashboard/students-by-class').subscribe({
        next: (data) => {
          this.createStudentsByClassChart(data);
        },
        error: (err) => {
          console.error('Error loading students by class:', err);
        }
      });

      // Load students by department chart
      this.http.get<any>('/api/dashboard/students-by-department').subscribe({
        next: (data) => {
          this.createStudentsByDepartmentChart(data);
        },
        error: (err) => {
          console.error('Error loading students by department:', err);
        }
      });
    }, 300);
  }

  createStudentsByClassChart(data: any): void {
    console.log('Creating students by class chart with data:', data);
    const canvas = document.getElementById('studentsByClassChart') as HTMLCanvasElement;
    if (!canvas) {
      console.error('Canvas studentsByClassChart not found');
      return;
    }

    // Destroy existing chart if any
    if (this.studentsByClassChart) {
      this.studentsByClassChart.destroy();
    }

    const labels = Object.keys(data);
    const values = Object.values(data) as number[];

    console.log('Chart labels:', labels);
    console.log('Chart values:', values);

    this.studentsByClassChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Số sinh viên',
          data: values,
          backgroundColor: 'rgba(74, 144, 226, 0.8)',
          borderColor: 'rgba(74, 144, 226, 1)',
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          title: {
            display: true,
            text: 'Số lượng sinh viên theo lớp'
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              stepSize: 1
            }
          }
        }
      }
    });
  }

  createStudentsByDepartmentChart(data: any): void {
    console.log('Creating students by department chart with data:', data);
    const canvas = document.getElementById('studentsByDepartmentChart') as HTMLCanvasElement;
    if (!canvas) {
      console.error('Canvas studentsByDepartmentChart not found');
      return;
    }

    // Destroy existing chart if any
    if (this.studentsByDepartmentChart) {
      this.studentsByDepartmentChart.destroy();
    }

    const labels = Object.keys(data);
    const values = Object.values(data) as number[];

    const colors = [
      'rgba(255, 99, 132, 0.8)',
      'rgba(54, 162, 235, 0.8)',
      'rgba(255, 206, 86, 0.8)',
      'rgba(75, 192, 192, 0.8)',
      'rgba(153, 102, 255, 0.8)',
      'rgba(255, 159, 64, 0.8)'
    ];

    this.studentsByDepartmentChart = new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          label: 'Số sinh viên',
          data: values,
          backgroundColor: colors,
          borderColor: colors.map(c => c.replace('0.8', '1')),
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom'
          },
          title: {
            display: true,
            text: 'Phân bổ sinh viên theo khoa'
          }
        }
      }
    });
  }

  ngOnDestroy(): void {
    if (this.studentsByClassChart) {
      this.studentsByClassChart.destroy();
    }
    if (this.studentsByDepartmentChart) {
      this.studentsByDepartmentChart.destroy();
    }
  }

  // Quick action methods
  openStudentModal(): void {
    console.log('🎯 Dashboard: Navigating to students page...');
    this.router.navigate(['/students']).then(() => {
      console.log('✅ Dashboard: Navigation complete, triggering modal...');
      // Delay to ensure component subscription is ready
      setTimeout(() => {
        console.log('📢 Dashboard: Calling modalService.triggerStudentModal()');
        this.modalService.triggerStudentModal();
      }, 300);
    });
  }

  openTeacherModal(): void {
    console.log('🎯 Dashboard: Navigating to teachers page...');
    this.router.navigate(['/teachers']).then(() => {
      console.log('✅ Dashboard: Navigation complete, triggering modal...');
      setTimeout(() => {
        console.log('📢 Dashboard: Calling modalService.triggerTeacherModal()');
        this.modalService.triggerTeacherModal();
      }, 300);
    });
  }

  openClassModal(): void {
    console.log('🎯 Dashboard: Navigating to classes page...');
    this.router.navigate(['/classes']).then(() => {
      console.log('✅ Dashboard: Navigation complete, triggering modal...');
      setTimeout(() => {
        console.log('📢 Dashboard: Calling modalService.triggerClassModal()');
        this.modalService.triggerClassModal();
      }, 300);
    });
  }
}
