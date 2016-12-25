namespace KawaBot.Server

open System
open System.Net
open HealthController
open Http

module Program =
    [<EntryPoint>]
    let main argv =
        let listener = new HttpListener("http://*:80/")
        let oac = OAuthController(HTTPRequester())
        listener.Register(HealthController.GetUrlHandler())
        listener.Register(SlackVerificationController.GetUrlHandler())
        listener.Register(oac.GetUrlHandler())
        listener.Run()
        Console.ReadKey true |> ignore
        listener.Stop()
        0
