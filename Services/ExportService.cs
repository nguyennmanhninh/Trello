using ClosedXML.Excel;
using StudentManagementSystem.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;

namespace StudentManagementSystem.Services
{
    public interface IExportService
    {
        byte[] ExportStudentsToExcel(List<Student> students);
        byte[] ExportTeachersToExcel(List<Teacher> teachers);
        byte[] ExportGradesToExcel(List<Grade> grades);
        byte[] ExportClassReportToExcel(string classId, string className, List<Student> students, Dictionary<string, List<Grade>> studentGrades);
        
        // PDF Export Methods
        byte[] ExportStudentsToPdf(List<Student> students);
        byte[] ExportTeachersToPdf(List<Teacher> teachers);
        byte[] ExportGradesToPdf(List<Grade> grades);
        byte[] ExportClassReportToPdf(string classId, string className, List<Student> students, Dictionary<string, List<Grade>> studentGrades);
        
        // Advanced Reports
        byte[] ExportDepartmentReportToExcel(string departmentName, List<Class> classes, Dictionary<string, int> studentCounts);
        byte[] ExportDepartmentReportToPdf(string departmentName, List<Class> classes, Dictionary<string, int> studentCounts);
        byte[] ExportTeacherReportToExcel(string teacherName, List<Class> classes, List<Course> courses);
        byte[] ExportTeacherReportToPdf(string teacherName, List<Class> classes, List<Course> courses);
    }

