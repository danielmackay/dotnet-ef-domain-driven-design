﻿using DDD.Domain.Common.Entities;
using System.Text.Json.Serialization;

namespace DDD.Application.Orders.Commands.AddPayment;

public record AddPaymentCommand(string Currency, decimal Amount) : IRequest
{
    [JsonIgnore]
    public Guid OrderId { get; set; }
}

public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public AddPaymentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var spec = new OrderByIdSpec(orderId);
        var order = await _dbContext.Orders
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException();

        var payment = new Money(new Currency(request.Currency), request.Amount);
        order.AddPayment(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}