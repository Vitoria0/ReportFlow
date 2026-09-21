namespace Vendas.Domain.Entities;

public sealed class ReportRequest
{
    private ReportRequest()
    {
    }

    private ReportRequest(Guid id, DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("A data inicial deve ser menor ou igual a data final.");

        Id = id;
        StartDate = startDate;
        EndDate = endDate;
        Status = ReportRequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ReportRequest Create(DateTime startDate, DateTime endDate) =>
        new(Guid.NewGuid(), startDate, endDate);

    public void MarkProcessing()
    {
        Status = ReportRequestStatus.Processing;
        ErrorMessage = null;
    }

    public void MarkCompleted()
    {
        Status = ReportRequestStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = ReportRequestStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}

public static class ReportRequestStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}