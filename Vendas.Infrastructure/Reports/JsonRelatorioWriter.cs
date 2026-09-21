using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Infrastructure.Reports;

public sealed class JsonRelatorioWriter(IConfiguration configuration) : IRelatorioWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task WriteAsync(RelatorioVendasResponse relatorio, CancellationToken cancellationToken)
    {
        var outputDirectory = configuration["Reports:OutputDirectory"] ?? "reports";
        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, $"relatorio-{relatorio.ReportId}.json");
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, relatorio, JsonOptions, cancellationToken);
    }
}
