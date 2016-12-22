namespace KawaBot.Server

open System

[<AutoOpen>]
module Utils =
    let ToDateTimeString (date: DateTime) = date.ToString("dd/MM/yyyy HH:mm:ss:fffff")
