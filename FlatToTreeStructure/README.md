# FlatToTreeStructure

## Problem

A tree may be represented as table or object hierarchy/graph form.

This program experiments with converting a table to an object hierarchy. Such
problem shows up in many places, such as transforming an SQL result to an object
hierarchy.

In the table below, repeated values are left out to better show the hierarchical
nature of the data:

```text
a_id | b_id | c_id
------------------
 a_1 |  b_1 |  c_1
     |      |  c_2
     |  b_2 |  c_3
     |      |  c_4
```

Here assumptions are:

- 1. Ids are unique
- 2. Result is an object hierarchy, not a graph
- 3. Rows are sorted

The equivalent tree is:

```text
a_1
  b_1
    c_1
    c_2
  b_2
    c_3
    c_4
```

To transform, or evaluate, the table (seen as a parsing problem), to a typed
object hierarchy may be accomplished by a post-order traversal. For a sorted
table, this means going through rows top to bottom.

For hierarchy level n + 1, we can a priori determine its vertical bounds, i.e.,
where a level ends, provided an additional assumption:

- 4. Requier look-ahead by independently address cells in the table.

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

Observe how `+`s are present when Ids of non-leaf nodes change.

Transforming, or evaluating, a table with all assumptions met would be an
example of the interpreter pattern with `a_id`, `b_id`, `c_id` hinting at
separate evaluation functions.

For an ADO.NET result set, assumptions (3) and (4) don't hold. To pre-process
the result, reading rows into another structure, would be inefficient. It would
double the amount of storage needed and require more code for new types.

Also observe that in most cases, the SQL query to retrieve collections of
entities below the root will consist of left joins, not inner joins. An entity
will typically have a collection of zero or more entities, leading to a result
with empty cells.

## Alternatives

Multiple result could potentially simplify the parsing problem by splitting it
into smaller problems. This would only work for two-level aggregates where the
second query could use the primary key of the parent entity (the root). Once we
get to three levels or more, we (1) no longer know the parent Id to query for
and.

Whether one large query with joins or multiple smaller queries (passed as a
"single" query to the backend) is faster is impossible to tell. The difference
might be negligible for the problem at hand.

## Implementation

As (3) and (4) don't hold for ADO.NET, an algorithm should work around those.
Random ordering of rows imply maintaining maps of Ids. Conceptually, maps would
store previous paths taken through the object hierarchy, with each row being a
potentially new path.

## See also

- https://blog.ploeh.dk/2023/09/18/do-orms-reduce-the-need-for-mapping
- Discussion of post: https://twitter.com/ploeh/status/1703782709422657939
