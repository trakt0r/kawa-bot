namespace KawaBot.Server

open System

module Environment =
    let ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")
    let VerificationToken = Environment.GetEnvironmentVariable("VERIFICATION_TOKEN")
    let ClientID = Environment.GetEnvironmentVariable("CLIENT_ID")
    let mutable BotToken = ""