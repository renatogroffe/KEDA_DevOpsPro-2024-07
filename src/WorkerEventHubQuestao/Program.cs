using WorkerEventHubQuestao;
using WorkerEventHubQuestao.Data;
using Microsoft.ApplicationInsights.DependencyCollector;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<VotacaoRepository>();

builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    options.ConnectionString =
        builder.Configuration.GetConnectionString("ApplicationInsights");
});

builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
(module, o) =>
{
    module.EnableSqlCommandTextInstrumentation = true;
});

var host = builder.Build();
host.Run();