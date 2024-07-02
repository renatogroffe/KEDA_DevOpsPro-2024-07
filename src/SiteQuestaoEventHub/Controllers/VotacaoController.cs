using Microsoft.AspNetCore.Mvc;
using SiteQuestaoEventHub.EventHubs;

namespace SiteQuestaoEventHub.Controllers;

public class VotacaoController : Controller
{
    private readonly ILogger<VotacaoController> _logger;
    private readonly VotacaoProducer _producer;

    public VotacaoController(ILogger<VotacaoController> logger,
        VotacaoProducer producer)
    {
        _logger = logger;
        _producer = producer;
    }

    public async Task<IActionResult> VotoArgentina()
    {
        return await ProcessarVoto("Argentina");
    }

    public async Task<IActionResult> VotoBrasil()
    {
        return await ProcessarVoto("Brasil");
    }

    public async Task<IActionResult> VotoUruguai()
    {
        return await ProcessarVoto("Uruguai");
    }

    public async Task<IActionResult> VotoColombia()
    {
        return await ProcessarVoto("Colombia");
    }

    private async Task<IActionResult> ProcessarVoto(string interesse)
    {
        _logger.LogInformation($"Processando voto para o interesse: {interesse}");
        await _producer.Send(interesse);
        _logger.LogInformation($"Informações sobre o voto '{interesse}' enviadas para o Azure Event Hubs!");

        TempData["Voto"] = interesse;
        return RedirectToAction("Index", "Home", new { voto = interesse });
    }
}