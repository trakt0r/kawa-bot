namespace KawaBot.Server

open System
open System.Net
open HealthController
open Http

module Program =
    [<EntryPoint>]
    let main argv =
        let listener = new HttpListener("http://*:80/")
        listener.Register(HealthController.GetUrlHandler())
        listener.Run()
        Console.ReadKey true |> ignore
        0
