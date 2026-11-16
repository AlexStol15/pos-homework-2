using Application;

namespace Infrastructure;

public sealed class CsvResultsExporter : IResultsExporter
{
    public void Export(string path, IEnumerable<ResultRow> rows)
    {
        using var writer = new StreamWriter(path) { NewLine = "\r\n" };
        writer.WriteLine("StudentId,StudentName,Status,Score");
        foreach (var row in rows)
            writer.WriteLine(FormattableString.Invariant($"{row.StudentId},{Escape(row.StudentName)},{row.Status},{row.Score:F2}"));
    }

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) < 0 ? value : "\"" + value.Replace("\"", "\"\"") + "\"";
}
