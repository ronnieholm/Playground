# DapperUnitOfWork

This project researches how to do data access in clean architecture using Dapper
(any non-ORM, really). Unlike with EF, its `DbSet<T>` repositories, and
`DbContext` unit of work, neither are part of Dapper. Dapper is a thin object
mapper, not an ORM, and those must be build on top of Dapper.

Similarly, EF Migration must be substituted by the DbUp tool or similar.

## Prototype investigation

*Q: How much boilerplate code is required to use Dapper over EF?*

A significant amount of repetitive code could go into implementing repositories
with CRUD operations and map database results to domain objects. The unit of
work pattern is straightforward to implement, and works well with emitting
domain events inside the application. For instance, when an OrderLine changes,
the Order may need to be updated as well. Both Orders and OrderLines tables must
be updated in a single transaction.

*Q: In clean architecture, a domain class tends to have getters only, and is
reconstructed by EF using its private constructor and reflection. How to achieve
a similar result with Dapper?*

Reconstructing a domain object, we can't pass into its public constructor the
values to reconstruct it. Reconstructing from database is different from
creating the domain objects, where a domain event may be emitted. Ignoring the
domain event, we may not be able to arrive at the state of the object through
the public constructor alone. Additional calls, which we cannot know about, may
have affect the object's state. Or the object may track internal state whose
value is being calculated internally.

We can't add setters to the domain object as it would potentially break object
invariants. And to set a property value using reflection, the property must be
declared as `private set` or `init`, or only the internal backing field name is
available.

With EF, setting through reflection takes place based on information from
mappings. If by definition a domain object is always reconstructed from the
database in a valid state, invariant checking can be bypassed. The same holds
true for value objects which have a private parameter-less constructor and a
private `Value` property.

For us to instantiate a domain object using a private parameter-less constructor
and per-field reflection is tedious and error-prone. A private constructor with
arguments may not work all that well. The private and public constructor may end
up having the same list of arguments which C# disallows. Though with `CreatedAt`
and `UpdatedAt` entity fields, the constructors may differ. These fields could
be set by the database layer and not through the public constructor. Or we could
add a marker argument to the private constructor.

The problem with reflection still persists, though. Calling the private
constructor with a list of value objects, populated collections, and internal
values, it's easy to get the arguments wrong and end up with a runtime
exception. On the other hand, multiple public constructors, one of which must
only be called internally by the database layer, is confusing to clients.

Adding an extra constructor, public or private, doesn't violate clean
architecture per se. What clean architecture states is that the domain must be
persistence ignorant, i.e., not dependent on any data store. It doesn't preclude
a domain object from knowing when it's being reconstructed. It's no different
from EF forcing the class to implement private parameter-less constructor.

Still, with value objects reconstruction, EF has a performance edge. EF
constructs these using the same parameter-less private constructor and reflection
approach. It means their values are assumed to be valid on construction.
Validation code in the constructor isn't run, which yields better performance.

As with entities, we could add a private constructor and invoke it using the
reflection, but it probably isn't worth it. Most value objects are thin wrappers
around primitive types and don't undergo state changes over their lifetime. The
public constructor enables setting its complete value, with the downside of
re-running validation code.

*Q: Without automatic change tracking in Dapper, if we have an `Order` with
`OrderLines`, what would it take to track changes?*

It may be up to repository code to track changes and generate relevant SQL
statements. For instance, loading an initial `Order` with `OrderLines`, after
modification the database layer must infer which entities were deleted, added,
or updated, and possibly which properties were updated.

To determine if individual properties of an `Order` or `OrderLine` ware modified
adds significant boilerplate code. One option would be taking advantage of C#
records and their compiler implementation object comparisons. However, we'd have
to load the original data from the database once more before doing the
comparison.

Alternatively, we can manually implement the unit of work pattern. Application
logic would manually indicate modifications, additions, and deletions across any
number of aggregates (multiple aggregates may have changed as the result of code
reacting to published domain events). Imagine the unit of work implementation
maintaining lists of modified, added, and deleted entities and a save operation
itertating those collections. The downside is that in command handlers, we'd
have to replicate and keep in sync what's going on inside a domain object when
calling methods on it. In the command handler, we'd update the unit of work,
leading to possible bugs as the two divert.

Rather than manually keeping the unit of work up-to-date, a better approach
might be taking advantage of the domain events emitted as an aggregate undergoes
state changes. Next, the repository save operation would iterate the list of
domain events. From it, surgical SQL queries may be constructed. In this regard,
the storage layer has become a consumer of domain events similar to any other
internal consumer. Or we could add a different type of events, called storage
events.

Now suppose the database isn't the bottleneck -- that network latency isn't an
issue. Then the database can track in-progress, per-request changes without
violating clean architecture principles. When a client request comes in, the
application starts a snapshot transaction and does its work to process the
request. If any part of the snapshot is modified by another transaction, the
current transaction would fail on commit. Queries, perhaps triggered by domain
event processing during the same request, would read up-to-date data from the
snapshot, even though it isn't yet committed.

*Q: How to set system properties on each record with little repetition?*

With `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedBy` fields on each domain
entity, setting their values would be the responsibility of a repository. Unlike
with EF, we can't maintain these fields from a single implementation. Without a
change tracking unit of work, the equivalent of `DbContext`, in a single place,
we don't know which objects were modified, added, or deleted.

## Conclusion

A central theme with Dapper is how it maps results onto types. Given the above
reflection constraints, Dapper is unable to directly construct entities. We'd
have to create intermediate types for Dapper to map onto and then map those
types onto the domain model. This adds more boilerplate and memory use to the
database layer.

Dapper appears best suited for cases where a database already exists and EF
mappings would be tricky and/or for (non-)clean architecture solutions where
we'd want maximum control over queries.

In the approaches above, persistence is made harder because we're going for
one-to-one correspondence with EF features, but with more explicit tooling.
We're attempting to mimic EF's client side, in-memory state tracking and to
minimize the number of database interactions. The price EF's pays is that it
must keep a copy of the original objects to perform efficient change track.

Also, keep in mind the cognitive overhead of EF over the lifetime of an
application and how updates to EF might affect an application. Sometimes, it's
better to be explicit and a little repetitive in the database layer and not an
ORM.

## References

- [How to implement Unit Of Work pattern with Dapper?
](https://stackoverflow.com/questions/31298235/how-to-implement-unit-of-work-pattern-with-dapper/45029588)
