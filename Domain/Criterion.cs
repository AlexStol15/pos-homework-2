namespace Domain;

public sealed class Criterion
{
    public int Id { get; }
    public string Name { get; private set; } = "";
    public decimal MaxScore { get; private set; }
    public decimal Weight { get; private set; }
    public bool IsLocked { get; private set; }

    internal Criterion(int id, string name, decimal maxScore, decimal weight)
    {
        Id = id;
        Edit(name, maxScore, weight);
    }

    internal void Edit(string name, decimal maxScore, decimal weight)
    {
        EnsureUnlocked();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Criterion name cannot be empty.");
        if (maxScore <= 0)
            throw new ArgumentException("Maximum score must be positive.");
        if (weight <= 0)
            throw new ArgumentException("Weight must be positive.");

        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    internal void EnsureUnlocked()
    {
        if (IsLocked)
            throw new InvalidOperationException($"Criterion {Id} is locked.");
    }

    internal void Lock() => IsLocked = true;
}
