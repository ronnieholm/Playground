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
    // Database/DuplicatePerson error because person already exist
    // Other integration layer error    

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

let run2 (c: CreatePersonCommand) =
    // Same as run but combines result and validation computation expression for easier reading
    // as the steps involved increases.
    result {
        // FIX: Move validation logic to validate function on CreatePersonCommand module. 
        let c' = validation {
            let! id = PersonId.validate 1
            // FIX: The domain layer shouldn't know about field names. Instead
            // if an error is returned, the code below in application layer, which
            // knows about the field name, should map the returned error to one
            // including field name based on CreatePersonCommand.
            and! name = Name.validate (nameof(c.Name)) c.Name // |> Result.mapError withField (nameof(c.Name)) 
            and! age = Age.validate (nameof(c.Age)) c.Age // |> Result.mapError withField (nameof(c.Age))
            return id, name, age
        }
        let! id, name, age = c' |> Result.mapError ValidationErrors
        // In real-life, we'd likely call PersonRepository.get and fail if the person already
        // already exists. That would be an async operation, in which case we should use
        // taskResult computation expression instead.
        //do! PersonRepository.getAsync id |> Result.requireNone (DuplicatePerson id)
        let! person = PersonAggregateRoot.create id name age |> Result.mapError BusinessError
        // Persisting to database, another async operation, would happen here.
        // If persisting fails, it'll raise an exception, not return an error.
        return PersonDto.ofPerson person
    }

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