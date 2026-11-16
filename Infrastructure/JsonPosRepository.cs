using System.Text.Json;
using Application;
using Domain;

namespace Infrastructure;

public sealed class JsonPosRepository(string path, EvaluationFactory evaluationFactory) : IPosRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true
    };

    private readonly string _path = Path.GetFullPath(path);

    public PosState Read()
    {
        try
        {
            using var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var saved = JsonSerializer.Deserialize<SavedState>(stream, JsonOptions)
                ?? throw new JsonException("Expected a JSON object.");
            return Restore(saved);
        }
        catch (FileNotFoundException)
        {
            return new PosState();
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
        {
            throw new InvalidDataException($"Cannot load '{_path}': {exception.Message}", exception);
        }
    }

    public void Update(Action<PosState> change)
    {
        using var writeLock = File.Open(_path + ".lock", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var state = Read();
        change(state);
        Save(state);
    }

    private void Save(PosState state)
    {
        var students = state.Students.Select(student => new SavedStudent(student.Id, student.Name,
            student.Evaluations.Select(evaluation =>
                new SavedEvaluation(evaluation.CriterionId, evaluation.Score.Value)).ToArray())).ToArray();
        var criteria = state.Rubric.Criteria.Select(criterion => new SavedCriterion(
            criterion.Id, criterion.Name, criterion.MaxScore, criterion.Weight, criterion.IsLocked)).ToArray();

        var tempPath = _path + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            JsonSerializer.Serialize(stream, new SavedState(students, criteria), JsonOptions);
            stream.Flush(flushToDisk: true);
        }

        File.Move(tempPath, _path, overwrite: true);
    }

    private PosState Restore(SavedState saved)
    {
        var state = new PosState();
        foreach (var criterion in saved.Criteria)
        {
            state.Rubric.AddCriterion(criterion.Id, criterion.Name, criterion.MaxScore, criterion.Weight);
            if (criterion.IsLocked)
                state.Rubric.LockCriterion(criterion.Id);
        }

        var studentIds = new HashSet<int>();
        foreach (var savedStudent in saved.Students)
        {
            var student = new Student(savedStudent.Id, savedStudent.Name);
            if (!studentIds.Add(student.Id))
                throw new InvalidOperationException($"Student {student.Id} appears more than once.");

            foreach (var savedEvaluation in savedStudent.Evaluations)
            {
                var evaluation = evaluationFactory.Create(state.Rubric, savedEvaluation.CriterionId, savedEvaluation.Score);
                if (!state.Rubric.GetCriterion(evaluation.CriterionId).IsLocked)
                    throw new InvalidOperationException($"Criterion {evaluation.CriterionId} has an evaluation and must be locked.");

                student.AssignEvaluation(evaluation);
            }

            state.Students.Add(student);
        }

        return state;
    }

    private sealed record SavedState(SavedStudent[] Students, SavedCriterion[] Criteria);
    private readonly record struct SavedStudent(int Id, string Name, SavedEvaluation[] Evaluations);
    private readonly record struct SavedCriterion(int Id, string Name, decimal MaxScore, decimal Weight, bool IsLocked);
    private readonly record struct SavedEvaluation(int CriterionId, decimal Score);
}
