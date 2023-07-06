open FsToolkit.ErrorHandling

// Helper

type ValidationError = { Field: string; Message: string }
module ValidationError =
    let create field message =
        { Field = field; Message = message }

// Value objects

type PersonId = PersonId of int
module PersonId =
    let validate id =
        Ok (PersonId id)
    let value (PersonId id) = id

type Name = Name of string
module Name =
    let validate field name =
        if name = "John" then
            Error (ValidationError.create field "Too short")
        else
            Ok (Name name)
    let value (Name s) = s

type Age = Age of int
module Age =
    let validate field age =
        if age = 42 then
            Error (ValidationError.create field "Too young")
        else
            Ok (Age age)
    let value (Age n) = n

// Entity

[<NoEquality; NoComparison>]
type PersonAggregateRoot = { Id: PersonId; Name: Name; Age: Age }

type PersonAggregateBusinessError =
    | Blah of string

module PersonAggregateRoot =
    let create id name age =
        if Name.value name = "Ronnie" then
            Error (Blah "Random business error")
        else
            Ok { Id = id; Name = name; Age = age }

// Command

type CreatePersonCommand = { Name: string; Age: int }

type PersonDto = { Id: int; Name: string; Age: int }
module PersonDto =
    let ofPerson (p: PersonAggregateRoot) =
        { Id = PersonId.value p.Id; Name = Name.value p.Name; Age = Age.value p.Age }

// These errors may be used to infer the HTTP status code of the request.
type CreatePersonCommandError =
    | ValidationErrors of ValidationError list
    | BusinessError of PersonAggregateBusinessError
    // Data store error because person already exist
    // Other external system error because of some creation check

let run (c: CreatePersonCommand) =
    validation {
        let! id = PersonId.validate 1
        and! name = Name.validate (nameof(c.Name)) c.Name
        and! age = Age.validate (nameof(c.Age)) c.Age
        return id, name, age
    }
    |> Result.mapError ValidationErrors
    |> Result.bind (fun (id, name, age) ->
        PersonAggregateRoot.create id name age
        |> Result.mapError BusinessError
        |> Result.map PersonDto.ofPerson)

// Runner

let r1 = run { Name = "John"; Age = 42 }
let r2 = run { Name = "Ronnie"; Age = 69 }
let r3 = run { Name = "Jane"; Age = 50 }

printfn $"%A{r1}"
printfn $"%A{r2}"
printfn $"%A{r3}"

// Output:
// Error (ValidationErrors [{ Field = "Name"
//                            Message = "Too short" }; { Field = "Age"
//                                                       Message = "Too young" }])
// Error (BusinessError (Blah "Random business error"))
// Ok { Id = 1
//      Name = "Jane"
//      Age = 50 }