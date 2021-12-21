# DapperUnitOfWork

This project researches how to implement data access in clean architecture using
Dapper. Unlike with EF, with a repository in the form of `DbSet<T>` and a unit
of work in the form of `DbContext`, neither come with Dapper. Dapper is a thin
object mapper, not an ORM, and these must be build on top of Dapper.

Similarly, instead of EF Migration, we use `DbUp` to manage the database schema.

## Prototype investigation

*Q: How much boilerplate code is required to use Dapper over EF?*

A significant amount of repetitive code is needed to implement repositories with
CRUD operations and mapping database results to domain objects. The unit of work
pattern is fairly straightforward to implement, and comes in handy if we're
subscribing to domain events inside the application or if every aggregate root
must maintain its last modified timestamp or that of any of its children. For
instance, whenever a root's children change, say an OrderLine, the Order must
update as well. This means updating both Orders and OrderLines tables in a
single transaction.

*Q: In clean architecture, domain classes tend to have getters only, and are
reconstructed by EF using their private constructor and reflection. How to
achieve a similar result with Dapper?*

On first sight, reconstructing a domain object from a database, we can't pass
into its constructor the values to reconstruct it. Because reconstructing from
database is different from creating the object through a command, where a domain
event is published.

We also can't add setters to the domain object as that would potentially break
object invariants. Also, to set a property value using reflection, the property
must be declared `(private) set` or `init`, or we'd have to use the internal
backing field name. With EF, objects may be instantiated by EF using the private
parameterless constructor. Then EF uses reflection to set properties without
going through setters, using information from Fluent API mappings. By definition
a domain object is reconstructed from the database in a valid state, so
invariant checking may be bypassed. The same holds true for `ValueObject`s which
also have a private parameterless constructor and a private `Value` property.
 
Handwriting reflection code to instantiate a domain object using a private
parameterless constructor and setting values is tedious and error-prone. As an
alternative, instantiating a domain object through its private constructor with
arguments doesn't work all that well. The private and public constructors may
end up having the exact same list of arguments which C# disallows.

Then we could add a marker argument, an argument with a default value only
explicitly set by Dapper, to the public constructor and get the best of both
worlds: no need for reflection and domain objects could be reconstructed using
their public constructors. The compiler would catch type errors as the
application evolves and performance without reflection would be excellent. The
marker argument should be an `enum`, not a `bool`, value and should have a
default value to hide the arguments from commands. Repository logic should set
the value explicitly which causes the domain object to skip publishing of domain
events. 

We don't violate clean architecture by adding an extra argument. What clean
architecture states is that the domain must remain persistence ignorant, i.e.,
not dependent on any database. This doesn't preclude a domain object from
knowing when it's getting reconstituted. It's no different than EF forcing a
private parameterless constructor.

* Q: Without automatic change tracking in Dapper, if we have an `Order` with
`OrderLines`, what would it take to track changes?*
   
It's up to repository code to change track and issue correct SQL. For instance,
loading `OrderLines` based on `OrderId` to determine which `OrderLines` has been
deleted, added, or updated. While we may delete one `OrderLine` at a time,
through a command, which is easily tracked by its Id, and similarly for updating
the `OrderLine` through a command, in general determining if individual
properties of an `OrderLine` was modified adds significant boilerplate
code. That is, unless we can take advantage of records and their compiler
implementation object comparisons.

Alternatively, we must implement the unit of work pattern to simplify repository
implementations, and have the application manually indicate modifications,
additions, and deletions across any number of aggregates, e.g., multiple
aggregates may have changed as the result of subscribers to domain events. The
unit of work would know how to surgically modify, add, delete each entity with
possible child entities. `CreatedBy` and `CreatedAt` columns could also be
automatically set by the unit of work based on the nature of the change.

*Q: How to set system properties on each record without repetition?*

If we have `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt` fields on each
domain object, setting these become the responsibility of repositories. We can't
abstract setting these properties to the `DbContext` as with EF. Knowing which
objects in the `DbContext` are modified, added, or deleted would require a more
sophisticated change tracker.

## Conclusion

Dapper appears best suited for cases where a database already exists and EF
mappings would be tricky to get right. Or for applications that aren't clean
architecture based or where we want maximum control over the queries. For a new
application which owns the database, EF seems the best option the CRUD part. On
the query side, Dapper might still be an option. However, it's possible to
execute straight SQL though EF as well, so the benefits of Dapper may be
insignificant.

In the procedures set forth above, persistence is made unnecessary hard by
forcing a one-to-one implementation of EF features with tooling whose nature is
more explicit. We're mimicking EF, doing state tracking in-memory on the client
to minimize the number of database interactions to what EF determines optimal.

Suppose the database isn't the bottleneck. Then we could've the database track
changes on behalf of the application without violating clean architecture
principles. When a request comes into the application, the application starts a
snapshot transaction and does its work. If any part of the snapshot is modified
by another transaction, the current transaction would automatically fail on
commit.

The downside is that anytime we're applying an operation to the in-memory object
reconstructed from the database, we (may, if values are to be used later) have
to call into the persistence layer and have it apply the same operation. Then,
as with EF, an event handler would start by fetching the aggregate from the
database, and given the modified snapshot, values set elsewhere in the
transaction would be visible to the event handler, and it could apply additional
operations.

## References

- [How to implement Unit Of Work pattern with Dapper?
](https://stackoverflow.com/questions/31298235/how-to-implement-unit-of-work-pattern-with-dapper/45029588)
