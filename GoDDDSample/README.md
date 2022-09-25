# Go Domain Driven Design sample

Achieving a pure DDD architecture with Go is hard. Go's limited type system and
unpopularity of ORMs mean we face a complex implementation task or only
partially implement DDD. Granted, not every DDD solution requires the full
complement of DDD features, but a solution grows it's deemed to require more
features. With Go it's easy getting locked into a half-baked custom
implementation.

## Deviations from pure DDD architecture

- *Application layer Data Transfer Objects contains JSON serialization
  annotations*. Stictly speaking, adding these annotations is a violation of the
  architecture as Application is presentation neutral. The problem with Go,
  however, is that it has no equivalent of .NET's Json.NET library where one can
  declaratively specify how to serialize any object, e.g., change FooBar to
  foo_bar or serialize DateTime to a specific format across serialization of all
  types.
  
  If we didn't add annotations on ProductDto, with out of the box Go, we'd be
  forced into creating a DTO specific to web JSON serialization, and add the
  annotations on that type. It would add boilerplate, not just in defining the
  types but copying ProductDto fields to ProductWebDto.
  
  A similar case could be make one level deeper and where we could've added
  annotations on value objects and entities. At least by annotating ProductDto,
  we don't deeply permeate Core with presentation details.
  
- *Without a ORM, tracking changes across aggregates is made difficult*. Several
  options exist, such as inside commands mirroring update operations on the
  aggregate on the database. But that means the repository interface must expose
  high-granularity operations and there's a good change in-memory and
  in-database models get out of sync.
  
  Another option is emitting domain events and on the repository expose
  ProductSave() methods for the Product aggregate. Inside that method method, we
  could iterate domain events stored on Product aggregate and map those to
  database operations. The assumption is that everything we need to persist can
  be extracted from the domain event or from the public interface of the
  aggregate (if not, then values would have be extracted using reflection or the
  Memento pattern. Possibly we'd encounter issues with one domain event
  triggering a handler to update another aggregate).
  
  Updating a relational database based on domain events is a hybrid between
  relational and event sourcing. The alternative is writing persistence code to
  traverse an aggregate, detecting changing. This implies we first fetch current
  values from the database, then calculate the delta between two object
  hierarchies, and generate SQL from the delta (what an ORM would do for us by
  change tracking behind the scenes).
  
  To compare, with .NET's EF + MediatR, we can call dbContext.SaveChanges() at
  the end of a command and SaveChanges is responsible for iterating domain
  events and trigger calls to handlers listening for events as well as updating
  any CreatedAt and ModifiedAt fields on entities and aggregate root. The EF
  context can even change track multiple aggregate roots. While with Go, we
  could make it work for a single aggregate with SaveProductChanges(), making it
  work across multiple aggregates without an ORM and MediatR tracker is
  non-trivial.

## Conclusion

Go isn't a good fit for a full DDD architecture. It's more well-suited for the
traditional presentation, business logic, database three-layer architecture.