    public class ExportService : IExportService
    {
        // Unicode Font for Vietnamese support in PDF
        private PdfFont GetVietnameseFont()
        {
            try
            {
                // Try to use Arial Unicode MS (if available)
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (File.Exists(fontPath))
                {
                    return PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                }
                
                // Fallback to Times New Roman (supports Vietnamese)
                fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
                if (File.Exists(fontPath))
                {
                    return PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                }
                
                // Last resort: use Helvetica (limited Vietnamese support)
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
            catch
            {
                // Fallback to standard font
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
        }

        public byte[] ExportStudentsToExcel(List<Student> students)
        {
        using var workbook = new XLWorkbook();
var worksheet = workbook.Worksheets.Add("Danh Sach Sinh Vien");

            // Headers
 worksheet.Cell(1, 1).Value = "Mã Sinh Viên";
            worksheet.Cell(1, 2).Value = "Họ và Tên";
         worksheet.Cell(1, 3).Value = "Ngày Sinh";
            worksheet.Cell(1, 4).Value = "Giới Tính";
         worksheet.Cell(1, 5).Value = "Số Điện Thoại";
            worksheet.Cell(1, 6).Value = "Địa Chỉ";
    worksheet.Cell(1, 7).Value = "Mã Lớp";

    // Style headers
var headerRow = worksheet.Range(1, 1, 1, 7);
    headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

// Data
            for (int i = 0; i < students.Count; i++)
            {
              var student = students[i];
       worksheet.Cell(i + 2, 1).Value = student.StudentId;
 worksheet.Cell(i + 2, 2).Value = student.FullName;
                worksheet.Cell(i + 2, 3).Value = student.DateOfBirth.ToString("dd/MM/yyyy");
      worksheet.Cell(i + 2, 4).Value = student.Gender ? "Nam" : "Nữ";
  worksheet.Cell(i + 2, 5).Value = student.Phone;
    worksheet.Cell(i + 2, 6).Value = student.Address;
                worksheet.Cell(i + 2, 7).Value = student.ClassId;
     }

            worksheet.Columns().AdjustToContents();

         using var stream = new MemoryStream();
            workbook.SaveAs(stream);
return stream.ToArray();
        }

        public byte[] ExportTeachersToExcel(List<Teacher> teachers)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh Sach Giao Vien");

            // Headers
            worksheet.Cell(1, 1).Value = "Mã Giáo Viên";
            worksheet.Cell(1, 2).Value = "Họ và Tên";
            worksheet.Cell(1, 3).Value = "Ngày Sinh";
            worksheet.Cell(1, 4).Value = "Giới Tính";
            worksheet.Cell(1, 5).Value = "Số Điện Thoại";
            worksheet.Cell(1, 6).Value = "Địa Chỉ";
            worksheet.Cell(1, 7).Value = "Khoa";

            // Style headers
            var headerRow = worksheet.Range(1, 1, 1, 7);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Data
            for (int i = 0; i < teachers.Count; i++)
            {
                var teacher = teachers[i];
                worksheet.Cell(i + 2, 1).Value = teacher.TeacherId;
                worksheet.Cell(i + 2, 2).Value = teacher.FullName;
                worksheet.Cell(i + 2, 3).Value = teacher.DateOfBirth.ToString("dd/MM/yyyy");
                worksheet.Cell(i + 2, 4).Value = teacher.Gender ? "Nam" : "Nữ";
                worksheet.Cell(i + 2, 5).Value = teacher.Phone ?? "";
                worksheet.Cell(i + 2, 6).Value = teacher.Address ?? "";
                worksheet.Cell(i + 2, 7).Value = teacher.Department?.DepartmentName ?? "";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

public byte[] ExportGradesToExcel(List<Grade> grades)
        {
          using var workbook = new XLWorkbook();
var worksheet = workbook.Worksheets.Add("Bang Diem");

   // Headers
            worksheet.Cell(1, 1).Value = "Mã Sinh Viên";
     worksheet.Cell(1, 2).Value = "Mã Môn Học";
      worksheet.Cell(1, 3).Value = "Điểm";
        worksheet.Cell(1, 4).Value = "Xếp Loại";

     // Style headers
            var headerRow = worksheet.Range(1, 1, 1, 4);
        headerRow.Style.Font.Bold = true;
      headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

    // Data
   for (int i = 0; i < grades.Count; i++)
            {
   var grade = grades[i];
           worksheet.Cell(i + 2, 1).Value = grade.StudentId;
    worksheet.Cell(i + 2, 2).Value = grade.CourseId;
 worksheet.Cell(i + 2, 3).Value = grade.Score;
        worksheet.Cell(i + 2, 4).Value = grade.Classification;
        }

     worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
       workbook.SaveAs(stream);
return stream.ToArray();
        }

        public byte[] ExportClassReportToExcel(string classId, string className, List<Student> students, Dictionary<string, List<Grade>> studentGrades)
        {
            using var workbook = new XLWorkbook();
         var worksheet = workbook.Worksheets.Add($"Lop {className}");

       // Title
            worksheet.Cell(1, 1).Value = $"BAO CAO LOP: {className}";
  worksheet.Range(1, 1, 1, 5).Merge();
     worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
     worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
          int currentRow = 3;
            worksheet.Cell(currentRow, 1).Value = "Mã SV";
        worksheet.Cell(currentRow, 2).Value = "Họ và Tên";
  worksheet.Cell(currentRow, 3).Value = "Số Môn";
      worksheet.Cell(currentRow, 4).Value = "Điểm TB";
worksheet.Cell(currentRow, 5).Value = "Xếp Loại";

          var headerRow = worksheet.Range(currentRow, 1, currentRow, 5);
     headerRow.Style.Font.Bold = true;
    headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

  // Data
    currentRow++;
        foreach (var student in students)
      {
      worksheet.Cell(currentRow, 1).Value = student.StudentId;
      worksheet.Cell(currentRow, 2).Value = student.FullName;

    if (studentGrades.ContainsKey(student.StudentId) && studentGrades[student.StudentId].Any())
         {
        var grades = studentGrades[student.StudentId];
          worksheet.Cell(currentRow, 3).Value = grades.Count;
   var avgScore = grades.Average(g => g.Score);
    worksheet.Cell(currentRow, 4).Value = Math.Round(avgScore, 2);
           worksheet.Cell(currentRow, 5).Value = GetClassification(avgScore);
            }
   else
        {
            worksheet.Cell(currentRow, 3).Value = 0;
     worksheet.Cell(currentRow, 4).Value = "N/A";
           worksheet.Cell(currentRow, 5).Value = "Chưa có điểm";
   }

     currentRow++;
        }

        worksheet.Columns().AdjustToContents();

          using var stream = new MemoryStream();
            workbook.SaveAs(stream);
       return stream.ToArray();
        }

        private string GetClassification(decimal score)
        {
            if (score >= 9.0m) return "Xuất sắc";
            if (score >= 8.0m) return "Giỏi";
            if (score >= 7.0m) return "Khá";
            if (score >= 5.5m) return "Trung bình";
            if (score >= 4.0m) return "Yếu";
            return "Kém";
        }

        // ==================== PDF EXPORT METHODS ====================

        public byte[] ExportStudentsToPdf(List<Student> students)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph("DANH SÁCH SINH VIÊN")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 25, 12, 8, 12, 23, 10 }))
                .UseAllAvailableWidth();

