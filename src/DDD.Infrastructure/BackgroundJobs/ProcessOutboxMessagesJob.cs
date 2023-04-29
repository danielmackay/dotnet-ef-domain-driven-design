﻿using DDD.Domain.Common.Base;
using DDD.Domain.DomainServices;
using DDD.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace DDD.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IPublisher _publisher;
    private readonly IDateTime _dateTime;

    public ProcessOutboxMessagesJob(ApplicationDbContext applicationDbContext, IPublisher publisher, IDateTime dateTime)
    {
        _applicationDbContext = applicationDbContext;
        _publisher = publisher;
        _dateTime = dateTime;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _applicationDbContext.OutboxMessages
            .Where(om => om.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(
                message.Content,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            if (domainEvent is null)
                continue;

            // NOTE: In Production, this should be wrapped in a try-catch
            await _publisher.Publish(domainEvent, context.CancellationToken);

            message.ProcessedOnUtc = _dateTime.Now;
        }

        await _applicationDbContext.SaveChangesAsync();
    }
}
