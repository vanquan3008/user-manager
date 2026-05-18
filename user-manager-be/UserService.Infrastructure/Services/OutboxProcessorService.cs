using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Domain.Events;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Services;

public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly int _batchSize;
    private readonly int _maxRetryCount;

    public OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        AppSettings appSettings,
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollInterval = TimeSpan.FromSeconds(Math.Max(1, appSettings.Outbox.PollIntervalSeconds));
        _batchSize = Math.Max(1, appSettings.Outbox.BatchSize);
        _maxRetryCount = Math.Max(1, appSettings.Outbox.MaxRetryCount);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_pollInterval);

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing outbox messages.");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.AttemptCount < _maxRetryCount)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            var utcNow = clock.UtcNow;
            try
            {
                var domainEvent = OutboxJsonSerializer.Deserialize(message.Payload, message.Type);
                await PublishAsync(mediator, domainEvent, cancellationToken);
                message.MarkProcessed(utcNow);
            }
            catch (Exception ex)
            {
                message.MarkFailed(utcNow, ex.Message);
                _logger.LogError(ex, "Failed to process outbox message {OutboxMessageId}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Task PublishAsync(IMediator mediator, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notification = Activator.CreateInstance(notificationType, domainEvent)
            ?? throw new InvalidOperationException($"Unable to create notification for event '{domainEvent.GetType().Name}'.");

        return mediator.Publish((INotification)notification, cancellationToken);
    }
}
