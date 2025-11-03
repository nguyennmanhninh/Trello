import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent)
  },

  {
    path: '',
    loadComponent: () => import('./components/layout/layout.component').then(m => m.LayoutComponent),
    children: [
      {
        path: 'dashboard-admin',
        loadComponent: () => import('./components/dashboard-admin/dashboard-admin.component').then(m => m.DashboardAdminComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'dashboard-teacher',
        loadComponent: () => import('./components/dashboard-teacher/dashboard-teacher.component').then(m => m.DashboardTeacherComponent),
        canActivate: [authGuard],
        data: { roles: ['Teacher'] }
      },
      {
        path: 'dashboard-student',
        loadComponent: () => import('./components/dashboard-student/dashboard-student.component').then(m => m.DashboardStudentComponent),
        canActivate: [authGuard],
        data: { roles: ['Student'] }
      },
      {
        path: 'students',
        loadComponent: () => import('./components/students/students.component').then(m => m.StudentsComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher', 'Student'] }
      },
      {
        path: 'teachers',
        loadComponent: () => import('./components/teachers/teachers.component').then(m => m.TeachersComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'classes',
        loadComponent: () => import('./components/classes/classes.component').then(m => m.ClassesComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher'] }
      },
      {
        path: 'courses',
        loadComponent: () => import('./components/courses/courses.component').then(m => m.CoursesComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher', 'Student'] } // Student can view courses
      },
      {
        path: 'grades',
        loadComponent: () => import('./components/grades/grades.component').then(m => m.GradesComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher', 'Student'] }
      },
      {
        path: 'departments',
        loadComponent: () => import('./components/departments/departments.component').then(m => m.DepartmentsComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'profile',
        loadComponent: () => import('./components/profile/profile.component').then(m => m.ProfileComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher', 'Student'] }
      },
      // ATTENDANCE MANAGEMENT ROUTES
      {
        path: 'attendance',
        loadComponent: () => import('./components/attendance-list/attendance-list.component').then(m => m.AttendanceListComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher'] }
      },
      {
        path: 'attendance/take/:id',
        loadComponent: () => import('./components/take-attendance/take-attendance.component').then(m => m.TakeAttendanceComponent),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Teacher'] }
      },
      {
        path: 'my-attendance',
        loadComponent: () => import('./components/student-attendance/student-attendance.component').then(m => m.StudentAttendanceComponent),
        canActivate: [authGuard],
        data: { roles: ['Student'] }
      },
      // TODO: Add more routes after creating components
      // {
      //   path: 'students',
      //   loadComponent: () => import('./components/students/students-list/students-list.component').then(m => m.StudentsListComponent),
      //   data: { roles: ['Admin', 'Teacher'] }
      // },
      // {
      //   path: 'teachers',
      //   loadComponent: () => import('./components/teachers/teachers-list/teachers-list.component').then(m => m.TeachersListComponent),
      //   data: { roles: ['Admin'] }
      // },
      // {
      //   path: 'grades',
      //   loadComponent: () => import('./components/grades/grades-list/grades-list.component').then(m => m.GradesListComponent),
      //   data: { roles: ['Admin', 'Teacher'] }
      // },
      // {
      //   path: 'classes',
      //   loadComponent: () => import('./components/classes/classes-list/classes-list.component').then(m => m.ClassesListComponent),
      //   data: { roles: ['Admin', 'Teacher'] }
      // },
      // {
      //   path: 'courses',
      //   loadComponent: () => import('./components/courses/courses-list/courses-list.component').then(m => m.CoursesListComponent),
      //   data: { roles: ['Admin'] }
      // },
      // {
      //   path: 'departments',
      //   loadComponent: () => import('./components/departments/departments-list/departments-list.component').then(m => m.DepartmentsListComponent),
      //   data: { roles: ['Admin'] }
      // },
    ]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
