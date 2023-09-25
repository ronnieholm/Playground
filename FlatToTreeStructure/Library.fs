namespace FlatToTreeStructure

open System
open System.Collections.Generic
open System.Data.SQLite

[<AutoOpen>]
module Domain =
    // In real-world DDD apps, we cannot just touch the internal collections of entities (except through reflection).
    // We might want to use an approach similar to the immutable one to instead generate collections to pass to
    // entity constructors. This doesn't solve the problem of inadvertently creating domain events unless we use
    // a special constructor overload during deserialization.

    // Disable generating comparison and equality code to mimic DDD entities. In a real DDD app, these
    // types would be defined in the domain. Deserialization doesn't require internal mirror types.
    [<NoComparison; NoEquality>]
    type C = { Id: string }

    [<NoComparison; NoEquality>]
    type B = { Id: string; Cs: C list }

    [<NoComparison; NoEquality>]
    type A = { Id: string; Bs: B list }

    type AId = string
    type BId = string
    type CId = string

module ParseResultSetIntoMutableDomainObjects =
    let parseC (r: SQLiteDataReader) : C option =
        // No dictionary needed as we're at a object hierarchy leaf where no object repeats.
        let id = r["c_id"]
        if id <> DBNull.Value then Some { Id = string id } else None

    // Here we assume b only has one parent (part of a one-to-may relationship). Had
    // b been had multiple parents (part of a many-to-many relationship), key would
    // have to be a key + b key, with a key passed into parseB.
    let parsedBs = Dictionary<string, B>()

    let parseB (r: SQLiteDataReader) : B option =
        let id = r["b_id"]

        if id <> DBNull.Value then
            let id = string id
            let ok, _ = parsedBs.TryGetValue(id)

            if not ok then
                parsedBs.Add(id, { Id = id; Cs = [] })

                // If b isn't there, c isn't either so nest parsing of c inside conditional.
                match parseC r with
                | Some c ->
                    let b = parsedBs[id]
                    parsedBs[id] <- { b with Cs = c :: b.Cs }
                | None -> ()

                // Whether we parsed c or not, return new b.
                Some parsedBs[id]
            else
                // We parsed b before, but now it may be with another c
                // BUG: As objects in F# are immutable, the updated b is invisible inside parsedAs.
                match parseC r with
                | Some c ->
                    let b = parsedBs[id]
                    parsedBs[id] <- { b with Cs = c :: b.Cs }
                | None -> ()

                None
        else
            None

    let parsedAs = Dictionary<string, A>()

    let parseA (r: SQLiteDataReader) : A =
        let id = string r["a_id"]
        let ok, _ = parsedAs.TryGetValue(id)

        if not ok then
            parsedAs.Add(id, { Id = id; Bs = [] })

        // As a is always there (top of object hierarchy), we always want to attempt to parse b, so outside conditional.
        match parseB r with
        | Some b ->
            let a = parsedAs[id]
            // If bs are later updated by parseB, we'll not catch the update due to F#'s immutability.
            // We need parseB to tell us if the Some is a new or updated item. If it's an updated item
            // we need to locate the b inside bs, remove it, and add a new b. Problem with this approach
            // is that lists in F# are linked-in list under the hood, so they have linear time complexity.
            //
            // F# has a set time, but problem here is that with DDD, the entities we add to this set has
            // their structural comparison disabled and they have no custom comparison defined. So we
            // cannot quickly locate the existing object and remove it.
            //
            // Could one possible solution be to maintain an additional dictionary of a key and bs?
            // Then when parsedAs dictionary is converted to a list, we patch up each a with its bs?
            // This would avoid creating a new instance of b with each new c as the new dictionary
            // would store bs as a mutable collection. Does this approach scale if add ds to the mix?
            parsedAs[id] <- { a with Bs = b :: a.Bs }
        | None -> ()

        parsedAs[id]

    let parse (reader: SQLiteDataReader) : A list =
        while reader.Read() do
            parseA reader |> ignore

        parsedAs.Values |> Seq.toList

module ParseResultSetIntoImmutableDomainObjects =
    // Visited paths from A -> B -> C
    let parsedCs = Dictionary<AId * BId, Dictionary<CId, C>>()

    let parseC (r: SQLiteDataReader) (aid: AId) (bid: BId) =
        let cid = r["c_id"]

        if cid <> DBNull.Value then
            let cid = string cid
            let ok, cs = parsedCs.TryGetValue((aid, bid))

            if not ok then
                let cs = Dictionary<CId, C>()
                cs.Add(cid, { Id = cid })
                parsedCs.Add((aid, bid), cs)
            else
                cs.Add(string cid, { Id = string cid })

    // Visited paths from A -> B.
    let parsedBs = Dictionary<AId, Dictionary<BId, B>>()

    let parseB (r: SQLiteDataReader) (aid: AId) =
        let bid = r["b_id"]

        if bid <> DBNull.Value then
            let bid = string bid
            let ok, bs = parsedBs.TryGetValue(aid)

            if not ok then
                // We haven't yet been on a path from this A yet.
                let bs = Dictionary<BId, B>()
                bs.Add(bid, { Id = bid; Cs = [] })
                parsedBs.Add(aid, bs)
            else
                // We have been on a path from the A, but not to this B.
                let ok, _ = bs.TryGetValue(bid)

                if not ok then
                    bs.Add(bid, { Id = bid; Cs = [] })

            parseC r aid bid

    // If the SQL query isn't limited to a single Id of A, it results in a virtual root
    // with paths to multiple As, hence the parsedAs dictionary.
    //
    // Visited paths from virtual root -> A.
    let parsedAs = Dictionary<AId, A>()

    let parseA (r: SQLiteDataReader) =
        let aid = string r["a_id"]
        let ok, _ = parsedAs.TryGetValue(aid)

        if not ok then
            parsedAs.Add(aid, { Id = aid; Bs = [] })

        parseB r aid

    let parse (reader: SQLiteDataReader) : A list =
        while reader.Read() do
            parseA reader

        // Transform mutable dictionaries into immutable As.
        parsedAs.Values
        |> Seq.map (fun a ->
            let bs =
                parsedBs[a.Id].Values
                |> Seq.map (fun b ->
                    let ok, cs = parsedCs.TryGetValue((a.Id, b.Id))

                    { b with
                        Cs = if not ok then [] else cs.Values |> Seq.toList })
                |> Seq.toList

            { a with Bs = bs })
        |> Seq.toList

open Xunit

type Tests() =
    [<Fact>]
    let Test () =
        let connection =
            new SQLiteConnection("URI=file:../../../flatToTreeStructure.sqlite")

        connection.Open()

        let sql =
            """select a.id a_id, b.id b_id, c.id c_id
               from a
               left join b on a.id = b.aid
               left join c on b.id = c.bid
               where a.id = @id"""

        use cmd = new SQLiteCommand(sql, connection)
        cmd.Parameters.AddWithValue("@id", "a1") |> ignore
        let reader = cmd.ExecuteReader()

        // SQL reader returns three rows in a one-to-many relationship:
        // a1,b1,c1
        // a1,b1,c2
        // a1,b2,DBNull

        // let parsedAsMutable = ParseResultSetIntoMutableDomainObjects.parse ()
        // Assert.Equal(1, parsedAsMutable.Length)

        let parsedAsImmutable = ParseResultSetIntoImmutableDomainObjects.parse reader
        Assert.Equal(1, parsedAsImmutable.Length)
