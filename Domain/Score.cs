namespace Domain;

public sealed record Score
{
    public decimal Value { get; }

    private Score(decimal value) => Value = value;

    internal static Score Create(decimal value, decimal maxScore)
    {
        if (value < 0 || value > maxScore)
            throw new ArgumentException($"Score must be between 0 and {maxScore}.");

        return new Score(value);
    }
}
