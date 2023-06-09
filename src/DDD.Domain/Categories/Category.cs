﻿namespace DDD.Domain.Categories;

public class Category : AggregateRoot<CategoryId>
{
    public string Name { get; private set; } = default!;

    private Category() { }

    // NOTE: Need to use a factory, as EF does not let owned entities (i.e Money & Sku) be passed via the constructor
    public static Category Create(string name, ICategoryService categoryService)
    {
        var category = new Category
        {
            Id = new CategoryId(Guid.NewGuid()),
        };

        category.UpdateName(name, categoryService);

        category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name));

        return category;
    }

    public void UpdateName(string name, ICategoryService categoryService)
    {
        Guard.Against.Empty(name);

        if (categoryService.CategoryExists(name))
            throw new DomainException($"Category {name} already exists");

        Name = name;
    }
}
