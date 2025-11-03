-- Check teacher GV001 data
SELECT 'Teacher Info' as Section;
SELECT TeacherId, FullName, Username FROM Teachers WHERE TeacherId = 'GV001';

SELECT 'Classes for GV001' as Section;
SELECT ClassId, ClassName, TeacherId FROM Classes WHERE TeacherId = 'GV001';

SELECT 'Students in GV001 Classes' as Section;
SELECT s.StudentId, s.FullName, s.ClassId, c.ClassName
FROM Students s
INNER JOIN Classes c ON s.ClassId = c.ClassId
WHERE c.TeacherId = 'GV001';

SELECT 'All Classes with Teachers' as Section;
SELECT TOP 10 c.ClassId, c.ClassName, c.TeacherId, t.FullName as TeacherName
FROM Classes c
LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
ORDER BY c.ClassId;
