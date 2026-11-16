namespace Application;

public interface IResultsExporter
{
    void Export(string path, IEnumerable<ResultRow> rows);
}

public enum ResultStatus
{
    Final,
    Provisional,
    NotGraded
}

public sealed record ResultRow(int StudentId, string StudentName, ResultStatus Status, decimal? Score);
