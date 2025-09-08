using Microsoft.Extensions.Options;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public class HealthChecker : IHealthChecker
{
    private readonly HttpClient _client;
    private readonly HealthOptions _options;

    public HealthChecker(IOptions<HealthOptions> options, HttpClient client)
    {
        _client = client;
        _options = options.Value;
        
        _client.Timeout = TimeSpan.FromSeconds(3);
    }
    
    public async Task<WorkerHealthResult> CheckHealthAsync()
    {
        try
        {
            var response = await _client.GetAsync(_options.WorkerHost);
            if (response.IsSuccessStatusCode)
            {
                return new WorkerHealthResult
                {
                    IsReady = true
                };
            }
        
            return new WorkerHealthResult
            {
                Error = $"Service is not ready (status code: {response.StatusCode})"
            };
        }
        catch (Exception e)
        {
            return new WorkerHealthResult
            {
                Error = e.Message
            };
        }
    }
}

public class HealthOptions
{
    public required string WorkerHost { get; init; }
}