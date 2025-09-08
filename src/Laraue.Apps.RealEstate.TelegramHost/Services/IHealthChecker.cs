namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public interface IHealthChecker
{
    Task<WorkerHealthResult> CheckHealthAsync();
}

public class WorkerHealthResult
{
    public bool IsReady { get; init; }

    public string? Error { get; init; }
}