namespace Vendas.Worker.Models;

public sealed record ReportQueueMessage(
    Guid ReportId,
    DateTime StartDate,
    DateTime EndDate);
