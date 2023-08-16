# FSharpValidationVsBusinessRules

DDD example where in the command we want to first validate its input. Then we
create an entity from the validated input. It all potentially results in a
validaton error or a business error.

Some examples of DDD apply the validations in the command/query by calling a set
of validate functions, e.g., [Grace](https://github.com/ScottArbeit/Grace)
version control system. This approach doesn't require an explicit type for each
value object, which means less code and fewer allocations. On the other hand,
now it's up to application layer code to call the correct validate functions for
each command/query property. And if a type is repeated across commands/queries,
the set of validation functions must be repeated as well. This approach moves
correct-by-construction responsibility out of the domain layer.

## Links

- [What are Business Rules? It's not this](https://www.youtube.com/watch?v=FbYcIqVmGRk)
- [Validation and business rules](https://blog.ploeh.dk/2023/06/26/validation-and-business-rules)
