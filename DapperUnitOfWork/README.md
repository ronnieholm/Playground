# DapperUnitOfWork

This project researches how to implement data access in clean architecture using
Dapper instead of EF. Unlike with EF, which provides repositories in the form of
`DbSet<T>` and a unit of work implementation in the form of `DbContext`, neither
of these come out of the box with Dapper. As Dapper is a thin object mapper and
not an ORM, we have to build these on top of Dapper. 

Similarly, instead of EF Migration, we use DbUp to manage the database schema.

## Prototype investigation

- Q: How much boilerplate code is required to use Dapper over EF?

  A: A significant amount of repetitive code is needed to implement
  repositories with CRUD operations and mapping database results to domain
  objects.
  The Unit of Work pattern is fairly straightforward to implement. It comes in
  useful if we're subscribing to domain events within the application or if we
  have an invariant that states that every aggregate root must maintain the
  last modified timestamp of itself and any of its children. Whenever one of
  the children change, say an order line, the order must update as well. This
  means updating both Orders and OrderLines tables in a single transaction.

- Q: In clean architecture, domain classes tend to have getters only, and are
  reconstructed by EF using their private constructor and reflection. How to
  achieve a similar result with Dapper?

  A: On first sight, reconstructing a domain object from the database, we can't
  pass into its constructor the values to reconstruct it as reconstructing from
  database is different from creating the object through a command request. When
  constructed by a command, a domain event is published which should never
  happen when reconstructed from a database. 
  
  We can't add setters to the domain object as it would potentially break object
  invariants. And to set a property value using reflection, it must be declared
  as "(private) set" or "init", or we must use the internal backing field name.

  With EF, objects may be reconstructed  by EF rist calling their private
  constructor. Then EF uses reflection to set properties without setters using
  information from the Fluent API mappings. The assumption is that a domain
  object reconstructed from the database is by definition in a valid state, so
  invariant checking can be bypassed. The same holds for ValueObject which also
  have a private parameterless constructor and a private Value property.
   
  Hand-writing reflection code is tedious work. But even reconstructing a domain
  object through a private constructor with arguments doesn't work well. The
  private and public constructors may have the exact same list of arguments. The
  private constructor would have to be called using reflection and the
  parameters would become losely typed.  

  Adoping to Dapper, adding a marker argument to the public constructor would
  give the best from both worlds: no need for reflection and reconstructing
  objects using their public constructor. The compiler will catch any type
  errors as the application evolves and performance without reflection is
  excellent. The marker argument should be an enum, not a boolean, value and it
  should have a default value to hide the feature from commands. Repository
  logic should set the value explicitly which would skip the publishing of
  domain events.

  As such, we don't violate clean architecture by adding the extra argument.
  What clean architecture states is that the domain must remain persistance
  ignorant, i.e., not dependent on any database. That doesn't preclude a domain
  object from knowing when it's getting reconstituted. It's no different than
  the EF requirement of a private parameterless constructor.

- Q: Without automatic change tracking in Dapper, if we have an Order with
  OrderLines, what would it take to track changes?
   
  A: It's up to repository code to change track and issue the corresponding SQL.
  For instance, loading OrderLines based on OrderId to determine which order
  lines was deleted, added, or updated. While we may delete one order line at a
  time, through a command, which is easily tracked by its Id, and similarly for
  updating the OrderLine through a command, in general determining if properties
  of an OrderLine has changes adds significant boilerplate code. That is, unless
  we can take advantage of records and their compiler implementation object
  comparisons.

  Alternatively, we must implement the Unit of Work pattern to simplify
  repository implementations, and have the application manually indicate
  changes, additions, and deletions across any number of aggregates, e.g.,
  multiple aggregates may have changed as the result of subscribers to domain
  events.

- Q: How to set system properties on each record with repetition?

  A: If we have CreatedBy, CreatedAt, UpdatedBy, UpdatedAt fields on each domain
  object, setting these become the responsibility of repositories. We can't
  abstract setting these properties to the DbContext as with EF. Knowing which
  objects in the DbContext are added or updated would require a more
  sophisticated change tracker.

## Conclusion

Dapper appears best suited for cases where a database already exists and EF
mappings would be tricky to get right. Or for applications that aren't clean
architecture based or where we want maximum control over the queries. For a new
application, who owns the database application, EF seems the best option the
CRUD part. On the query side, Dapper might still be an option. However, it's
possible to execute straight SQL though EF as well, so the benefits of Dapper
may be insignificant.

## References

- [How to implement Unit Of Work pattern with Dapper?
](https://stackoverflow.com/questions/31298235/how-to-implement-unit-of-work-pattern-with-dapper/45029588)