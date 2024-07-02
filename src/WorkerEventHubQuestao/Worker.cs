using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using WorkerEventHubQuestao.Data;
using WorkerEventHubQuestao.EventHubs;

namespace WorkerEventHubQuestao;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly VotacaoRepository _repository;
    private readonly string _eventHub;
    private readonly EventProcessorClient _processor;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public Worker(ILogger<Worker> logger,
       IConfiguration configuration,
       VotacaoRepository repository)
    {
        _logger = logger;
        _repository = repository;

        _eventHub = configuration["AzureEventHubs:EventHub"]!;
        var consumerGroup = configuration["AzureEventHubs:ConsumerGroup"];
        var blobContainer = configuration["AzureEventHubs:BlobContainer"];

        _processor = new EventProcessorClient(
            new BlobContainerClient(
                configuration["AzureEventHubs:BlobStorageConnectionString"],
                blobContainer),
            consumerGroup,
            configuration["AzureEventHubs:EventHubsConnectionString"],
            _eventHub);
        _processor.ProcessEventAsync += ProcessEventHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        _logger.LogInformation($"Event Hub = {_eventHub}");
        _logger.LogInformation($"Consumer Group = {consumerGroup}");
        _logger.LogInformation($"Blob Container = {blobContainer}");

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Iniciando {nameof(StartAsync)}...");
        _processor.StartProcessing(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executando {nameof(StopAsync)}...");
        _processor.StopProcessing();
        return Task.CompletedTask;
    }

    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        var eventData = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss} Evento] " + eventData);

        var dataProcessed = false;
        QuestaoEventData? questaoEventData = null;
        try
        {
            questaoEventData = JsonSerializer.Deserialize<QuestaoEventData>(
                eventData, _jsonSerializerOptions);
        }
        catch
        {
            _logger.LogError(
                "Erro durante a deserializacao dos dados recebidos!");
        }

        if (questaoEventData is not null &&
            !String.IsNullOrWhiteSpace(questaoEventData.IdVoto) &&
            !String.IsNullOrWhiteSpace(questaoEventData.Horario) &&
            !String.IsNullOrWhiteSpace(questaoEventData.Producer) &&
            !String.IsNullOrWhiteSpace(questaoEventData.Interesse))
        {
            _repository.Save(questaoEventData);
            _logger.LogInformation($"Voto = {questaoEventData.IdVoto} | " +
                $"Tecnologia = {questaoEventData.Interesse} | " +
                "Evento computado com sucesso!");
            dataProcessed = true;
        }

        if (!dataProcessed)
            _logger.LogError("Formato dos dados do evento inválido!");

        await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        _logger.LogError($"Error Handler Exception: {eventArgs.Exception.Message}");
        return Task.CompletedTask;
    }
}