            // Headers
            table.AddHeaderCell(CreateHeaderCell("Mã SV"));
            table.AddHeaderCell(CreateHeaderCell("Họ và Tên"));
            table.AddHeaderCell(CreateHeaderCell("Ngày Sinh"));
            table.AddHeaderCell(CreateHeaderCell("Giới Tính"));
            table.AddHeaderCell(CreateHeaderCell("SĐT"));
            table.AddHeaderCell(CreateHeaderCell("Địa Chỉ"));
            table.AddHeaderCell(CreateHeaderCell("Mã Lớp"));

            // Data rows
            foreach (var student in students)
            {
                table.AddCell(CreateCell(student.StudentId));
                table.AddCell(CreateCell(student.FullName));
                table.AddCell(CreateCell(student.DateOfBirth.ToString("dd/MM/yyyy")));
                table.AddCell(CreateCell(student.Gender ? "Nam" : "Nữ"));
                table.AddCell(CreateCell(student.Phone ?? ""));
                table.AddCell(CreateCell(student.Address ?? ""));
                table.AddCell(CreateCell(student.ClassId.ToString()));
            }

            document.Add(table);

            // Footer
            document.Add(new Paragraph($"\nTổng số: {students.Count} sinh viên")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return stream.ToArray();
        }

        public byte[] ExportTeachersToPdf(List<Teacher> teachers)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph("DANH SÁCH GIÁO VIÊN")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 25, 12, 8, 12, 20, 13 }))
                .UseAllAvailableWidth();

            // Headers
            table.AddHeaderCell(CreateHeaderCell("Mã GV"));
            table.AddHeaderCell(CreateHeaderCell("Họ và Tên"));
            table.AddHeaderCell(CreateHeaderCell("Ngày Sinh"));
            table.AddHeaderCell(CreateHeaderCell("Giới Tính"));
            table.AddHeaderCell(CreateHeaderCell("SĐT"));
            table.AddHeaderCell(CreateHeaderCell("Địa Chỉ"));
            table.AddHeaderCell(CreateHeaderCell("Khoa"));

            // Data rows
            foreach (var teacher in teachers)
            {
                table.AddCell(CreateCell(teacher.TeacherId));
                table.AddCell(CreateCell(teacher.FullName));
                table.AddCell(CreateCell(teacher.DateOfBirth.ToString("dd/MM/yyyy")));
                table.AddCell(CreateCell(teacher.Gender ? "Nam" : "Nữ"));
                table.AddCell(CreateCell(teacher.Phone ?? ""));
                table.AddCell(CreateCell(teacher.Address ?? ""));
                table.AddCell(CreateCell(teacher.Department?.DepartmentName ?? ""));
            }

            document.Add(table);

            // Footer
            document.Add(new Paragraph($"\nTổng số: {teachers.Count} giáo viên")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return stream.ToArray();
        }

        public byte[] ExportGradesToPdf(List<Grade> grades)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph("BẢNG ĐIỂM")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 35, 15, 30 }))
                .UseAllAvailableWidth();

            // Headers
            table.AddHeaderCell(CreateHeaderCell("Mã Sinh Viên"));
            table.AddHeaderCell(CreateHeaderCell("Môn Học"));
            table.AddHeaderCell(CreateHeaderCell("Điểm"));
            table.AddHeaderCell(CreateHeaderCell("Xếp Loại"));

            // Data rows
            foreach (var grade in grades)
            {
                table.AddCell(CreateCell(grade.StudentId));
                table.AddCell(CreateCell(grade.Course?.CourseName ?? grade.CourseId.ToString()));
                table.AddCell(CreateCell(grade.Score.ToString("0.0m")));
                table.AddCell(CreateCell(grade.Classification ?? ""));
            }

            document.Add(table);

            // Statistics
            if (grades.Any())
            {
                var avgScore = grades.Average(g => g.Score);
                document.Add(new Paragraph($"\nĐiểm trung bình: {avgScore:0.00m}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10)
                    .SetBold());
            }

            document.Add(new Paragraph($"Tổng số: {grades.Count} điểm")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return stream.ToArray();
        }

        public byte[] ExportClassReportToPdf(string classId, string className, List<Student> students, Dictionary<string, List<Grade>> studentGrades)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph($"BÁO CÁO LỚP: {className}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph($"Mã lớp: {classId}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12));

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 30, 15, 20, 20 }))
                .UseAllAvailableWidth();

            // Headers
            table.AddHeaderCell(CreateHeaderCell("Mã SV"));
            table.AddHeaderCell(CreateHeaderCell("Họ và Tên"));
            table.AddHeaderCell(CreateHeaderCell("Số Môn"));
            table.AddHeaderCell(CreateHeaderCell("Điểm TB"));
            table.AddHeaderCell(CreateHeaderCell("Xếp Loại"));

            // Data rows
            foreach (var student in students)
            {
                table.AddCell(CreateCell(student.StudentId));
                table.AddCell(CreateCell(student.FullName));

                if (studentGrades.ContainsKey(student.StudentId) && studentGrades[student.StudentId].Any())
                {
                    var grades = studentGrades[student.StudentId];
                    table.AddCell(CreateCell(grades.Count.ToString()));
                    var avgScore = grades.Average(g => g.Score);
                    table.AddCell(CreateCell(Math.Round(avgScore, 2).ToString("0.00m")));
                    table.AddCell(CreateCell(GetClassification(avgScore)));
                }
                else
                {
                    table.AddCell(CreateCell("0"));
                    table.AddCell(CreateCell("N/A"));
                    table.AddCell(CreateCell("Chưa có điểm"));
                }
            }

            document.Add(table);

            // Summary
            document.Add(new Paragraph($"\nTổng số sinh viên: {students.Count}")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(10)
                .SetBold());

            document.Close();
            return stream.ToArray();
        }

        // ==================== ADVANCED EXCEL REPORTS ====================

        public byte[] ExportDepartmentReportToExcel(string departmentName, List<Class> classes, Dictionary<string, int> studentCounts)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Khoa {departmentName}");

            // Title
            worksheet.Cell(1, 1).Value = $"BÁO CÁO KHOA: {departmentName}";
            worksheet.Range(1, 1, 1, 4).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            int currentRow = 3;
            worksheet.Cell(currentRow, 1).Value = "Mã Lớp";
            worksheet.Cell(currentRow, 2).Value = "Tên Lớp";
            worksheet.Cell(currentRow, 3).Value = "Giáo Viên";
            worksheet.Cell(currentRow, 4).Value = "Số Sinh Viên";

            var headerRow = worksheet.Range(currentRow, 1, currentRow, 4);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Data
            currentRow++;
            int totalStudents = 0;
            foreach (var cls in classes)
            {
                worksheet.Cell(currentRow, 1).Value = cls.ClassId;
                worksheet.Cell(currentRow, 2).Value = cls.ClassName;
                worksheet.Cell(currentRow, 3).Value = cls.Teacher?.FullName ?? "N/A";
                
                int count = studentCounts.ContainsKey(cls.ClassId.ToString()) ? studentCounts[cls.ClassId.ToString()] : 0;
                worksheet.Cell(currentRow, 4).Value = count;
                totalStudents += count;
                
                currentRow++;
            }

            // Total row
            currentRow++;
            worksheet.Cell(currentRow, 3).Value = "Tổng cộng:";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 4).Value = totalStudents;
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportDepartmentReportToPdf(string departmentName, List<Class> classes, Dictionary<string, int> studentCounts)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph($"BÁO CÁO KHOA: {departmentName}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 30, 30, 25 }))
                .UseAllAvailableWidth();

            // Headers
            table.AddHeaderCell(CreateHeaderCell("Mã Lớp"));
            table.AddHeaderCell(CreateHeaderCell("Tên Lớp"));
            table.AddHeaderCell(CreateHeaderCell("Giáo Viên"));
            table.AddHeaderCell(CreateHeaderCell("Số Sinh Viên"));

            // Data rows
            int totalStudents = 0;
            foreach (var cls in classes)
            {
                table.AddCell(CreateCell(cls.ClassId.ToString()));
                table.AddCell(CreateCell(cls.ClassName ?? ""));
                table.AddCell(CreateCell(cls.Teacher?.FullName ?? "N/A"));
                
                int count = studentCounts.ContainsKey(cls.ClassId.ToString()) ? studentCounts[cls.ClassId.ToString()] : 0;
                table.AddCell(CreateCell(count.ToString()));
                totalStudents += count;
            }

            // Total row
            var totalCell1 = CreateCell("").SetBold();
            var totalCell2 = CreateCell("").SetBold();
            var totalCell3 = CreateCell("Tổng cộng:").SetBold().SetTextAlignment(TextAlignment.RIGHT);
            var totalCell4 = CreateCell(totalStudents.ToString()).SetBold();
            
            table.AddCell(totalCell1);
            table.AddCell(totalCell2);
            table.AddCell(totalCell3);
            table.AddCell(totalCell4);

            document.Add(table);

            document.Close();
            return stream.ToArray();
        }

        public byte[] ExportTeacherReportToExcel(string teacherName, List<Class> classes, List<Course> courses)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"GV {teacherName}");

            // Title
            worksheet.Cell(1, 1).Value = $"BÁO CÁO GIÁO VIÊN: {teacherName}";
            worksheet.Range(1, 1, 1, 3).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Classes section
            int currentRow = 3;
            worksheet.Cell(currentRow, 1).Value = "CÁC LỚP ĐANG CHỦ NHIỆM";
            worksheet.Range(currentRow, 1, currentRow, 3).Merge();
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Mã Lớp";
            worksheet.Cell(currentRow, 2).Value = "Tên Lớp";
            worksheet.Cell(currentRow, 3).Value = "Khoa";

            var headerRow1 = worksheet.Range(currentRow, 1, currentRow, 3);
            headerRow1.Style.Font.Bold = true;
            headerRow1.Style.Fill.BackgroundColor = XLColor.LightBlue;

            currentRow++;
            foreach (var cls in classes)
            {
                worksheet.Cell(currentRow, 1).Value = cls.ClassId;
                worksheet.Cell(currentRow, 2).Value = cls.ClassName;
                worksheet.Cell(currentRow, 3).Value = cls.Department?.DepartmentName ?? "N/A";
                currentRow++;
            }

            // Courses section
            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "CÁC MÔN HỌC ĐANG GIẢNG DẠY";
            worksheet.Range(currentRow, 1, currentRow, 3).Merge();
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Mã Môn";
            worksheet.Cell(currentRow, 2).Value = "Tên Môn Học";
            worksheet.Cell(currentRow, 3).Value = "Số Tín Chỉ";

            var headerRow2 = worksheet.Range(currentRow, 1, currentRow, 3);
            headerRow2.Style.Font.Bold = true;
            headerRow2.Style.Fill.BackgroundColor = XLColor.LightBlue;

            currentRow++;
            foreach (var course in courses)
            {
                worksheet.Cell(currentRow, 1).Value = course.CourseId;
                worksheet.Cell(currentRow, 2).Value = course.CourseName;
                worksheet.Cell(currentRow, 3).Value = course.Credits;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportTeacherReportToPdf(string teacherName, List<Class> classes, List<Course> courses)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Set Vietnamese font for entire document
            var font = GetVietnameseFont();
            document.SetFont(font);

            // Title
            var title = new Paragraph($"BÁO CÁO GIÁO VIÊN: {teacherName}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold();
            document.Add(title);

            document.Add(new Paragraph("\n"));

            // Classes section
            var classesTitle = new Paragraph("CÁC LỚP ĐANG CHỦ NHIỆM")
                .SetFontSize(14)
                .SetBold();
            document.Add(classesTitle);

            var classTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 40, 40 }))
                .UseAllAvailableWidth();

            classTable.AddHeaderCell(CreateHeaderCell("Mã Lớp"));
            classTable.AddHeaderCell(CreateHeaderCell("Tên Lớp"));
            classTable.AddHeaderCell(CreateHeaderCell("Khoa"));

            foreach (var cls in classes)
            {
                classTable.AddCell(CreateCell(cls.ClassId.ToString()));
                classTable.AddCell(CreateCell(cls.ClassName ?? ""));
                classTable.AddCell(CreateCell(cls.Department?.DepartmentName ?? "N/A"));
            }

            document.Add(classTable);

            document.Add(new Paragraph("\n"));

            // Courses section
            var coursesTitle = new Paragraph("CÁC MÔN HỌC ĐANG GIẢNG DẠY")
                .SetFontSize(14)
                .SetBold();
            document.Add(coursesTitle);

            var courseTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 60, 20 }))
                .UseAllAvailableWidth();

            courseTable.AddHeaderCell(CreateHeaderCell("Mã Môn"));
            courseTable.AddHeaderCell(CreateHeaderCell("Tên Môn Học"));
            courseTable.AddHeaderCell(CreateHeaderCell("Số Tín Chỉ"));

            foreach (var course in courses)
            {
                courseTable.AddCell(CreateCell(course.CourseId.ToString()));
                courseTable.AddCell(CreateCell(course.CourseName ?? ""));
                courseTable.AddCell(CreateCell(course.Credits.ToString()));
            }

            document.Add(courseTable);

            document.Close();
            return stream.ToArray();
        }

        // ==================== PDF HELPER METHODS ====================

        private Cell CreateHeaderCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(10);
        }

        private Cell CreateCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text))
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.LEFT);
        }
    }
}
