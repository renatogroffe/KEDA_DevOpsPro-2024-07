using Dapper.Contrib.Extensions;

namespace WorkerEventHubQuestao.Data;

[Table("dbo.HistoricoVotacao")]
public class VotoTecnologia
{
    [Key]
    public int Id { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string? EventHub { get; set; }
    public string? Producer { get; set; }
    public string? Consumer { get; set; }
    public string? ConsumerGroup { get; set; }
    public string? HorarioVoto { get; set; }
    public string? IdVoto { get; set; }
    public string? Interesse { get; set; }
}