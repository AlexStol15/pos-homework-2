namespace Domain;

public sealed class Rubric
{
    private readonly Dictionary<int, Criterion> _criteria = new();

    public IReadOnlyCollection<Criterion> Criteria => _criteria.Values;

    public void AddCriterion(int id, string name, decimal maxScore, decimal weight)
    {
        if (id <= 0)
            throw new ArgumentException("Criterion ID must be positive.");
        if (_criteria.ContainsKey(id))
            throw new InvalidOperationException($"Criterion {id} already exists.");

        _criteria.Add(id, new Criterion(id, name, maxScore, weight));
    }

    public Criterion GetCriterion(int id) =>
        _criteria.TryGetValue(id, out var criterion)
            ? criterion
            : throw new InvalidOperationException($"Criterion {id} was not found.");

    public void EditCriterion(int id, string name, decimal maxScore, decimal weight) =>
        GetCriterion(id).Edit(name, maxScore, weight);

    public void RemoveCriterion(int id)
    {
        GetCriterion(id).EnsureUnlocked();
        _criteria.Remove(id);
    }

    public void LockCriterion(int id) => GetCriterion(id).Lock();
}
