# DapperUnitOfWork

This project researches implementing data access in clean architecture using
Dapper instead of EF. Unlike with EF, which provides repositories in the form of
`DbSet<T>` and unit of work in the form of `DbContext`, neither of these come
out of the box with Dapper. Instead Dapper is a thin object wrapper and not an
ORM. Similarly, instead of EF Migration, we use DbUp to manage the database
schema.

## Prototype investigation

1. Q: How much boilerplate code is required using Dapper over EF?

   A: A significant amount of repetitive code is needed to implement
   repositories with CRUD operations and mapping from database result to domain
   objects by hand.

   The Unit of Work pattern is fairly straightforward to implement and useful
   when in one transaction we want to modify more than one table. In many cases
   this happens rarely, through. Where it can happen is if we're subscribing to
   domain events within the application or if we have an invariant that states
   that every aggregate root must maintain the last modified timestamp. Whenever
   one of the children changes, say an order line, then the order must updates
   as well. This means updating both Orders and OrderLines tables in a single
   transaction.

1. Q: In clean architecture, domain classes tend to have getter properties only
   and are reconstructed by EF using their private constructors and reflection.
   How to achieve a similar result with Dapper?

   A: We can't pass into a domain object constructor the values to reconstruct
   it. Reconstructing from database isn't the same as creating the instance in a
   command request. When called from a command, a domain event is published
   which should never happen when reconstructed from a database. Also, we can't
   add setter properties to the domain object as it would risk breaking object
   invariants. To be able to set a property value using reflection, it must be
   declared as "(private) set" or "init", or we must use the internal backing
   field name.

   EF reconstructs object by calling their private constructor. Then EF uses
   reflection to set properties without a setter using information from the
   Fluent API mappings. The assumption is that a domain object reconstructed
   from the database is by definition in a valid state, so regular validation
   can be safely bypassed. The same holds for ValueObject which must also have a
   private parameterless constructor. 
   
   Hand-writing this reflection code is a lot of tedious work. But
   reconstructing a domain object through a private constructor with arguments
   doesn't work well as the private and public constructors may have the exact
   same arguments. Also, the private constructor would have be called using
   reflected and the parameters would become losely typed.  

   No good solution without significant overhead exists to support the clean
   architecture. A hacky solution would be to add a marker parameter to the
   private constructor to signal to clients that it's only to be called when
   reconstructing the object. The arguments would then differ from the existing
   public constructor so both constructors could be make public.

1. Q: Without automatic change tracking in Dapper, if you have an Order with
   OrderLines, what would it take to track changes?
   
   A: It's up to repository code to change track and issue correspond SQL. For
   instance, by loading every row ID on OrderLines based on OrderId to determine
   which order lines was been deleted, added, or updated. So while we may delete
   one order line at a time, through a command, simplifying the change tracking,
   tracking changes inside an object adds significant boilerplate code.

   If we have CreatedBy, CreatedAt, UpdatedBy, UpdatedAt fields on each domain
   object, setting these become the responsibility of the repository. We can't
   abstract it to the DbContext as with EF. That would require a more
   sophisticated change tracker.

With the above limitations of Dapper, Dapper appears best suited for a legacy
database where EF Code First may be tricky to setup correctly or for
applications that aren't clean architecture based. For a new application, and
the database owned by that application, EF seems the best option for CRUD
operations. For the query side, Dapper might still be an option. However, it's
possible to execute straight SQL though EF as well, so the benefits of Dapper
would be small.

## References

- [How to implement Unit Of Work pattern with Dapper?
](https://stackoverflow.com/questions/31298235/how-to-implement-unit-of-work-pattern-with-dapper/45029588)