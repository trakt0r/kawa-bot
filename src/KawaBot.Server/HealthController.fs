namespace KawaBot.Server

open System.IO
open Http

module HealthController =
    let GetUrlHandler() =
        new UrlHandler("health", fun req resp -> async {
            use writer = new StreamWriter(resp.OutputStream)
            writer.Write("OK")
        })
