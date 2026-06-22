namespace GradeService.Domain;

public class Grade
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public string CourseCode { get; private set; } = string.Empty;
    public decimal Score { get; private set; }
    public string LetterGrade { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Grade() { }

    public static Grade Create(Guid orderId, Guid studentId, string courseCode, decimal score)
    {
        return new Grade
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            StudentId = studentId,
            CourseCode = courseCode,
            Score = score,
            LetterGrade = CalculateLetterGrade(score),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void UpdateScore(decimal newScore)
    {
        Score = newScore;
        LetterGrade = CalculateLetterGrade(newScore);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string CalculateLetterGrade(decimal score) => score switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B",
        >= 60 => "C",
        >= 50 => "D",
        _ => "F"
    };
}
