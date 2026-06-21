namespace OrderService.Domain.Entities;

public class GradeRecord
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string StudentId { get; private set; } = string.Empty;
    public string CourseId { get; private set; } = string.Empty;
    public string CourseName { get; private set; } = string.Empty;
    public decimal Score { get; private set; }
    public string Grade { get; private set; } = string.Empty;
    public DateTime RecordedAt { get; private set; } = DateTime.UtcNow;
    public string RecordedBy { get; private set; } = string.Empty;

    private GradeRecord() { }

    public static GradeRecord Create(string studentId, string courseId, string courseName, decimal score, string recordedBy)
    {
        var grade = CalculateGrade(score);
        return new GradeRecord
        {
            StudentId = studentId,
            CourseId = courseId,
            CourseName = courseName,
            Score = score,
            Grade = grade,
            RecordedBy = recordedBy
        };
    }

    private static string CalculateGrade(decimal score) => score switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B",
        >= 60 => "C",
        >= 50 => "D",
        _ => "F"
    };
}
