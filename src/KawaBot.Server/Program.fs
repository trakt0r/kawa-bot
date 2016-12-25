namespace KawaBot.Server

open System
open System.Net
open HealthController
open Http

module Program =
    [<EntryPoint>]
    let main argv =
        let listener = new HttpListener("*", 50000)
        listener.Register(HealthController.GetUrlHandler())
        listener.Run()
        Console.ReadKey true |> ignore
        listener.Stop()
        0
