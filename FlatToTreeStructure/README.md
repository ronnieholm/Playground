# FlatToTreeStructure

*Work in progress.*

## Problem

A tree may be represented as a table or as an object hierarchy.

This program experiments with converting from a table to an object hierarchy.
This problems shows up in transforming an SQL result set into an object
hierarchy.

In the table below, repetition is indicated by leaving out duplicate values.
Imagine the table rotated 90 degrees clock-wise:

```text
a_id | b_id | c_id
------------------
 a_1 |  b_1 |  c_1
     |      |  c_2
     |  b_2 |  c_3
     |      |  c_4
```

Assumptions are that

- 1. Ids are unique
- 2. the result is an object hierarchy, not a graph
- 3. rows are sorted

The equivalent table is:

```text
a_1
  b_1
    c_1
    c_2
  b_2
    c_3
    c_4
```

Transforming, or evaluating the table (seen as a parsing problem), to a typed
object hierarchy can be done recursively as a post-order traversal or
imperatively by looping the rows.

For hierarchy level n + 1, we need its vertical bounds, represented by `+`s
below. Vertically determining where a level ends adds to the assumptions that

- 4. Look-ahead is required with ability to independently address cells in the
     table

With sequential access only, recursive traversal becomes hard:

```text
a_id | b_id | c_id
------------------
++++++++++++++++++
 a_1 |  b_1 |  c_1
     |      |  c_2
     |++++++++++++
     |  b_2 |  c_3
     |      |  c_4
     |++++++++++++
++++++++++++++++++
```

Observe how the `+`s occur when Ids of non-leaf nodes change.

Evaluating the table would be an example of the interpreter pattern and `a_id`,
`b_id`, `c_id` hints to separate evaluation functions.

For an ADO.NET result set, assumptions (3) and (4) don't hold. We don't want to
to first process the result set, reading rows into another structure. That would
double the amount of storage needed and require more code.

Also, in most cases the SQL query for retrieving collections of entities below
the root will consist of left joins, not inner joins. An entity will typically
have a collection of zero or more entities, leading to a result set with empty
cells.

## Alternatives

Multiple result sets could potentially simplify the parsing problem by splitting
it into multiple smaller problems. However, this would only work for two-level
aggregates where the second query could use the primary key of the parent entity
(the aggregate root). Once we get to three levels or more, we (1) no longer know
the parent Id to query for and (2) there may be multiple Ids to query for.

Whether one large query with joins or multiple queries passed as a single query
is faster is impossible to tell upfront. The difference might be negligable for
the problem at hand.

## Implementation

As (3) and (4) don't hold true for ADO.NET, an implementation should work
without. Random ordering of rows imply maintaining lists/maps of Ids already
encountered. Conceptually a row traversal cache, with each row plotting a path
through the hierarchy, potentially encounting new nodes along the way.

## See also

- https://blog.ploeh.dk/2023/09/18/do-orms-reduce-the-need-for-mapping
- Discussion of post: https://twitter.com/ploeh/status/1703782709422657939
