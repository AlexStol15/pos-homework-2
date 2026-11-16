namespace Domain;

public interface IScoreStrategy
{
    decimal? Calculate(Student student, Rubric rubric);
}

public sealed class StrictFinalStrategy : IScoreStrategy
{
    public decimal? Calculate(Student student, Rubric rubric)
    {
        if (rubric.Criteria.Any(criterion => student.FindEvaluation(criterion.Id) is null))
            return null;

        return ScoreCalculator.WeightedResult(student, rubric);
    }
}

public sealed class ProvisionalStrategy : IScoreStrategy
{
    public decimal? Calculate(Student student, Rubric rubric) =>
        ScoreCalculator.WeightedResult(student, rubric);
}

public sealed class ScoreCalculator
{
    public decimal? Calculate(Student student, Rubric rubric, IScoreStrategy strategy) =>
        strategy.Calculate(student, rubric);

    internal static decimal? WeightedResult(Student student, Rubric rubric)
    {
        var graded = rubric.Criteria
            .Select(criterion => (Criterion: criterion, Evaluation: student.FindEvaluation(criterion.Id)))
            .Where(entry => entry.Evaluation is not null)
            .ToArray();

        if (graded.Length == 0)
            return null;

        var largestWeight = graded.Max(entry => entry.Criterion.Weight);
        decimal totalWeight = 0;
        decimal weightedScore = 0;

        foreach (var (criterion, evaluation) in graded)
        {
            var weight = criterion.Weight / largestWeight;
            totalWeight += weight;
            weightedScore += weight * (evaluation!.Score.Value / criterion.MaxScore);
        }

        return 10 * (weightedScore / totalWeight);
    }
}
