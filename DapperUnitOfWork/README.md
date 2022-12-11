# DapperUnitOfWork

This project researches how to implement data access in clean architecture using
Dapper (any non-ORM, really). Unlike with EF and its `DbSet<T>` repositories and
`DbContext` unit of work, neither are part of Dapper. Dapper is a thin object
mapper, not an ORM, and those must be build on top of Dapper.

Similarly, instead of EF Migration, `DbUp` or similar must be used to manage the
database schema.

## Prototype investigation

*Q: How much boilerplate code is required to use Dapper over EF?*

A significant amount of repetitive code is needed to implement repositories with
CRUD operations and to map database results to domain objects. The unit of work
pattern is fairly straightforward to implement. It comes in handy if we're
subscribing to domain events inside the application, if every aggregate root
maintains its last modified timestamp. For instance, whenever children change,
say an OrderLine, the Order must update as well. This means updating both Orders
and OrderLines tables in a single transaction.

*Q: In clean architecture, a domain class tends to have getters only, and is
reconstructed by EF using a private constructor and reflection. How to achieve a
similar result with Dapper?*

On first sight, reconstructing a domain object, we can't pass into its
constructor the values to reconstruct it. Because reconstructing from database
is different from creating the object through a command, where a domain event is
published.

We also can't add setters to the domain object as that would potentially break
object invariants. And to set a property value using reflection, the property
must be declared `(private) set` or `init`, or we have to use the internal
backing field name. With EF, reflection is used based on information from Fluent
API mappings. By definition a domain object is reconstructed from the database
in a valid state, so invariant checking may be bypassed. The same holds true for
`ValueObject`s which also have a private parameterless constructor and a private
`Value` property.

Handwriting reflection code to instantiate a domain object using a private
parameterless constructor and setting values is tedious and error-prone. As an
alternative, instantiating a domain object through its private constructor with
arguments doesn't work all that well. The private and public constructors may
end up having the exact same list of arguments which C# disallows.

We could add a marker argument, an argument with a default value only explicitly
set by Dapper, to the public constructor and get the best of both worlds: no
need for reflection and domain objects could be reconstructed using public
constructors. The compiler would catch type errors as the application evolves
and performance would be excellent. The marker argument should be an `enum`, not
a `bool`, value and should have a default value to hide the arguments from
commands. Repository logic should set the value explicitly which causes the
domain object to skip publishing of domain events.

Adding an extra argument doesn't violate clean architecture. What clean
architecture states is that the domain must remain persistence ignorant, i.e.,
not dependent on any data store. It doesn't preclude a domain object from
knowing when it's being reconstituted. It's no different than EF forcing a
private parameterless constructor.

*Q: Without automatic change tracking in Dapper, if we have an `Order` with
`OrderLines`, what would it take to track changes?*

It's up to repository code to change track and issue corresponding SQL
statements. For instance, loading and `OrderId` with `OrderLines` to determine
which `OrderLines` has been deleted, added, or updated. In general determining
if individual properties of an `Order` or `OrderLine` was modified adds
significant boilerplate code. One option would be taking advantage of records
and their compiler implementation object comparisons. However, then we'd have to
load the original data from the database once more before doing the comparison.

Alternatively, we can implement the unit of work pattern to simplify repository
implementations, and have the application manually indicate modifications,
additions, and deletions across any number of aggregates, e.g., multiple
aggregates may have changed as the result of subscribers to domain events. The
unit of work would know how to surgically modify, add, delete each entity with
possible child entities. `CreatedBy` and `CreatedAt` columns could also be
automatically set by the unit of work based on the nature of the change. The
downside is that in command handlers, we'd have to replicate and keep in sync
what's going on inside a domain object when calling methods on it. In the
command handler, we'd update the unit of work, leading to possible bugs as the
two change indepedently of each other.

Suppose the database isn't the bottleneck. Then the database could track changes
on behalf of the application without violating clean architecture principles.
When a request comes into the application, the application starts a snapshot
transaction and does its work. If any part of the snapshot is modified by
another transaction, the current transaction would automatically fail on commit.
Later get queries, perhaps triggered by domain event processing, would read
up-to-date data from the snapshot.

Rather than manually keeping the unit of work up-to-date, a better approach is
taking advantage of domain events emitted as an aggregate undergoes state
changes. Imagine a repository save operation receiving the current state of
aggregate(s) and the list of thin domain events.

The repository can surgically update the store (in whichever format it is) based
on the aggregate(s) and the thin domain events. Thin events contain only key
information, so aggregate(s) are required for field values. In this regard, the
storage layer has become a consumer of domain events similar to any other
(external) consumer, except it need not call back into the API to get the
current state of agregates based on the event.

This approach extends to registering a list of handlers per domain event type
and call each handler during the save operation. Each handler may futher add to
the list of domain events until all domain events are processed, and the
transaction committed. In addition, this approach ensures our domain events are
correct and complete, or we'd quickly learn about from the queries.

*Q: How to set system properties on each record without repetition?*

If we have `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt` fields on each
domain object, setting these become the responsibility of repositories. We can't
abstract setting these properties to the `DbContext` as with EF. Knowing which
objects in the `DbContext` are modified, added, or deleted would require a more
sophisticated change tracker.

## Conclusion

Dapper appears best suited for cases where a database already exists and EF
mappings would be tricky to get right. Or for applications that aren't clean
architecture based or where we want maximum control over queries. For a new
application which owns the database, EF seems the best option for the CRUD part.
On the query side, Dapper might still be an option. However, it's possible to
execute straight SQL though EF as well, so the benefits of Dapper may be
insignificant.

In the approaches above, persistence is made harder because we're going for
one-to-one correspondence with EF features, but with more explict tooling. We're
trying to mimick EF in-memory state tracking on the client, and to minimize the
number of database interactions to EF levels. But EF keeps a copy of original
objects to perform efficient change track.

## References

- [How to implement Unit Of Work pattern with Dapper?
](https://stackoverflow.com/questions/31298235/how-to-implement-unit-of-work-pattern-with-dapper/45029588)
