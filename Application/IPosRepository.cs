using Domain;

namespace Application;

public interface IPosRepository
{
    PosState Read();
    void Update(Action<PosState> change);
}

public sealed class PosState
{
    public List<Student> Students { get; } = new();
    public Rubric Rubric { get; } = new();
}
