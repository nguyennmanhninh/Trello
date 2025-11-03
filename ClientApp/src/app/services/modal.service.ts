import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  // Observable streams for modal triggers
  private openStudentModalSource = new Subject<void>();
  private openTeacherModalSource = new Subject<void>();
  private openClassModalSource = new Subject<void>();

  // Observable that components can subscribe to
  openStudentModal$ = this.openStudentModalSource.asObservable();
  openTeacherModal$ = this.openTeacherModalSource.asObservable();
  openClassModal$ = this.openClassModalSource.asObservable();

  // Methods to trigger modals
  triggerStudentModal(): void {
    console.log('🔔 ModalService: triggerStudentModal() called - emitting event');
    this.openStudentModalSource.next();
    console.log('✅ ModalService: Student modal event emitted');
  }

  triggerTeacherModal(): void {
    console.log('🔔 ModalService: triggerTeacherModal() called - emitting event');
    this.openTeacherModalSource.next();
    console.log('✅ ModalService: Teacher modal event emitted');
  }

  triggerClassModal(): void {
    console.log('🔔 ModalService: triggerClassModal() called - emitting event');
    this.openClassModalSource.next();
    console.log('✅ ModalService: Class modal event emitted');
  }
}
