namespace Domain;

public sealed class Evaluation
{
    public int CriterionId { get; }
    public Score Score { get; }

    internal Evaluation(int criterionId, Score score)
    {
        CriterionId = criterionId;
        Score = score;
    }
}
