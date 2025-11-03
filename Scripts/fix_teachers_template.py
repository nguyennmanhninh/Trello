#!/usr/bin/env python
# -*- coding: utf-8 -*-

# Read students template
with open(r'C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\ClientApp\src\app\features\students\students.component.html', 'r', encoding='utf-8') as f:
    content = f.read()

# Apply all replacements
content = content.replace('students-container', 'teachers-container')
content = content.replace('students-table', 'teachers-table')
content = content.replace('Sinh viên', 'Giáo viên')
content = content.replace('sinh viên', 'giáo viên')
content = content.replace('student of students', 'teacher of teachers')
content = content.replace('.student.', '.teacher.')
content = content.replace('selectedStudents', 'selectedTeachers')
content = content.replace('createStudent()', "router.navigate(['/teachers/new'])")
content = content.replace('viewStudent(', 'viewTeacher(')
content = content.replace('editStudent(', 'editTeacher(')
content = content.replace('deleteStudent(', 'deleteTeacher(')
content = content.replace('toggleSelectStudent(', 'toggleSelectTeacher(')
content = content.replace('searchString', 'searchTerm')
content = content.replace('onSearchChange(searchTerm)', 'onSearchChange()')
content = content.replace("onSearchChange('')", 'onSearchChange()')
content = content.replace('selectedClassId', 'selectedDepartment')
content = content.replace('selectedDepartmentId', 'selectedDepartment')
content = content.replace('Lớp', 'Khoa')
content = content.replace('.className', '.departmentName')
content = content.replace('student.studentId', 'teacher.teacherId')
content = content.replace('student.', 'teacher.')
content = content.replace('students.', 'teachers.')
content = content.replace('students', 'teachers')

# Write to teachers template
with open(r'C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\ClientApp\src\app\features\teachers\teachers.component.html', 'w', encoding='utf-8') as f:
    f.write(content)

print("✅ Teachers template created successfully!")
