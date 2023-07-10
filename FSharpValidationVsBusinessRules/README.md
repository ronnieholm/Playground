# FSharpValidationVsBusinessRules

DDD example where in the command we want to first validate command input. Then
we want to create an entity from the validated input, with the creation
resulting in business error rather than validation errors.

Some examples of DDD applies the validations in the command/query by calling a
set of validate functions. With this approach we don't need an explicit type for
each value object, which means less code and fewer allocations. On the other
hand, now it's up to application layer code to call the correct validate
functions for each command/query. And if a type is repeated across
commands/queries, the set of validation functions must both be repeated as well.
So this approach moves responsibility of the domain layer.

## Links

- [What are Business Rules? It's not this](https://www.youtube.com/watch?v=FbYcIqVmGRk)
- [Validation and business rules](https://blog.ploeh.dk/2023/06/26/validation-and-business-rules)
