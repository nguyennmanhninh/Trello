import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Student, Teacher } from '../models/models';

export interface ProfileResponse {
  role: string;
  data: Student | Teacher | AdminProfile;
}

export interface AdminProfile {
  userId: number;
  username: string;
  role: string;
  entityId: string;
}

export interface UpdateAdminProfileRequest {
  username: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly apiUrl = 'http://localhost:5298/api/profile';  // ✅ Use absolute URL

  constructor(private http: HttpClient) {}

  // Get current user profile
  getProfile(): Observable<ProfileResponse> {
    console.log('📡 ProfileService - Calling getProfile with credentials');
    return this.http.get<ProfileResponse>(this.apiUrl, {
      withCredentials: true  // ✅ Ensure session cookie is sent
    });
  }

  // Update student profile
  updateStudentProfile(student: Student): Observable<any> {
    return this.http.put(`${this.apiUrl}/student`, student);
  }

  // Update teacher profile
  updateTeacherProfile(teacher: Teacher): Observable<any> {
    return this.http.put(`${this.apiUrl}/teacher`, teacher);
  }

  // Update admin profile
  updateAdminProfile(request: UpdateAdminProfileRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/admin`, request);
  }

  // Change password
  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/password`, request);
  }
}
