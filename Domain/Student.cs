namespace Domain;

public sealed class Student
{
    private readonly Dictionary<int, Evaluation> _evaluations = new();

    public int Id { get; }
    public string Name { get; private set; } = "";
    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations.Values;

    public Student(int id, string name)
    {
        if (id <= 0)
            throw new ArgumentException("Student ID must be positive.");

        Id = id;
        Rename(name);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be empty.");

        Name = name.Trim();
    }

    public Evaluation? FindEvaluation(int criterionId) =>
        _evaluations.GetValueOrDefault(criterionId);

    public void AssignEvaluation(Evaluation evaluation)
    {
        if (!_evaluations.TryAdd(evaluation.CriterionId, evaluation))
            throw new InvalidOperationException(
                $"An evaluation for criterion {evaluation.CriterionId} already exists. Use correction to change it.");
    }

    public void CorrectEvaluation(Evaluation evaluation)
    {
        if (!_evaluations.ContainsKey(evaluation.CriterionId))
            throw new InvalidOperationException($"No evaluation exists for criterion {evaluation.CriterionId}.");

        _evaluations[evaluation.CriterionId] = evaluation;
    }

    public void RemoveEvaluation(int criterionId)
    {
        if (!_evaluations.Remove(criterionId))
            throw new InvalidOperationException($"No evaluation exists for criterion {criterionId}.");
    }
}
