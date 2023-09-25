namespace FlatToTreeStructure

open System
open System.Collections.Generic
open System.Data.SQLite

[<AutoOpen>]
module Domain =
    // In an actual DDD app, we wouldn't modify an entity's collection (except
    // like EF through reflection or through a constructor).
    //
    // Disable generating comparison and equality code to mimic DDD entities. In
    // a real DDD app, these types would be defined in the domain.
    //
    // With the algorithm below, deserialization doesn't require internal mirror
    // types.
    [<NoComparison; NoEquality>]
    type C = { Id: string }

    [<NoComparison; NoEquality>]
    type B = { Id: string; Cs: C list }

    [<NoComparison; NoEquality>]
    type A = { Id: string; Bs: B list }

    type AId = string
    type BId = string
    type CId = string

module ParseResultSetIntoImmutableDomainObjects =
    // Visited paths from A -> B -> C
    let parsedCs = Dictionary<AId * BId, Dictionary<CId, C>>()

    let parseC (r: SQLiteDataReader) (aid: AId) (bid: BId) =
        let cid = r["c_id"]

        if cid <> DBNull.Value then
            let cid = string cid
            let ok, cs = parsedCs.TryGetValue((aid, bid))

            if not ok then
                // We haven't been on a path from A to B before.
                let cs = Dictionary<CId, C>()
                cs.Add(cid, { Id = cid })
                parsedCs.Add((aid, bid), cs)
            else
                // We have been on a path from A to B, but not this C.
                cs.Add(string cid, { Id = string cid })

    // Visited paths from A -> B.
    let parsedBs = Dictionary<AId, Dictionary<BId, B>>()

    let parseB (r: SQLiteDataReader) (aid: AId) =
        let bid = r["b_id"]

        if bid <> DBNull.Value then
            let bid = string bid
            let ok, bs = parsedBs.TryGetValue(aid)

            if not ok then
                // We haven't been on a path from A before.
                let bs = Dictionary<BId, B>()
                bs.Add(bid, { Id = bid; Cs = [] })
                parsedBs.Add(aid, bs)
            else
                // We have been on a path from A, but not to this B.
                let ok, _ = bs.TryGetValue(bid)

                if not ok then
                    bs.Add(bid, { Id = bid; Cs = [] })

            parseC r aid bid

    // If the SQL query isn't limited to a single Id of A, it results in a
    // virtual root with paths to multiple As, hence the parsedAs dictionary.
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

        // Transform mutable dictionaries of unique paths to an immutable
        // object hierarchy of As, Bs, Cs.
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

        let parsedAsImmutable = ParseResultSetIntoImmutableDomainObjects.parse reader
        Assert.Equal(1, parsedAsImmutable.Length)
