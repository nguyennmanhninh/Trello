import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

interface StudentClass {
  classId: string;
  className: string;
  departmentName: string;
}

interface StudentGrade {
  courseId: string;
  courseName: string;
  score: number;
  classification: string;
  credits: number;
}

interface StudentDashboardData {
  studentClass: StudentClass | null;
  studentGrades: StudentGrade[];
  averageScore: number;
}

@Component({
  selector: 'app-dashboard-student',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-student.component.html',
  styleUrl: './dashboard-student.component.scss'
})
export class DashboardStudentComponent implements OnInit {
  studentClass: StudentClass | null = null;
  studentGrades: StudentGrade[] = [];
  averageScore: number = 0;
  totalCredits: number = 0;
  
  loading: boolean = true;
  error: string = '';
  
  private gradeChart: Chart | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = '';

    this.http.get<any>('/api/dashboard/student-stats').subscribe({
      next: (data) => {
        console.log('📊 Student dashboard raw data:', data);
        
        // Map student class
        const classData = data.studentClass || data.StudentClass;
        if (classData) {
          this.studentClass = {
            classId: classData.classId || classData.ClassId || '',
            className: classData.className || classData.ClassName || '',
            departmentName: classData.departmentName || classData.DepartmentName || classData.Department?.departmentName || classData.Department?.DepartmentName || ''
          };
        }

        // Map student grades
        this.studentGrades = (data.studentGrades || data.StudentGrades || []).map((g: any) => ({
          courseId: g.courseId || g.CourseId || '',
          courseName: g.courseName || g.CourseName || g.Course?.courseName || g.Course?.CourseName || '',
          score: g.score || g.Score || 0,
          classification: g.classification || g.Classification || '',
          credits: g.credits || g.Credits || g.Course?.credits || g.Course?.Credits || 0
        }));

        this.averageScore = data.averageScore || data.AverageScore || 0;
        this.totalCredits = this.studentGrades.reduce((sum, g) => sum + g.credits, 0);

        console.log('✅ Mapped data:', {
          class: this.studentClass,
          grades: this.studentGrades,
          averageScore: this.averageScore,
          totalCredits: this.totalCredits
        });

        this.loading = false;
        
        // Create chart after data is loaded
        if (this.studentGrades.length > 0) {
          setTimeout(() => this.createGradeChart(), 100);
        }
      },
      error: (err) => {
        console.error('Error loading student dashboard:', err);
        this.error = 'Không thể tải dữ liệu dashboard';
        this.loading = false;
      }
    });
  }

  createGradeChart(): void {
    const canvas = document.getElementById('gradeChart') as HTMLCanvasElement;
    if (!canvas) return;

    // Destroy existing chart if any
    if (this.gradeChart) {
      this.gradeChart.destroy();
    }

    const labels = this.studentGrades.map(g => g.courseName);
    const scores = this.studentGrades.map(g => g.score);
    
    // Color code based on score
    const backgroundColors = scores.map(score => {
      if (score >= 9) return 'rgba(39, 174, 96, 0.8)'; // Xuất sắc - green
      if (score >= 8) return 'rgba(52, 152, 219, 0.8)'; // Giỏi - blue
      if (score >= 7) return 'rgba(241, 196, 15, 0.8)'; // Khá - yellow
      if (score >= 5) return 'rgba(230, 126, 34, 0.8)'; // Trung bình - orange
      return 'rgba(231, 76, 60, 0.8)'; // Yếu/Kém - red
    });

    this.gradeChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Điểm',
          data: scores,
          backgroundColor: backgroundColors,
          borderColor: backgroundColors.map(c => c.replace('0.8', '1')),
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
            text: 'Biểu đồ điểm các môn học'
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            max: 10,
            ticks: {
              stepSize: 1
            }
          }
        }
      }
    });
  }

  getClassificationClass(classification: string): string {
    switch (classification) {
      case 'Xuất sắc': return 'grade-excellent';
      case 'Giỏi': return 'grade-good';
      case 'Khá': return 'grade-fair';
      case 'Trung bình': return 'grade-average';
      case 'Yếu': return 'grade-weak';
      case 'Kém': return 'grade-poor';
      default: return '';
    }
  }

  getScoreClass(score: number): string {
    if (score >= 9) return 'score-excellent';
    if (score >= 8) return 'score-good';
    if (score >= 7) return 'score-fair';
    if (score >= 5) return 'score-average';
    return 'score-weak';
  }

  getOverallClassification(): string {
    const score = this.averageScore;
    if (score >= 9) return 'Xuất sắc';
    if (score >= 8) return 'Giỏi';
    if (score >= 7) return 'Khá';
    if (score >= 5) return 'Trung bình';
    if (score >= 4) return 'Yếu';
    return 'Kém';
  }

  ngOnDestroy(): void {
    if (this.gradeChart) {
      this.gradeChart.destroy();
    }
  }
}
