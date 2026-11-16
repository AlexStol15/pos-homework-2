using Application;
using Domain;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PosConsole;

var services = new ServiceCollection();
services.AddSingleton<EvaluationFactory>();
services.AddSingleton<ScoreCalculator>();
services.AddSingleton<StrictFinalStrategy>();
services.AddSingleton<ProvisionalStrategy>();
services.AddSingleton<IPosRepository>(provider =>
    new JsonPosRepository("data.json", provider.GetRequiredService<EvaluationFactory>()));
services.AddSingleton<IResultsExporter, CsvResultsExporter>();
services.AddSingleton<PosFacade>();
services.AddSingleton<PosMenu>();

using var provider = services.BuildServiceProvider();
provider.GetRequiredService<PosMenu>().Run();
