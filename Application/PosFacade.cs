using Domain;

namespace Application;

public sealed class PosFacade(
    IPosRepository repository,
    IResultsExporter exporter,
    EvaluationFactory evaluationFactory,
    ScoreCalculator calculator,
    StrictFinalStrategy strictFinal,
    ProvisionalStrategy provisional)
{
    public void AddStudent(string name) => repository.Update(state =>
    {
        var id = state.Students.Select(student => student.Id).DefaultIfEmpty().Max() + 1;
        state.Students.Add(new Student(id, name));
    });

    public IReadOnlyList<Student> ListStudents() => repository.Read().Students.AsReadOnly();

    public void EditStudent(int studentId, string name) =>
        repository.Update(state => GetStudent(state, studentId).Rename(name));

    public void DeleteStudent(int studentId) =>
        repository.Update(state => state.Students.Remove(GetStudent(state, studentId)));

    public void AddCriterion(string name, decimal maxScore, decimal weight) => repository.Update(state =>
    {
        var id = state.Rubric.Criteria.Select(criterion => criterion.Id).DefaultIfEmpty().Max() + 1;
        state.Rubric.AddCriterion(id, name, maxScore, weight);
    });

    public IReadOnlyCollection<Criterion> ListCriteria() => repository.Read().Rubric.Criteria;

    public void EditCriterion(int criterionId, string name, decimal maxScore, decimal weight) =>
        repository.Update(state => state.Rubric.EditCriterion(criterionId, name, maxScore, weight));

    public void DeleteCriterion(int criterionId) =>
        repository.Update(state => state.Rubric.RemoveCriterion(criterionId));

    public void AssignEvaluation(int studentId, int criterionId, decimal score) => repository.Update(state =>
    {
        var student = GetStudent(state, studentId);
        var evaluation = evaluationFactory.Create(state.Rubric, criterionId, score);
        student.AssignEvaluation(evaluation);
        state.Rubric.LockCriterion(criterionId);
    });

    public IReadOnlyList<StudentEvaluation> ListEvaluations(int studentId)
    {
        var state = repository.Read();
        return GetStudent(state, studentId).Evaluations.Select(evaluation =>
        {
            var criterion = state.Rubric.GetCriterion(evaluation.CriterionId);
            return new StudentEvaluation(criterion.Id, criterion.Name, evaluation.Score.Value, criterion.MaxScore);
        }).ToArray();
    }

    public void CorrectEvaluation(int studentId, int criterionId, decimal score) => repository.Update(state =>
    {
        var student = GetStudent(state, studentId);
        student.CorrectEvaluation(evaluationFactory.Create(state.Rubric, criterionId, score));
    });

    public void RemoveEvaluation(int studentId, int criterionId) =>
        repository.Update(state => GetStudent(state, studentId).RemoveEvaluation(criterionId));

    public StudentResult CalculateResult(int studentId)
    {
        var state = repository.Read();
        return CalculateResult(GetStudent(state, studentId), state.Rubric);
    }

    public void ExportResults(string path)
    {
        var state = repository.Read();
        var rows = state.Students.Select(student =>
        {
            var (final, current) = CalculateResult(student, state.Rubric);
            if (final.HasValue)
                return new ResultRow(student.Id, student.Name, ResultStatus.Final, final);
            if (current.HasValue)
                return new ResultRow(student.Id, student.Name, ResultStatus.Provisional, current);
            return new ResultRow(student.Id, student.Name, ResultStatus.NotGraded, null);
        });

        exporter.Export(path, rows);
    }

    private StudentResult CalculateResult(Student student, Rubric rubric) =>
        new(calculator.Calculate(student, rubric, strictFinal), calculator.Calculate(student, rubric, provisional));

    private static Student GetStudent(PosState state, int studentId) =>
        state.Students.Find(student => student.Id == studentId)
        ?? throw new InvalidOperationException($"Student {studentId} was not found.");
}

public sealed record StudentEvaluation(int CriterionId, string CriterionName, decimal Score, decimal MaxScore);
public sealed record StudentResult(decimal? Final, decimal? Provisional);
