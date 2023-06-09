﻿using DDD.Domain.Common.Entities;
using DDD.Domain.Products;

namespace DDD.Domain.Orders;

public class LineItem : Entity<LineItemId>
{
    public required OrderId OrderId { get; init; }

    public required ProductId ProductId { get; init; }

    public Product? Product { get; init; }

    // Detatch price from product to capture the price at the time of purchase
    public required Money Price { get; init; }

    public int Quantity { get; private set; }

    public Money Total => new(Price.Currency, Price.Amount * Quantity);

    private LineItem() { }

    // NOTE: Need to use a factory, as EF does not let owned entities (i.e Money) be passed via the constructor
    // Internal so that only the Order can create a LineItem
    internal static LineItem Create(OrderId orderId, ProductId productId, Money price, int quantity)
    {
        Guard.Against.ZeroOrNegative(price.Amount);
        Guard.Against.ZeroOrNegative(quantity);

        var lineItem = new LineItem()
        {
            Id = new LineItemId(Guid.NewGuid()),
            OrderId = orderId,
            ProductId = productId,
            Price = price,
            Quantity = quantity
        };

        return lineItem;
    }

    internal void AddQuantity(int quantity) => Quantity += quantity;

    internal void RemoveQuantity(int quantity)
    {
        Guard.Against.Condition(Quantity - quantity <= 0, "Can't remove all units.  Remove the entire item instead.");
        Quantity -= quantity;
    }
}

