namespace Domain;

public sealed class EvaluationFactory
{
    public Evaluation Create(Rubric rubric, int criterionId, decimal value)
    {
        var criterion = rubric.GetCriterion(criterionId);
        return new Evaluation(criterion.Id, Score.Create(value, criterion.MaxScore));
    }
}
