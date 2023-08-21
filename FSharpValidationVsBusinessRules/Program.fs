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
    let validate name =
        if name = "John" then
            Error "Too short"
        else
            Ok (Name name)
    let value (Name s) = s

type Age = Age of int
module Age =
    let validate age =
        if age = 42 then
            Error "Too young"
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
module CreatePersonCommand =
    let validate (c: CreatePersonCommand) =
        validation {
            let! id = PersonId.validate 1
            and! name = Name.validate c.Name |> Result.mapError (ValidationError.create (nameof(c.Name))) 
            and! age = Age.validate  c.Age |> Result.mapError (ValidationError.create (nameof(c.Age)))
            return id, name, age
        }

type PersonDto = { Id: int; Name: string; Age: int }
module PersonDto =
    let ofPerson (p: PersonAggregateRoot) =
        { Id = PersonId.value p.Id; Name = Name.value p.Name; Age = Age.value p.Age }

// These errors may be used to infer the HTTP status code of the request.
type CreatePersonCommandError =
    | ValidationErrors of ValidationError list
    | BusinessError of PersonAggregateBusinessError
    // Database/DuplicatePerson error because person already exist
    // Other integration layer error    

let run (c: CreatePersonCommand) =
    CreatePersonCommand.validate c
    |> Result.mapError ValidationErrors
    |> Result.bind (fun (id, name, age) ->
        PersonAggregateRoot.create id name age
        |> Result.mapError BusinessError
        |> Result.map PersonDto.ofPerson)

let run2 (c: CreatePersonCommand) =
    // Same as run but using result computation expression for easier reading as the number
    // of steps involved increases.
    result {
        let! id, name, age = CreatePersonCommand.validate c |> Result.mapError ValidationErrors
        // In real-life, we'd likely call PersonRepository.get and fail if the person already
        // already exists. That would be an async operation, in which case we should use the
        // taskResult computation expression instead of result.
        //do! PersonRepository.getAsync id |> Result.requireNone (DuplicatePerson id)
        let! person = PersonAggregateRoot.create id name age |> Result.mapError BusinessError
        // Persisting to database, another async operation, would happen here.
        // If persisting fails, it'll raise an exception, not return an error.
        return PersonDto.ofPerson person
    }

// Runner

let r1 = run2 { Name = "John"; Age = 42 }
let r2 = run2 { Name = "Ronnie"; Age = 69 }
let r3 = run2 { Name = "Jane"; Age = 50 }

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