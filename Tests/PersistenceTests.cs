using Application;
using Domain;
using Infrastructure;
using Xunit;

namespace Tests;

public sealed class PersistenceTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("pos-tests-");
    private string DataPath => Path.Combine(_directory.FullName, "data.json");

    [Fact]
    public void CriterionStaysLockedAfterGradingRemovingGradeAndReloading()
    {
        var facade = CreateFacade();
        facade.AddStudent("Anna");
        facade.AddCriterion("Exam", 20, 2);
        Assert.Throws<ArgumentException>(() => facade.AssignEvaluation(1, 1, 25));
        Assert.False(Assert.Single(facade.ListCriteria()).IsLocked);
        facade.AssignEvaluation(1, 1, 15);

        var restarted = CreateFacade();
        Assert.True(Assert.Single(restarted.ListCriteria()).IsLocked);
        Assert.Equal(15m, Assert.Single(restarted.ListEvaluations(1)).Score);
        restarted.RemoveEvaluation(1, 1);

        var reloaded = CreateFacade();
        Assert.Empty(reloaded.ListEvaluations(1));
        Assert.True(Assert.Single(reloaded.ListCriteria()).IsLocked);
        Assert.Throws<InvalidOperationException>(() => reloaded.EditCriterion(1, "New exam", 30, 3));
        Assert.Throws<InvalidOperationException>(() => reloaded.DeleteCriterion(1));
    }

    [Fact]
    public void InvalidJsonRemainsUnchangedWhenAWriteIsAttempted()
    {
        const string invalidJson = "{ broken";
        File.WriteAllText(DataPath, invalidJson);

        Assert.Throws<InvalidDataException>(() => CreateFacade().AddStudent("Anna"));
        Assert.Equal(invalidJson, File.ReadAllText(DataPath));
    }

    private PosFacade CreateFacade()
    {
        var factory = new EvaluationFactory();
        return new PosFacade(new JsonPosRepository(DataPath, factory), new CsvResultsExporter(),
            factory, new ScoreCalculator(), new StrictFinalStrategy(), new ProvisionalStrategy());
    }

    public void Dispose() => _directory.Delete(recursive: true);
}
