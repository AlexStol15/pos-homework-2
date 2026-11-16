using System.Globalization;
using Application;

namespace PosConsole;

internal sealed class PosMenu(PosFacade pos)
{
    private const string Menu = """
        ПОС — Преподавательская Оценка Студентов
         1. Add student                8. Delete criterion
         2. List students               9. Assign evaluation
         3. Edit student               10. List student evaluations
         4. Delete student             11. Correct evaluation
         5. Add criterion              12. Remove evaluation
         6. List criteria              13. Calculate student result
         7. Edit criterion            14. Export results to CSV
         0. Exit
        """;

    public void Run()
    {
        Console.WriteLine(Menu);
        while (true)
        {
            try
            {
                switch (Prompt("Option"))
                {
                    case "0":
                        return;
                    case "1":
                        pos.AddStudent(Prompt("Name"));
                        Console.WriteLine("Student added.");
                        break;
                    case "2":
                        ListStudents();
                        break;
                    case "3":
                        pos.EditStudent(ReadId("Student ID"), Prompt("New name"));
                        Console.WriteLine("Student updated.");
                        break;
                    case "4":
                        pos.DeleteStudent(ReadId("Student ID"));
                        Console.WriteLine("Student deleted.");
                        break;
                    case "5":
                        pos.AddCriterion(Prompt("Name"), ReadNumber("Maximum score"), ReadNumber("Weight"));
                        Console.WriteLine("Criterion added.");
                        break;
                    case "6":
                        ListCriteria();
                        break;
                    case "7":
                        pos.EditCriterion(ReadId("Criterion ID"), Prompt("New name"),
                            ReadNumber("New maximum score"), ReadNumber("New weight"));
                        Console.WriteLine("Criterion updated.");
                        break;
                    case "8":
                        pos.DeleteCriterion(ReadId("Criterion ID"));
                        Console.WriteLine("Criterion deleted.");
                        break;
                    case "9":
                        pos.AssignEvaluation(ReadId("Student ID"), ReadId("Criterion ID"), ReadNumber("Score"));
                        Console.WriteLine("Evaluation assigned.");
                        break;
                    case "10":
                        ListEvaluations();
                        break;
                    case "11":
                        pos.CorrectEvaluation(ReadId("Student ID"), ReadId("Criterion ID"), ReadNumber("New score"));
                        Console.WriteLine("Evaluation corrected.");
                        break;
                    case "12":
                        pos.RemoveEvaluation(ReadId("Student ID"), ReadId("Criterion ID"));
                        Console.WriteLine("Evaluation removed.");
                        break;
                    case "13":
                        ShowResult();
                        break;
                    case "14":
                        pos.ExportResults("results.csv");
                        Console.WriteLine("Results exported to results.csv.");
                        break;
                    default:
                        Console.WriteLine("Choose an option from 0 to 14.");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                or IOException or InvalidDataException or UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
        }
    }

    private void ListStudents()
    {
        var students = pos.ListStudents();
        if (students.Count == 0)
            Console.WriteLine("No students yet.");

        foreach (var student in students.OrderBy(student => student.Id))
            Console.WriteLine($"{student.Id}. {student.Name}");
    }

    private void ListCriteria()
    {
        var criteria = pos.ListCriteria();
        if (criteria.Count == 0)
            Console.WriteLine("No criteria yet.");

        foreach (var criterion in criteria.OrderBy(criterion => criterion.Id))
        {
            var status = criterion.IsLocked ? ", locked" : "";
            Console.WriteLine($"{criterion.Id}. {criterion.Name} (max {criterion.MaxScore}, weight {criterion.Weight}{status})");
        }
    }

    private void ListEvaluations()
    {
        var evaluations = pos.ListEvaluations(ReadId("Student ID"));
        if (evaluations.Count == 0)
            Console.WriteLine("This student has no evaluations.");

        foreach (var evaluation in evaluations.OrderBy(evaluation => evaluation.CriterionId))
            Console.WriteLine($"{evaluation.CriterionId}. {evaluation.CriterionName}: {evaluation.Score} / {evaluation.MaxScore}");
    }

    private void ShowResult()
    {
        var result = pos.CalculateResult(ReadId("Student ID"));
        Console.WriteLine($"Final: {FormatResult(result.Final)}");
        Console.WriteLine($"Provisional: {FormatResult(result.Provisional)}");
    }

    private static string FormatResult(decimal? score) =>
        score?.ToString("F2", CultureInfo.InvariantCulture) ?? "Unavailable";

    private static string Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine()?.Trim() ?? throw new EndOfStreamException();
    }

    private static int ReadId(string label)
    {
        if (int.TryParse(Prompt(label), out var id) && id > 0)
            return id;

        throw new ArgumentException($"{label} must be a positive whole number.");
    }

    private static decimal ReadNumber(string label)
    {
        var input = Prompt(label).Replace(',', '.');
        const NumberStyles style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        if (decimal.TryParse(input, style, CultureInfo.InvariantCulture, out var value))
            return value;

        throw new ArgumentException($"{label} must be a number.");
    }
}
