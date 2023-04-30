# dotnet-ef-domain-driven-design

[![.NET](https://github.com/danielmackay/dotnet-ef-domain-driven-design/actions/workflows/dotnet.yml/badge.svg)](https://github.com/danielmackay/dotnet-ef-domain-driven-design/actions/workflows/dotnet.yml)

## Features

- Aggregate Roots
- Entities
- Value Objects
- Strongly Typed IDs
- Domain events
- CQRS Commands & Queries
- Fluent Validation
- Minimal APIs
- Specifications
- Outbox Pattern with Hangfire background processing

## Key Design Decisions

### Object Creation

- Objects must be constructed with a factory pattern so that domain events can be raised upon explicit creation, but NOT raised when EF fetches entities from the DB.
- Properties need to be passed to constructors to ensure they are in a valid state on object creation.  Can't use `required init` properties as they then become unmodifiable.
- EF does not allow owned entities to be passed to constructors, so these MUST be set via factory methods.
- Can remove nullable warnings by using `null!`.  This is safe to do so as we can only create an object via our factory method which we know sets these properties.

### Use Specifications to load aggregate roots

Aggregate roots need to be loaded in their entirety.  This means the root entities and all child entities that make up the aggregate root.  To achieve this we will use specifications so that we can consistently load the aggregate root and all of its related entities in a single query.

### DomainService interfaces will need to exist in Domain

Sometimes entities will need to leverage a service to perform a behavior.  In these scenarios, we will need a DomainService interface in the Domain project, and implementation in the Application or Infrastructure project.

### Unit Test Naming Conventions

Test naming convention should follow the pattern

```cs
public void {Method/PropertyName}_Should_{ExpectedBehavior}_When_{StateUnderTest}()
```
