using Domain;
using Xunit;

namespace Tests;

public sealed class DomainTests
{
    private readonly EvaluationFactory _factory = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void FactoryRejectsScoresOutsideCriterionRange(int score)
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Exam", 20, 1);

        Assert.Throws<ArgumentException>(() => _factory.Create(rubric, 1, score));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    public void FactoryAcceptsBothScoreLimits(int score)
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Exam", 20, 1);

        var evaluation = _factory.Create(rubric, 1, score);

        Assert.Equal((decimal)score, evaluation.Score.Value);
    }

    [Fact]
    public void FactoryRejectsMissingCriterion()
    {
        Assert.Throws<InvalidOperationException>(() => _factory.Create(new Rubric(), 1, 0));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void RubricRejectsNonPositiveMaximumOrWeight(int maxScore, int weight)
    {
        var rubric = new Rubric();

        Assert.Throws<ArgumentException>(() => rubric.AddCriterion(1, "Exam", maxScore, weight));
        Assert.Empty(rubric.Criteria);
    }

    [Fact]
    public void StudentRejectsDuplicateEvaluation()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Exam", 20, 1);
        var student = new Student(1, "Anna");
        student.AssignEvaluation(_factory.Create(rubric, 1, 10));

        Assert.Throws<InvalidOperationException>(() => student.AssignEvaluation(_factory.Create(rubric, 1, 7)));
        Assert.Equal(10m, Assert.Single(student.Evaluations).Score.Value);
    }

    [Fact]
    public void CorrectionReplacesCurrentScore()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Exam", 20, 1);
        var student = new Student(1, "Anna");
        student.AssignEvaluation(_factory.Create(rubric, 1, 10));

        student.CorrectEvaluation(_factory.Create(rubric, 1, 17.5m));

        Assert.Equal(17.5m, Assert.Single(student.Evaluations).Score.Value);
    }

    [Fact]
    public void CalculatorUsesRelativeWeightsAndEachCriteriaMaximum()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Essay", 20, 1);
        rubric.AddCriterion(2, "Exam", 40, 3);
        var student = new Student(1, "Anna");
        student.AssignEvaluation(_factory.Create(rubric, 1, 10));
        student.AssignEvaluation(_factory.Create(rubric, 2, 30));

        var calculator = new ScoreCalculator();

        Assert.Equal(6.875m, calculator.Calculate(student, rubric, new StrictFinalStrategy()));
    }

    [Fact]
    public void ProvisionalResultUsesOnlyEvaluatedCriteria()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(1, "Essay", 20, 1);
        rubric.AddCriterion(2, "Exam", 40, 3);
        var student = new Student(1, "Anna");
        student.AssignEvaluation(_factory.Create(rubric, 1, 13.5m));
        var calculator = new ScoreCalculator();

        Assert.Null(calculator.Calculate(student, rubric, new StrictFinalStrategy()));
        Assert.Equal(6.75m, calculator.Calculate(student, rubric, new ProvisionalStrategy()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NoEvaluationsGiveNoResult(bool hasCriterion)
    {
        var rubric = new Rubric();
        if (hasCriterion)
            rubric.AddCriterion(1, "Exam", 20, 1);
        var student = new Student(1, "Anna");
        var calculator = new ScoreCalculator();

        Assert.Null(calculator.Calculate(student, rubric, new StrictFinalStrategy()));
        Assert.Null(calculator.Calculate(student, rubric, new ProvisionalStrategy()));
    }
}
