using FluentAssertions;
using GradeService.Domain;

namespace GradeService.UnitTests;

public class GradeEntityTests
{
    [Theory]
    [InlineData(95, "A+")]
    [InlineData(85, "A")]
    [InlineData(75, "B")]
    [InlineData(65, "C")]
    [InlineData(55, "D")]
    [InlineData(40, "F")]
    public void Create_ShouldAssignCorrectLetterGrade(decimal score, string expectedLetter)
    {
        var grade = Grade.Create(Guid.NewGuid(), Guid.NewGuid(), "CS101", score);

        grade.LetterGrade.Should().Be(expectedLetter);
        grade.Score.Should().Be(score);
        grade.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var orderId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        const string courseCode = "MATH201";
        const decimal score = 88m;

        var grade = Grade.Create(orderId, studentId, courseCode, score);

        grade.OrderId.Should().Be(orderId);
        grade.StudentId.Should().Be(studentId);
        grade.CourseCode.Should().Be(courseCode);
        grade.Score.Should().Be(score);
        grade.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateScore_ShouldChangeScoreAndLetterGrade()
    {
        var grade = Grade.Create(Guid.NewGuid(), Guid.NewGuid(), "CS101", 55m);
        grade.LetterGrade.Should().Be("D");

        grade.UpdateScore(92m);

        grade.Score.Should().Be(92m);
        grade.LetterGrade.Should().Be("A+");
        grade.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateScore_BelowPass_ShouldBeF()
    {
        var grade = Grade.Create(Guid.NewGuid(), Guid.NewGuid(), "CS101", 80m);

        grade.UpdateScore(30m);

        grade.LetterGrade.Should().Be("F");
    }
}
