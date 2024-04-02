namespace FsCheckSample.FSharp

open System
open Xunit
open FsCheck
open FsCheck.FSharp
open FsCheck.Xunit

module Domain =
    type Name = private Name of string

    module Name =
        [<Literal>]
        let MinLength = 1

        [<Literal>]
        let MaxLength = 30

        let create (value: string) =
            if String.IsNullOrWhiteSpace(value) then
                raise (ArgumentException("Must not be null or whitespace."))

            match value.Length with
            | l when l < MinLength -> raise (ArgumentException($"Must be at least length {MinLength}."))
            | l when l > MaxLength -> raise (ArgumentException($"Must be less than or equal length {MaxLength}."))
            | _ -> Name(value)
            
        let value (Name name) = name 

    type Age = private Age of uint

    module Age =
        [<Literal>]
        let Min = 18u

        [<Literal>]
        let Max = 120u

        let create (value: uint) =
            match value with
            | v when v < Min -> raise (ArgumentException($"Must be at least age {Min}."))
            | v when v > Max -> raise (ArgumentException($"Must be less than or below age {Max}."))
            | _ -> Age(value)

        let value (Age age) = age
    
    type Person = { Name: Name; Age: Age }

    module Person =
        let create name age = { Name = name; Age = age }

open Domain

type Arbitraries () =
    static member names =
         ArbMap.defaults.ArbFor<NonWhiteSpaceString>()
         |> Arb.filter (fun s -> s.Get.Length >= Name.MinLength && s.Get.Length <= Name.MaxLength)
         |> Arb.convert (fun s -> Name.create s.Get) (fun s -> NonWhiteSpaceString (Name.value s))
                        
    static member ages =
        ArbMap.defaults.ArbFor<uint>()
        |> Arb.filter (fun a -> a >= Age.Min && a <= Age.Max)
        |> Arb.convert Age.create Age.value

[<assembly: Properties(Arbitrary = [| typeof<Arbitraries> |])>]
do ()

module PersonTests =
    [<Fact>]
    let ``must be valid example 1`` () =
        let valid (name: Name) (age: Age) = true
        Check.QuickThrowOnFailure valid
        
    [<Property>]
    let  ``must be valid example 2`` (name: Name) (age: Age) (person: Person) =
        ()
