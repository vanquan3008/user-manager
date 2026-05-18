namespace UserService.Application.Configuration;

public sealed class OutboxConfiguration
{
    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 20;
    public int PollIntervalSeconds { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 10;
}
