# Go Domain Driven Design sample

Doing DDD with Go is hard. Go's limited type system and the unpopularity of ORMs
lead to a verbose implementation task. Granted, not every DDD solution requires
the full complement of DDD concept, but as a solution grows it's deemed to
require more concepts.

## Deviations from pure DDD architecture

- *Application layer Data Transfer Objects contains JSON serialization
  annotations*. Stictly speaking, adding these annotations is a violation of the
  architecture as Application should be presentation neutral. The problem with
  Go, however, is that it has no equivalent of .NET's Json.NET library where one
  declaratively specify how to serialize any object, e.g., change FooBar to
  foo_bar or serialize DateTime to a specific format across all types.
  
  In Go, if we don't add annotations to ProductDto, we're forced to create a DTO
  specific to web JSON serialization, and annotate that type. It would add
  boilerplate, not just in defining the types but in mapping ProductDto to
  ProductWebDto.
  
  A similar case could be make one level deeper where we could've added
  annotations on value objects and entities. At least by annotating ProductDto,
  we don't permeate Core with presentation details.
  
- *Without a ORM, tracking changes across aggregates is difficult*. Several
  options exist, such as inside commands mirroring update operations on the
  aggregate on the database. But that means there's a good change in-memory and
  in-database models get out of sync.
  
  Another option is emitting domain events and on the repository expose a
  ProductSave(p Product) method for the Product aggregate. Inside it, we could
  iterate domain events stored on Product aggregate and map those to database
  operations. The assumption is that everything we need to persist can be
  extracted from domain events (if not, then values would have to be extracted
  using reflection or the Memento pattern). Possibly we'd encounter issues with
  one domain event triggering a handler to update another aggregate).
  
  Updating a relational database based on domain events is a hybrid between
  relational and event sourcing. The alternative is writing persistence code to
  traverse an aggregate to detect changes. This implies we first fetch current
  values from the database, then calculate the delta between two object
  hierarchies, and generate SQL to reconsile the two (what an ORM would do for
  us by change tracking behind the scenes).
  
  By comparison, with .NET's EF + MediatR, we can call dbContext.SaveChanges()
  at the end of a command and SaveChanges() is responsible for iterating domain
  events and trigger calls to handlers listening for events as well as updating
  any CreatedAt and ModifiedAt fields on entities and aggregate root. The EF
  context can even change track multiple aggregate roots. With Go, we could make
  it work for a single aggregate with SaveProductChanges(), but making it work
  across multiple aggregates without an ORM and MediatR is non-trivial.

## Conclusion

Go isn't an optimal fit for a full DDD architecture.

