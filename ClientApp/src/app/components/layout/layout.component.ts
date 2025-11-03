import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { filter } from 'rxjs/operators';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  roles: string[];
  badge?: string;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent implements OnInit {
  isSidebarOpen: boolean = true;
  isMobileMenuOpen: boolean = false;
  currentRoute: string = '';
  userRole: string | null = null;
  userName: string = '';
  userEmail: string = '';

  menuItems: MenuItem[] = [
    { label: 'Dashboard Admin', icon: '📊', route: '/dashboard-admin', roles: ['Admin'] },
    { label: 'Dashboard Giảng viên', icon: '📊', route: '/dashboard-teacher', roles: ['Teacher'] },
    { label: 'Dashboard Sinh viên', icon: '📊', route: '/dashboard-student', roles: ['Student'] },
    { label: 'Sinh viên', icon: '👨‍🎓', route: '/students', roles: ['Admin', 'Teacher'] },
    { label: 'Giảng viên', icon: '👨‍🏫', route: '/teachers', roles: ['Admin'] },
    { label: 'Lớp học', icon: '🏫', route: '/classes', roles: ['Admin', 'Teacher'] },
    { label: 'Môn học', icon: '📚', route: '/courses', roles: ['Admin', 'Teacher', 'Student'] }, // Student can view courses (read-only)
    { label: 'Điểm', icon: '📝', route: '/grades', roles: ['Admin', 'Teacher', 'Student'] }, // Student can view their own grades
    { label: 'Điểm danh', icon: '✓', route: '/attendance', roles: ['Admin', 'Teacher'] }, // Teacher manages attendance
    { label: 'Điểm danh của tôi', icon: '📋', route: '/my-attendance', roles: ['Student'] }, // Student views own attendance
    { label: 'Khoa', icon: '🏢', route: '/departments', roles: ['Admin'] },
    { label: 'Thông tin cá nhân', icon: '👤', route: '/profile', roles: ['Admin', 'Teacher', 'Student'] } // Profile for all roles
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.userRole = this.authService.userRole;
    
    // ✅ Subscribe to currentUser changes to auto-update when profile changes
    this.authService.currentUser.subscribe(user => {
      this.userName = user?.fullName || 'User';
      this.userEmail = user?.username || '';
      console.log('🔄 LayoutComponent - User data updated:', { userName: this.userName, userEmail: this.userEmail });
    });

    // Track current route
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.currentRoute = event.url;
      });
  }

  ngOnInit(): void {
    this.currentRoute = this.router.url;
    // Close mobile menu on medium+ screens
    this.checkScreenSize();
    window.addEventListener('resize', () => this.checkScreenSize());
  }

  checkScreenSize(): void {
    if (window.innerWidth >= 768) {
      this.isMobileMenuOpen = false;
      this.isSidebarOpen = true;
    } else {
      this.isSidebarOpen = false;
    }
  }

  toggleSidebar(): void {
    if (window.innerWidth < 768) {
      this.isMobileMenuOpen = !this.isMobileMenuOpen;
    } else {
      this.isSidebarOpen = !this.isSidebarOpen;
    }
  }

  closeMobileMenu(): void {
    if (window.innerWidth < 768) {
      this.isMobileMenuOpen = false;
    }
  }

  hasAccess(roles: string[]): boolean {
    return this.userRole ? roles.includes(this.userRole) : false;
  }

  isActive(route: string): boolean {
    return this.currentRoute === route || this.currentRoute.startsWith(route + '/');
  }

  logout(): void {
    this.authService.logout();
  }

  getRoleBadgeClass(): string {
    switch (this.userRole) {
      case 'Admin': return 'badge-admin';
      case 'Teacher': return 'badge-teacher';
      case 'Student': return 'badge-student';
      default: return '';
    }
  }

  ngOnDestroy(): void {
    window.removeEventListener('resize', () => this.checkScreenSize());
  }
}
