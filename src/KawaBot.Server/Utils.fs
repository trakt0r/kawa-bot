namespace KawaBot.Server

open System.Collections.Generic

[<AutoOpen>]
module Utils =
    let tryGet (dict: IReadOnlyDictionary<'K, 'V>) (key: 'K): option<'V> =
        match dict.TryGetValue(key) with
            | true, value -> Some(value)
            | _ -> None
