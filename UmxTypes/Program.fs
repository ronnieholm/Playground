open FSharp.UMX

module Name =
    [<Measure>] type name
    
    let create (s: string): Result<string<name>, string> =
        if s.Length > 10 then
            Error "Too long"
        else
            Ok %s
        
    let value (v: string<name>): string =
        %v

let n = Name.create "Ronnie"

let s =
    match n with
    | Ok n -> Name.value n
    | Error _ -> "Unknown"

// Notice how units of measure is erased at runtime
printfn $"%A{n} %A{s}"
